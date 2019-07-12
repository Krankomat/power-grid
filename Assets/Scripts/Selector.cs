using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
    
    public const float SelectionRaycastMaxDistance = 100f; 

    public GameObject selectionMarkPrefab;

    private GameObject selectionMark;
    private GameObject selectionCollider;

    private bool isSelected; 


    private void Start()
    {
        AttachSelectionMarkToParent();
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

}