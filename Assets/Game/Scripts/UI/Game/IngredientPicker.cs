using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = UnityEngine.Random;

#pragma warning disable 618

public class IngredientPicker : NetMessenger<IngredientClient>
{
    #region Definitions
    public enum MsgType : short { RoundReceived, RequestPick, PickResult, FinishPicking }
    public class RoundMsg : MessageBase { public int[] ingredientsIndexes; public int timeStamp; public int round; }
    public class PickMsg : MessageBase  { public int slotIndex; public int timeStamp; }
    public class PickResultMsg : MessageBase  { public bool success; public int index; }
    public class FinishPickingMsg : MessageBase  { }
    #endregion

    RoundMsg currentRound;
    int pickedSlotIndex;
    int currentRoundIndex = -1;
    float countDown = -1;
    bool pickEnabled;
    List<int> availableIndexes = new List<int>();

    RoundMsg CurrentRound => currentRound ?? (currentRound = new RoundMsg {ingredientsIndexes = new int[ingredientsSlots.Length]});
    
    [SerializeField] Image[] ingredientsSlots;
    [SerializeField] Text countDownLabel;
    [SerializeField] int rounds = 4;
    [SerializeField] float countDownDuration = 3;

    protected override short[] ServerMessagesToSend => new[] {(short) MsgType.RoundReceived, (short) MsgType.PickResult, (short) MsgType.FinishPicking};
    protected override short[] ClientMessagesToSend => new[] {(short) MsgType.RequestPick};
    protected override short OpCodeOffset => 100;

    public List<int> LocalPicked { get; } = new List<int>();
    public event Action OnFinishPicking;
    public event Action OnStartPicking;

    #region Events

    void Awake()
    {
        base.Update();
        gameObject.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();
        PickingInput();
        PickingCountDown();
    }
    
    protected override void OnMessageFromClient(IngredientClient client, short msgType, NetworkMessage msg)
    {
        if (msgType == (short) MsgType.RequestPick) OnPickRequest(client, msg.ReadMessage<PickMsg>());
    }

    protected override void OnMessageFromServer(short msgType, NetworkMessage msg)
    {
        if (msgType == (short) MsgType.RoundReceived) OnRound(msg.ReadMessage<RoundMsg>());
        if (msgType == (short) MsgType.PickResult) OnPickResult(msg.ReadMessage<PickResultMsg>());
        if (msgType == (short) MsgType.FinishPicking) OnFinishPick();
    }
    #endregion

    #region Private Methods
    void PickingInput()
    {
        if (!IsClient || !pickEnabled) return;
        if(Input.GetButtonDown("Up")) PickItem(0);
        if(Input.GetButtonDown("Right")) PickItem(1);
        if(Input.GetButtonDown("Down")) PickItem(2);
        if(Input.GetButtonDown("Left")) PickItem(3);
    }
    
    void PickingCountDown()
    {
        if (countDown <= 0) return;
        countDown -= Time.deltaTime;
        countDownLabel.text = ((int) countDown + 1).ToString();

        if (countDown > 0) return;
        
        countDownLabel.gameObject.SetActive(false);
        for (var i = 0; i < CurrentRound.ingredientsIndexes.Length; i++)
        {
            var ingredientIndex = CurrentRound.ingredientsIndexes[i];
            var ingredient = LevelManager.Ingredients[ingredientIndex];
            
            ingredientsSlots[i].sprite = ingredient.Icon;
            ingredientsSlots[i].color = Color.white;
            ingredientsSlots[i].gameObject.SetActive(true);
            ingredientsSlots[i].GetComponent<Outline>().effectColor = ingredient.Flavour.Color;
        }

        pickEnabled = true;
    }
    #endregion
    
    #region Picking Flow
    
    public void StartRounds()
    {
        availableIndexes.Clear();
        for (var i = 0; i < LevelManager.Ingredients.Count; i++)
            availableIndexes.Add(i);
        
        gameObject.SetActive(true);
        currentRoundIndex = 0;
        SendRound();
    }

    void SendRound()
    {
        for (var i = 0; i < ingredientsSlots.Length; i++)
        {
            if(availableIndexes.Count <= 0)
                for (var j = 0; j < LevelManager.Ingredients.Count; j++)
                    availableIndexes.Add(j);
            
            var index = Random.Range(0, availableIndexes.Count);
            CurrentRound.ingredientsIndexes[i] = availableIndexes[index];
            availableIndexes.RemoveAt(index);
        }

        CurrentRound.timeStamp = NetworkTransport.GetNetworkTimestamp();
        CurrentRound.round = currentRoundIndex;
        SendToClients((short)MsgType.RoundReceived, CurrentRound);
        currentRoundIndex++;
    }
    
    void OnRound(RoundMsg msg)
    {
        gameObject.SetActive(true);
        currentRound = msg;

        if (currentRound.round == 0)
        {
            LocalPicked.Clear();
            OnStartPicking?.Invoke();
        }

        for (var i = 0; i < ingredientsSlots.Length; i++)
            ingredientsSlots[i].gameObject.SetActive(false);
        

        countDownLabel.gameObject.SetActive(true);

        countDown = countDownDuration - GetServerDelay(msg.timeStamp);
    }
    
    public void PickItem(int slotIndex)
    {
        if(!pickEnabled) return;
        pickEnabled = false;
        pickedSlotIndex = slotIndex;
        ingredientsSlots[pickedSlotIndex].color = Color.yellow;
        var p = new PickMsg {slotIndex = slotIndex, timeStamp = NetworkTransport.GetNetworkTimestamp()};
        SendToServer((short)MsgType.RequestPick, p);
    }

    void OnPickRequest(IngredientClient client, PickMsg msg)
    {
        //Save the picking options and times to check
        if (client.PickedSlotIndex >= 0) throw new Exception("A client tried to pick again in the same round");
        client.PickTime = Time.time - GetDelay(client.Connection, msg.timeStamp);
        client.PickedSlotIndex = msg.slotIndex;

        //If there is still connections without picking return
        if (ConnectionsData.FirstOrDefault(c => c.PickedSlotIndex < 0) != null) return;

        //Check collisions and who win
        for (var i = 0; i < ConnectionsData.Count; i++)
        {
            var currentCnn = ConnectionsData[i];

            for (var j = i + 1; j < ConnectionsData.Count; j++)
            {
                var otherCnn = ConnectionsData[j];
                if (currentCnn.PickedSlotIndex != otherCnn.PickedSlotIndex) continue;

                if (currentCnn.PickTime < otherCnn.PickTime)
                {
                    otherCnn.PickedSlotIndex = -1;
                }
                else
                {
                    currentCnn.PickedSlotIndex = -1;
                    break;
                }
            }

            if (currentCnn.PickedSuccessfully) continue;
            var result = new PickResultMsg();
            result.success = currentCnn.PickedSlotIndex >= 0;
            result.index = result.success ? currentRound.ingredientsIndexes[currentCnn.PickedSlotIndex] : -1;
            SendToClient(currentCnn.Connection, (short) MsgType.PickResult, result);
            currentCnn.PickedSuccessfully = result.success;
        }

        //If there is still connections without picking return
        if (ConnectionsData.FirstOrDefault(c => c.PickedSlotIndex < 0) != null) return;

        //Save and clear pick
        for (var i = 0; i < ConnectionsData.Count; i++)
        {
            var itemIndex = currentRound.ingredientsIndexes[ConnectionsData[i].PickedSlotIndex];
            ConnectionsData[i].ItemsIndexes.Add(itemIndex);
            ConnectionsData[i].PickedSlotIndex = -1;
            ConnectionsData[i].PickedSuccessfully = false;
        }

        //If there is no more rounds finish, else send next round of picking
        if (currentRoundIndex >= rounds)
        {
            var finishMsg = new FinishPickingMsg();
            SendToClients((short) MsgType.FinishPicking, finishMsg);
        }
        else
        {
            SendRound();
        }
    }

    void OnPickResult(PickResultMsg msg)
    {
        pickEnabled = !msg.success;
        ingredientsSlots[pickedSlotIndex].color = msg.success ? Color.green : Color.red;
        if (msg.success) LocalPicked.Add(msg.index);
    }

    void OnFinishPick()
    {
        gameObject.SetActive(false);
        OnFinishPicking?.Invoke();
    }
    
    #endregion
}