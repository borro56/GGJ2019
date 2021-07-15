using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = UnityEngine.Random;

#pragma warning disable 618

public class LanMatchmaker : MonoBehaviour
{
    bool server;
    NetworkDiscovery discovery;

    [SerializeField] LanMatchButton matchPrefab;
    [SerializeField] Transform matchesContainer;
    [SerializeField] Button createButton;
    [SerializeField] UnityEvent onStarted;
    [SerializeField] float startDelay;
    [SerializeField] NetworkManager manager;
    [SerializeField] GameObject waiting;

    void Awake()
    {
        discovery = manager.GetComponent<NetworkDiscovery>();
        createButton.onClick.AddListener(CreateMatch);
    }

    void OnEnable()
    {
        server = false;
        discovery.Initialize();
        discovery.StartAsClient();
        InvokeRepeating(nameof(RefreshMatches), 0, 2);
    }

    void OnDisable()
    {
        createButton.onClick.RemoveListener(CreateMatch);
        if(!server) discovery.StopBroadcast();
        CancelInvoke();
    }

    void RefreshMatches()
    {
        for (var i = 0; i < matchesContainer.childCount; i++)
            Destroy(matchesContainer.GetChild(i).gameObject);

        foreach (var key in discovery.broadcastsReceived.Keys)
        {
            var networkBroadcastResult = discovery.broadcastsReceived[key];
            var button = Instantiate(matchPrefab, matchesContainer);
            button.Address = key;
            button.Seed = int.Parse(BytesToString(networkBroadcastResult.broadcastData));
        }
    }

    void CreateMatch()
    {
        if(server) return;
        matchesContainer.gameObject.SetActive(false);
        waiting.SetActive(true);
        server = true;
        var seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(seed);
        discovery.broadcastData = seed.ToString();
        discovery.StopBroadcast();
        discovery.StartAsServer();
        manager.StartHost();
        CancelInvoke();
        Invoke(nameof(DelayedStay), startDelay);
    }

    public void JoinMatch(LanMatchButton button)
    {
        matchesContainer.gameObject.SetActive(false);
        waiting.SetActive(true);
        Random.InitState(button.Seed);
        manager.networkAddress = button.Address;
        manager.StartClient();
        CancelInvoke();
        Invoke(nameof(DelayedStay), startDelay);
    }
    void DelayedStay()
    {
        gameObject.SetActive(false);
        onStarted.Invoke();
    }
    
    static string BytesToString(byte[] bytes)
    {
        char[] chArray = new char[bytes.Length / 2];
        Buffer.BlockCopy((Array) bytes, 0, (Array) chArray, 0, bytes.Length);
        return new string(chArray);
    }
}