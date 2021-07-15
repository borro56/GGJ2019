using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuestGenerator : MonoBehaviour
{
    [SerializeField] GuestInfo[] info;

    void Awake()
    {
        for (int i = 0; i < info.Length; i++)
        {
            info[i].Generate();
        }
    }
}
