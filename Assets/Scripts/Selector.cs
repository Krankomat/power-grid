using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{

    public GameObject selectionMark; 
    private GameObject selectionMarkInstance; 
    

    void Start()
    {
        AttachSelectionMarkToParent(); 
    }


    // Attaches the selection mark to the parent game object 
    private void AttachSelectionMarkToParent()
    {
        selectionMarkInstance = Instantiate(selectionMark);
        selectionMarkInstance.transform.parent = gameObject.transform;
    }

}
