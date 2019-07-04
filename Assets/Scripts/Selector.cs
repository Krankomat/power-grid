using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
    
    public const float SelectionRaycastMaxDistance = 100f; 

    public GameObject selectionMark;
    public GameObject selectionColliderContainer; 

    private GameObject selectionMarkInstance;
    private GameObject selectionColliderInstance; 


    private void Start()
    {
        AttachSelectionMarkToParent();
        AttachSelectionColliderToParent(); 
        selectionMarkInstance.SetActive(false); 
    }


    // Attaches the selection mark to the parent game object 
    private void AttachSelectionMarkToParent()
    {
        selectionMarkInstance = Instantiate(selectionMark);
        selectionMarkInstance.transform.SetParent(gameObject.transform); 
    }

    
    // Attaches a selection collider to the parent game object 
    private void AttachSelectionColliderToParent()
    {
        selectionColliderInstance = Instantiate(selectionColliderContainer);
        selectionColliderInstance.transform.SetParent(gameObject.transform);
    }
}
