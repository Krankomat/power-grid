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

    // Bool and not enum, because both can be true at the same time 
    public bool IsSelected { get; private set; }
    public bool IsHovered { get; private set; }


    private void Start()
    {
        AttachSelectionMarkToParent();
        CheckIfSelectionColliderIsAttached(); 
    }


    public void ToggleSelection()
    {
        if (IsSelected)
            Deselect(); 
        else
            Select();
    }


    public void Select()
    {
        IsSelected = true;
        selectionMark.GetComponent<SelectionMarkDisplayer>().Select();
        Debug.Log(gameObject + " was selected! ");
    }


    public void Deselect()
    {
        IsSelected = false;
        selectionMark.GetComponent<SelectionMarkDisplayer>().Deselect();
        Debug.Log(gameObject + " was deselected! ");
    }


    public void Hover()
    {
        IsHovered = true;
        selectionMark.GetComponent<SelectionMarkDisplayer>().Hover();
    }


    public void Unhover()
    {
        IsHovered = false;
        selectionMark.GetComponent<SelectionMarkDisplayer>().Unhover();
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
