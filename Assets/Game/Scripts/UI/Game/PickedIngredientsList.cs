using System;
using System.Net.Mail;
using UnityEngine;
using UnityEngine.UI;

public class PickedIngredientsList : MonoBehaviour
{
    [Serializable]
    public class IngredientSlot
    {
        public Image icon;
        public Text name;
        public Text flavourName;
        public Image flavourColor;
        public Outline marked;
    }

    int currentIndex = 0;
    int swapIndex = -1;
    [SerializeField] IngredientPicker picker;
    [SerializeField] IngredientSlot[] ingredientSlots;
    [SerializeField] Outline readyMarked;

    public bool InputActive { get; set; } = true;
    public event Action OnFinishSwapping;
    
    void OnEnable()
    {
        InputActive = true;
        currentIndex = 0;
        RefreshItems();
    }

    void Update()
    {
        if (!InputActive) return;

        if (Input.GetButtonDown("Down"))
            MoveMarked(1);

        if (Input.GetButtonDown("Up"))
            MoveMarked(-1);

        if (Input.GetButtonDown("Jump"))
        {
            if (currentIndex == ingredientSlots.Length)
            {
                InputActive = false;
                OnFinishSwapping?.Invoke();
            }
            else
            {
                if (swapIndex == -1)
                {
                    swapIndex = currentIndex;
                    PaintSelection(swapIndex);
                }
                else
                {
                    var old = picker.LocalPicked[swapIndex];
                    picker.LocalPicked[swapIndex] = picker.LocalPicked[currentIndex];
                    picker.LocalPicked[currentIndex] = old;
                    RefreshItems();
                }
            }
        }
    }

    void RefreshItems()
    {
        swapIndex = -1;
        readyMarked.enabled = currentIndex == ingredientSlots.Length;
        
        for (var i = 0; i < picker.LocalPicked.Count; i++)
        {
            var ingredient = LevelManager.Ingredients[picker.LocalPicked[i]];
            
            var slot = ingredientSlots[i];
            slot.icon.sprite = ingredient.Icon;
            slot.name.text = ingredient.name;
            slot.flavourName.text = ingredient.Flavour.name;
            slot.flavourColor.color = ingredient.Flavour.Color;
            PaintSelection(i);
        }
    }

    void MoveMarked(int offset)
    {
        currentIndex += offset;
        if (currentIndex < 0) currentIndex = ingredientSlots.Length;
        currentIndex %= ingredientSlots.Length + 1;

        for (var i = 0; i < picker.LocalPicked.Count; i++)
            PaintSelection(i);
        
        readyMarked.enabled = currentIndex == ingredientSlots.Length;
    }

    void PaintSelection(int i)
    {
        var color = new Color(0, 0, 0, 0);
        if(i == currentIndex) color = Color.red;
        if(i == swapIndex) color = Color.yellow;
        ingredientSlots[i].marked.effectColor = color;
    }
}