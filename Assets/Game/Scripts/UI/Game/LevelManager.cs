using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using UnityEngine;
using UnityEngine.Networking;

public class LevelManager : NetMessenger<LevelClient>
{
    public enum OpCode { RecipePicked, RecipeAssembled, RoundResult}
    public class StartPickMsg : MessageBase { }
    public class RecipeAssembledMsg : MessageBase { public int[] ingredients; public int recipeIndex; }
    public class RoundResultMsg : MessageBase { public int[] serverScores, clientScores; }
    
    static LevelManager instance;
    public static LevelManager Instance => instance ? instance : instance = FindObjectOfType<LevelManager>();
    public static List<Ingredient> Ingredients => Instance.ingredients;
    public static List<Nation> Nations => Instance.nations;
    public static List<Flavour> Flavours => Instance.flavours;
    public static List<Recipe> Recipes => Instance.recipes;

    bool showingIngredients = true;
    bool pickingRecipe = true;
    bool assemblingRecipe;
    int pickCount;

    [SerializeField] List<Flavour> flavours;
    [SerializeField] List<Recipe> recipes;
    [SerializeField] List<Nation> nations;
    [SerializeField] List<Ingredient> ingredients;
    [SerializeField] RecipesList recipesList;
    [SerializeField] IngredientPicker ingredientPicker;
    [SerializeField] PickedIngredientsList ingredientList;
    [SerializeField] ResultsWindow resultsWindow;
    [SerializeField] GameObject guestsInfo;
    [SerializeField] GuestInfo[] guests;
    [SerializeField] GameObject waitingModal;

    protected override short[] ClientMessagesToSend => new[] {(short) OpCode.RecipePicked, (short) OpCode.RecipeAssembled};
    protected override short[] ServerMessagesToSend => new[] {(short) OpCode.RoundResult};
    protected override short OpCodeOffset => 0;

    protected override void OnStartClient()
    {
        base.OnStartClient();
        ingredientPicker.OnStartPicking += OnStartIngredientPicking;
        ingredientPicker.OnFinishPicking += OnFinishIngredientPicking;
        ingredientList.OnFinishSwapping += OnFinishSwapping;
    }

    void OnEnable()
    {
        waitingModal.SetActive(true);
    }

    protected override void Update()
    {
        base.Update();
        
        if(!IsClient) return;

        if (pickingRecipe && (!IsServer || ConnectionsData.Count >= 2))
        {
            waitingModal.SetActive(false);
            recipesList.InputActive = true;

            if (Input.GetButtonDown("Jump"))
            {
                pickingRecipe = false;
                recipesList.InputActive = false;
                var pickMsg = new StartPickMsg();
                SendToServer((short) OpCode.RecipePicked, pickMsg);
                waitingModal.SetActive(true);
            }
        }

        if (assemblingRecipe)
        {
            if (Input.GetButtonDown("Left") || Input.GetButtonDown("Right"))
                SetIngredientsVisibility(!showingIngredients);
        }
    }

    void SetIngredientsVisibility(bool visibility)
    {
        showingIngredients = visibility;
        
        ingredientList.gameObject.SetActive(showingIngredients);
        ingredientList.InputActive = showingIngredients;
        
        guestsInfo.SetActive(!showingIngredients);
        recipesList.InputActive = !showingIngredients;
    }

    protected override void OnMessageFromClient(LevelClient cnn, short msgType, NetworkMessage msg)
    {
        if (msgType == (short) OpCode.RecipePicked)
        {
            pickCount++;
            if (pickCount < ClientsConnections.Count) return;
            pickCount = 0;
            ingredientPicker.StartRounds();
        }

        if (msgType == (short) OpCode.RecipeAssembled)
        {
            var assembledMsg = msg.ReadMessage<RecipeAssembledMsg>();
            OnAssembledMessage(cnn, assembledMsg);
        }
    }

    protected override void OnMessageFromServer(short msgType, NetworkMessage msg)
    {
        if (msgType == (short) OpCode.RoundResult)
        {
            var resultMsg = msg.ReadMessage<RoundResultMsg>();
            OnRoundResults(resultMsg);
        }
    }

    void OnStartIngredientPicking()
    {
        guestsInfo.gameObject.SetActive(false);
        waitingModal.SetActive(false);
    }

    void OnFinishIngredientPicking()
    {
        assemblingRecipe = true;
        SetIngredientsVisibility(true);
    }
    
    void OnFinishSwapping()
    {
        assemblingRecipe = false;
        var assembleMsg = new RecipeAssembledMsg();
        assembleMsg.ingredients = ingredientPicker.LocalPicked.ToArray();
        assembleMsg.recipeIndex = Recipes.IndexOf(recipesList.Current);
        SendToServer((short)OpCode.RecipeAssembled, assembleMsg);
        waitingModal.SetActive(true);
    }

    void OnAssembledMessage(LevelClient cnn, RecipeAssembledMsg msg)
    {
        //TODO: Validate client input
        var recipe = Recipes[msg.recipeIndex];
        cnn.scores = new int[guests.Length];
        for (var i = 0; i < guests.Length; i++)
            cnn.scores[i] = recipe.Score(msg.ingredients, guests[i]);

        if (ConnectionsData.FirstOrDefault(c => c.scores == null) != null) return;
        
        var roundResult = new RoundResultMsg();
        roundResult.serverScores = ConnectionsData.First(c => c.Connection.hostId < 0).scores;
        roundResult.clientScores = ConnectionsData.First(c => c.Connection.hostId >= 0).scores;
        SendToClients((short)OpCode.RoundResult, roundResult);
    }

    void OnRoundResults(RoundResultMsg result)
    {
        waitingModal.SetActive(false);

        if (IsServer)
            resultsWindow.Show(result.serverScores, result.clientScores);
        else
            resultsWindow.Show(result.clientScores, result.serverScores);
    }
}