using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject gameHUD;
    public GameObject selectableCubePrefab; 

    // Selection 
    private RaycastHit hit;
    private Ray selectionRay;
    private GameObject hitColliderContainer; 
    private GameObject hitGameObject; 
    private LayerMask selectionMask;
    private Selector hitSelector;
    private GameObject selectedGameObject;

    // Placement 
    private bool isPlacingSelectableCube;
    private GameObject selectableCubeToBePlaced;
    private Ray placingPreviewRay;
    private RaycastHit placingPreviewHit;
    private LayerMask placingPreviewLayerMask; 


    private GameHUDDisplayer hudDisplayer;
    private InteractionState interactionState;
    private const InteractionState InteractionStateDefault = InteractionState.Selecting; 


    private enum InteractionState
    {
        Selecting, 
        Placing 
    }


    void Start()
    {
        selectionMask = LayerMask.GetMask("ObjectSelecting");
        placingPreviewLayerMask = LayerMask.GetMask("ObjectPlacing"); 
        hudDisplayer = gameHUD.GetComponent<GameHUDDisplayer>();
        interactionState = InteractionState.Selecting; 
    }


    void Update()
    {
        SwitchInteractionStateIfNecessary(); 
        HandleControlsInInteractionState();
        HandleInteractionState(); 
    }


    private void SwitchInteractionStateIfNecessary()
    {
        // Place a new selectable cube with right mouse button 
        if (Input.GetKeyUp(KeyCode.E))
        {
            if (interactionState == InteractionState.Placing)
                CancelPlacingSelectableCube(); 
            else
                StartPlacingSelectableCube();
        }
    }


    private void HandleControlsInInteractionState()
    {
        
        if (interactionState == InteractionState.Selecting)
        {
            // Select object with left mouse button 
            if (Input.GetMouseButtonDown(0))
            {
                MakeSingleSelectionRaycast();
                RefreshGameHUDContent();
            }

            // Clear selection with escape key 
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (selectedGameObject != null)
                {
                    ClearSelection();
                    RefreshGameHUDContent();
                }
            }

            return; 
        }


        if (interactionState == InteractionState.Placing)
        {

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                CancelPlacingSelectableCube();
                return; 
            }

            if (Input.GetMouseButtonDown(0))
            {
                CompletePlacingSelectableCube();
                return; 
            }

            return; 
        }

        
        // If not supported state 
        Debug.LogError("Unsupported interaction state ");
                
    }


    private void HandleInteractionState()
    {

        if (interactionState == InteractionState.Selecting)
        {
            // don't do anything 
            return;
        }


        if (interactionState == InteractionState.Placing)
        {
            placingPreviewRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (!Physics.Raycast(placingPreviewRay, out placingPreviewHit, Selector.SelectionRaycastMaxDistance, placingPreviewLayerMask))
                return;

            selectableCubeToBePlaced.transform.position = placingPreviewHit.point;
            return;
        }


        // If not supported state 
        Debug.LogError("Unsupported interaction state ");
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


    private void StartPlacingSelectableCube()
    {
        interactionState = InteractionState.Placing; 
        selectableCubeToBePlaced = Instantiate(selectableCubePrefab);
        isPlacingSelectableCube = true; 
    }


    private void CancelPlacingSelectableCube()
    {
        interactionState = InteractionStateDefault;
        Destroy(selectableCubeToBePlaced); 
    }


    private void CompletePlacingSelectableCube()
    {
        interactionState = InteractionStateDefault;
        selectableCubeToBePlaced = null; 
    }

}
