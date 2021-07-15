using UnityEngine;
using UnityEngine.UI;

public class QuitButton : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Application.Quit);
    }
}