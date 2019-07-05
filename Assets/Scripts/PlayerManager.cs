using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject gameHUD; 

    private RaycastHit hit;
    private Ray selectionRay;
    private GameObject hitColliderContainer; 
    private GameObject hitGameObject; 
    private LayerMask selectionMask;
    private Selector hitSelector;

    private GameHUDDisplayer hudDisplayer; 

    private GameObject selectedGameObject; 


    void Start()
    {
        selectionMask = LayerMask.GetMask("ObjectSelecting");
        hudDisplayer = gameHUD.GetComponent<GameHUDDisplayer>(); 
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MakeSingleSelectionRaycast(); 
            RefreshGameHUDContent(); 
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (selectedGameObject != null)
            {
                ClearSelection();
                RefreshGameHUDContent(); 
            }
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
        hitSelector = hitGameObject.GetComponent<Selector>(); 

        if (hitSelector == null)
        {
            Debug.Log("Hit object has no Selector component!");
            return;
        }

        Debug.Log("Hit Object does have a selector component! ");

        // If there is no game object in selection so far 
        if (selectedGameObject == null)
        {
            SelectGameObject(hitGameObject);
            return; 
        }

        // If the selected object is already in selection, remove it from it 
        if (hitGameObject == selectedGameObject)
        {
            ClearSelection();
            return; 
        }
        
        // Else clear the selection and select the current game object 
        ClearSelection();
        SelectGameObject(hitGameObject); 
        
    }


    private void ClearSelection()
    {
        selectedGameObject.GetComponent<Selector>().Deselect();
        selectedGameObject = null; 
    }


    private void SelectGameObject(GameObject gameObject)
    {
        gameObject.GetComponent<Selector>().Select();
        selectedGameObject = gameObject; 
    }


    private void RefreshGameHUDContent()
    {
        string objectName, objectDescription;

        if (selectedGameObject == null)
        {
            objectName = "";
            objectDescription = ""; 
        } else
        {
            objectName = selectedGameObject.GetComponent<Descriptor>().objectName; 
            objectDescription = selectedGameObject.GetComponent<Descriptor>().description;
        }

        hudDisplayer.RefreshContent(objectName, objectDescription); 
    }

}
