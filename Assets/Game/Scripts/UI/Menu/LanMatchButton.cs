using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
#pragma warning disable 618

public class LanMatchButton : MonoBehaviour
{
    string address;
    
    public string Address
    {
        get { return address; }
        set
        {
            address = value;
            GetComponentInChildren<Text>().text = address;
        }
    }
    
    public int Seed { get; set; }

    void Awake() => GetComponent<Button>().onClick.AddListener(OnClick);

    void OnClick()
    {
        GetComponentInParent<LanMatchmaker>().JoinMatch(this);
    }
}