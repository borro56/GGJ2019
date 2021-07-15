using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Flavour")]
public class Flavour : ScriptableObject
{
    [SerializeField] Color color;

    public Color Color => color;
}