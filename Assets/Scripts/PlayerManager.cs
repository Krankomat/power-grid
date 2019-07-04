using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    private RaycastHit hit;
    private Ray selectionRay;
    private GameObject hitColliderContainer; 
    private GameObject hitGameObject; 
    private GameObject selectedGameObject;
    private LayerMask selectionMask; 


    void Start()
    {
        selectionMask = LayerMask.GetMask("ObjectSelecting"); 
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MakeSingleSelectionRaycast(); 
        }
    }

    
    private void MakeSingleSelectionRaycast()
    {
        selectionRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        // Make raycast and return, if nothing was hit 
        if (!Physics.Raycast(selectionRay, out hit, Selector.SelectionRaycastMaxDistance, selectionMask))
            return; 

        hitColliderContainer = hit.collider.gameObject;
        hitGameObject = hitColliderContainer.transform.parent.gameObject;

        if (hitGameObject.GetComponent<Selector>() == null)
        {
            Debug.Log("Hit object has no Selector component!");
            return;
        }

        Debug.Log("Hit Object does have a selector component! ");

        hitGameObject.GetComponent<Selector>().ToggleSelection();
        
    }

}
