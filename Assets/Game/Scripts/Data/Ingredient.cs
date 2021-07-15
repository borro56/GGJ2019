using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Ingredient")]
public class Ingredient : ScriptableObject, IIcon
{
    [SerializeField] Sprite icon;
    [SerializeField] Flavour flavour;
    [SerializeField] Ingredient[] replacements;

    public Sprite Icon => icon;
    public Flavour Flavour => flavour;
    public Ingredient[] Replacements => replacements;
}