using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Recipe")]
public class Recipe : ScriptableObject
{
    [SerializeField] Nation nationality;
    [SerializeField] List<Ingredient> ingredients;

    public Nation Nationality => nationality;
    public List<Ingredient> Ingredients => ingredients;

    public int Score(int[] ingredientsIndexes, GuestInfo guest)
    {
        var score = 100;

        for (var i = 0; i < ingredientsIndexes.Length; i++)
        {
            var pickedIngredient = LevelManager.Ingredients[ingredientsIndexes[i]];
            var actualIngredient = ingredients[i];

            if (pickedIngredient.Flavour != actualIngredient.Flavour)
                score -= 25;
            else if (actualIngredient.Replacements.Contains(pickedIngredient))
                score -= 10;
            else
                score -= 15;

            if (nationality == guest.Nationality)
                score += 5;

            if (guest.Likes.Contains(pickedIngredient.Flavour))
                score += 15;
            
            if (guest.Dislikes.Contains(pickedIngredient.Flavour))
                score -= 15;
        }

        return score;
    }
}
