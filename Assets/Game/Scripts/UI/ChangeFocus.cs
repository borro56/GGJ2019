using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeFocus : MonoBehaviour
{
    [SerializeField] GameObject target;

    void Start()
    {
        StopAllCoroutines();
        StartCoroutine(Select());
    }

    IEnumerator Select()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(target);
    }
}