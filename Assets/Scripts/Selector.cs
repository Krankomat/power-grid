using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
    
    public const float SelectionRaycastMaxDistance = 100f; 

    public GameObject selectionMarkPrefab;

    private GameObject selectionMark;
    private GameObject selectionCollider;
    private const string SelectionColliderName = "SelectionCollider"; 

    private bool isSelected; 


    private void Start()
    {
        AttachSelectionMarkToParent();
        CheckIfSelectionColliderIsAttached(); 
        selectionMark.SetActive(false); 
    }


    public void ToggleSelection()
    {
        if (isSelected)
            Deselect(); 
        else
            Select();
    }


    public void Select()
    {
        isSelected = true;
        selectionMark.SetActive(true);
        Debug.Log(gameObject + " was selected! ");
    }


    public void Deselect()
    {
        isSelected = false;
        selectionMark.SetActive(false);
        Debug.Log(gameObject + " was deselected! ");
    }


    // Attaches the selection mark to the parent game object 
    private void AttachSelectionMarkToParent()
    {
        selectionMark = Instantiate(selectionMarkPrefab, gameObject.transform, false);
    }


    // Checks, if the GameObject "SelectionCollider" is a child object 
    private void CheckIfSelectionColliderIsAttached()
    {
        foreach (Transform childTransform in transform)
        {
            if (String.Equals(childTransform.gameObject.name, SelectionColliderName))
                return; 
        }
        Debug.LogError("Selector Error: There is no gameObject named \"" 
                + SelectionColliderName + "\" attached to " + gameObject.name + "."); 
    }

}
