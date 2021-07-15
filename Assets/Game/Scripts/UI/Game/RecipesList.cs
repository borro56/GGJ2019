using System;
using UnityEngine;
using UnityEngine.UI;

public class RecipesList : MonoBehaviour
{
    [Serializable]
    public class IngredientSlot
    {
        public Image primaryIngredient;
        public Image secondaryIngredient;
        public Image flavourColor;
        public Text flavourName;
    }

    int currentIndex = 0;

    [SerializeField] Text recipeName;
    [SerializeField] Image recipeNationality;
    [SerializeField] IngredientSlot[] ingredientSlots;
    
    public Recipe Current { get; private set; }
    public bool InputActive { get; set; }

    void Awake()
    {
        SetRecipe(LevelManager.Recipes[currentIndex]);
    }

    void Update()
    {
        if(!InputActive) return;
        
        if (Input.GetButtonDown("Down"))
            currentIndex++;
        
        if (Input.GetButtonDown("Up"))
            currentIndex--;
        
        if (currentIndex < 0) currentIndex += LevelManager.Recipes.Count;
        currentIndex %= LevelManager.Recipes.Count;
        SetRecipe(LevelManager.Recipes[currentIndex]);
    }

    void SetRecipe(Recipe recipe)
    {
        Current = recipe;
        recipeName.text = recipe.name;
        recipeNationality.sprite = recipe.Nationality.Icon;

        for (var i = 0; i < recipe.Ingredients.Count; i++)
        {
            var ingredient = recipe.Ingredients[i];
            var slot = ingredientSlots[i];

            slot.flavourName.text = ingredient.Flavour.name;
            slot.flavourColor.color = ingredient.Flavour.Color;
            slot.primaryIngredient.sprite = ingredient.Icon;
            slot.secondaryIngredient.sprite = ingredient.Replacements[0].Icon;
        }
    }
}