using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Nation")]
public class Nation : ScriptableObject, IIcon
{
    [SerializeField] Sprite icon;
    [SerializeField] string[] maleNames;
    [SerializeField] string[] femaleNames;
    [SerializeField] string[] lastNames;
    [SerializeField] Sprite happyFace;
    [SerializeField] Sprite neutralFace;
    [SerializeField] Sprite sadFace;
    [SerializeField] AudioClip happySound;
    [SerializeField] AudioClip sadSound;

    public Sprite Icon => icon;
    public Sprite HappyFace => happyFace;
    public Sprite SadFace => sadFace;
    public Sprite NeutralFace => neutralFace;
    public AudioClip HappySound => happySound;
    public AudioClip SadSound => sadSound;

    public bool HasNames(Sex sex)
    {
        if (sex == Sex.Male && maleNames.Length > 0) return true;
        if (sex == Sex.Female && femaleNames.Length > 0) return true;
        return false;
    }

    public string GenerateFullName(Sex sex)
    {
        if (sex == Sex.Male)
            return maleNames.Random() + " " + lastNames.Random();
        
        if (sex == Sex.Female)
            return femaleNames.Random() + " " + lastNames.Random();
        
        throw new Exception("Unsupported sex");
    }
}