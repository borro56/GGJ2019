using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Initializer : NetMessenger
{
    [SerializeField] UnityEvent onClient;

    protected override short OpCodeOffset => 0;

    void Update()
    {
        base.Update();
        if (IsClient)
        {
            onClient.Invoke();
            Destroy(this);
        }
    }
}
