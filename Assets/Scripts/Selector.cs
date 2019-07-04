using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
    
    public const float SelectionRaycastMaxDistance = 100f; 

    public GameObject selectionMark; 
    private GameObject selectionMarkInstance;

    

    private void Start()
    {
        AttachSelectionMarkToParent();
        selectionMarkInstance.SetActive(false); 
    }


    // Attaches the selection mark to the parent game object 
    private void AttachSelectionMarkToParent()
    {
        selectionMarkInstance = Instantiate(selectionMark);
        selectionMarkInstance.transform.SetParent(gameObject.transform); 
    }

}
