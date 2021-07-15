using System.Collections.Generic;

public class IngredientClient : NetMessengerClient
{
    public int PickedSlotIndex { get; set; } = -1;
    public float PickTime { get; set; }
    public List<int> ItemsIndexes { get; } = new List<int>();
    public bool PickedSuccessfully { get; set; }
}