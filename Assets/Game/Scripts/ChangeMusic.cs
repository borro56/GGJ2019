using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMusic : MonoBehaviour
{
    [SerializeField] int index;

    void OnEnable()
    {
        MusicController.Instance.Play(index);
    }
}
