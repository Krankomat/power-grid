using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
    
    public const float SelectionRaycastMaxDistance = 100f; 

    public GameObject selectionMarkPrefab;
    public GameObject selectionColliderContainerPrefab; 

    private GameObject selectionMark;
    private GameObject selectionCollider;

    private bool isSelected; 


    private void Start()
    {
        AttachSelectionMarkToParent();
        AttachSelectionColliderToParent(); 
        selectionMark.SetActive(false); 
    }


    public void ToggleSelection()
    {
        if (isSelected)
            DeactivateSelection(); 
        else
            ActivateSelection();
    }


    // Attaches the selection mark to the parent game object 
    private void AttachSelectionMarkToParent()
    {
        selectionMark = Instantiate(selectionMarkPrefab);
        selectionMark.transform.SetParent(gameObject.transform); 
    }

    
    // Attaches a selection collider to the parent game object 
    private void AttachSelectionColliderToParent()
    {
        selectionCollider = Instantiate(selectionColliderContainerPrefab);
        selectionCollider.transform.SetParent(gameObject.transform);
    }


    private void ActivateSelection()
    {
        isSelected = true;
        selectionMark.SetActive(true);
        Debug.Log(gameObject + " was selected! "); 
    }


    private void DeactivateSelection()
    {
        isSelected = false;
        selectionMark.SetActive(false);
        Debug.Log(gameObject + " was deselected! ");
    }

}
