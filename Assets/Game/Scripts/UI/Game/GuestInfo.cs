using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class GuestInfo : MonoBehaviour
{
    [Serializable]
    public class FlavourSlot
    {
        public Text name;
        public Image like;
        public Image dislike;
    }
    
    [SerializeField] Image photo;
    [SerializeField] Image nation;
    [SerializeField] Text name;
    [SerializeField] FlavourSlot[] flavoursSlots;
    [SerializeField] int likeAmount;
    [SerializeField] int dislikeAmount;
    [SerializeField] List<Sex> availableSexs;
    
    public string Name { get; set; }
    public Nation Nationality { get; set; }
    public Sex Sex { get; set; }
    public List<Flavour> Dislikes { get; set; }
    public List<Flavour> Likes { get; set; }
    public bool Satisfied { get; set; }
    
    public void Generate()
    {
        Nationality = LevelManager.Nations.Random();
        Sex = availableSexs.Random();

        if (!Nationality.HasNames(Sex))
            Sex = Sex == Sex.Male ? Sex.Female : Sex.Male;
        
        Name = Nationality.GenerateFullName(Sex);
        Likes = LevelManager.Flavours.Random(likeAmount);
        Dislikes = LevelManager.Flavours.Random(dislikeAmount, Likes);

        photo.sprite = Nationality.NeutralFace;
        name.text = Name;
        nation.sprite = Nationality.Icon;
        for (var i = 0; i < LevelManager.Flavours.Count; i++)
        {
            flavoursSlots[i].like.gameObject.SetActive(false);
            flavoursSlots[i].dislike.gameObject.SetActive(false);
            flavoursSlots[i].name.text = LevelManager.Flavours[i].name;
        }

        for (var i = 0; i < Likes.Count; i++)
        {
            var flavour = Likes[i];
            var index = LevelManager.Flavours.IndexOf(flavour);
            flavoursSlots[index].like.gameObject.SetActive(true);
        }
        
        for (var i = 0; i < Dislikes.Count; i++)
        {
            var flavour = Dislikes[i];
            var index = LevelManager.Flavours.IndexOf(flavour);
            flavoursSlots[index].dislike.gameObject.SetActive(true);
        }
    }
}