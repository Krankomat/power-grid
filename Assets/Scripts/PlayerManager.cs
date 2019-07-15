using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    
    public GameObject gameHUD;
    public Vector2 gridCellDimensions;
    public MenuManager buildingMenu;
    public ElectricNetworkManager electricNetworkManager; 

    // Selection 
    private RaycastHit hit;
    private Ray selectionRay;
    private GameObject hitColliderContainer; 
    private GameObject hitGameObject; 
    private LayerMask selectionMask;
    private Selector hitSelector;
    private GameObject selectedGameObject;

    // Building Placement 
    private GameObject gameObjectToBePlaced;
    private ModelDyer modelDyer;
    private Ray placingPreviewRay;
    private RaycastHit placingPreviewHit;
    private LayerMask placingPreviewLayerMask;
    private Vector3 placementPosition;
    private CollisionHandler footprintCollisionHandler;
    private CollisionHandler electricCollisionHandler; 


    private GameHUDDisplayer hudDisplayer;
    private InteractionState interactionState;
    private const InteractionState InteractionStateDefault = InteractionState.Selecting; 

    
    private enum InteractionState
    {
        Selecting, 
        Placing, 
        InMenu 
    }


    void Start()
    {
        selectionMask = LayerMask.GetMask("ObjectSelecting");
        placingPreviewLayerMask = LayerMask.GetMask("ObjectPlacing"); 
        hudDisplayer = gameHUD.GetComponent<GameHUDDisplayer>();
        interactionState = InteractionState.Selecting;
        buildingMenu.OnMenuClose.AddListener(ResetInteractionState); 
    }


    void Update()
    {
        HandleControlsInInteractionState();
        HandleInteractionState();
    }


    private void SwitchInteractionStateIfNecessary()
    {
        if (Input.GetKeyUp(KeyCode.E))
            OpenMenu(buildingMenu);
    }


    private void HandleControlsInInteractionState()
    {
        
        if (interactionState == InteractionState.Selecting)
        {
            // Check if there are button presses that will change the interaction state
            SwitchInteractionStateIfNecessary();

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
                CancelPlacingGameObject();
                return; 
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (footprintCollisionHandler.isColliding)
                    HandleIntentToPlaceDownOnBlockedSpace(); 
                else 
                    CompletePlacingGameObject();

                return; 
            }
            

            return; 
        }


        if (interactionState == InteractionState.InMenu)
        {
            if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.E)) 
                CloseMenu(buildingMenu);

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
            MakePlacingPreviewRaycast();

            if (electricCollisionHandler == null)
                return; 

            if (electricCollisionHandler.isColliding)
                Debug.Log("The gameObject, which gets placed, is currently colliding! "); 

            return;
        }


        if (interactionState == InteractionState.InMenu)
            return; 


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


    private void MakePlacingPreviewRaycast()
    {
        placingPreviewRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(placingPreviewRay, out placingPreviewHit, Selector.SelectionRaycastMaxDistance, placingPreviewLayerMask))
            return;

        placementPosition.x = MathUtil.SteppedNumber(placingPreviewHit.point.x, gridCellDimensions.x);
        placementPosition.z = MathUtil.SteppedNumber(placingPreviewHit.point.z, gridCellDimensions.y); 

        gameObjectToBePlaced.transform.position = placementPosition;
    }


    public void StartPlacingGameObject(GameObject gameObjectPrefab)
    {
        interactionState = InteractionState.Placing;
        gameObjectToBePlaced = GameObject.Instantiate(gameObjectPrefab);
        modelDyer = gameObjectToBePlaced.GetComponent<ModelDyer>();
        modelDyer.ChangeMaterialsToPositiveHover();

        GameObject footprintCollider = GetChildObject(gameObjectToBePlaced, "FootprintCollider");
        footprintCollisionHandler = footprintCollider.GetComponent<CollisionHandler>();
        LinkFootprintColliderHandlerToModelDyerMaterialChanging();

        GameObject electricNetworkNodeCollider = GetChildObject(gameObjectToBePlaced, "ElectricNetworkNodeCollider");

        // If there is no ElectricNetworkNodeCollider attached to the gameObject 
        if (electricNetworkNodeCollider == null)
            return; 

        electricCollisionHandler = electricNetworkNodeCollider.GetComponent<CollisionHandler>();
        electricCollisionHandler.colliderIntersectingIsCurrentlyActive = true; 
    }


    public void CancelPlacingGameObject()
    {
        if (electricCollisionHandler != null)
            electricCollisionHandler.colliderIntersectingIsCurrentlyActive = false;

        UnlinkFootprintColliderHandlerToModelDyerMaterialChanging(); 
        interactionState = InteractionStateDefault;
        Destroy(gameObjectToBePlaced);
        modelDyer = null;
    }

    public void CompletePlacingGameObject()
    {
        if (electricCollisionHandler != null)
        {
            ElectricNetworkConnector electricNetworkConnector = gameObjectToBePlaced.GetComponent<ElectricNetworkConnector>();
            electricNetworkManager.HandleElectricNetworkNodeAddOn(electricNetworkConnector, electricCollisionHandler); 
            electricCollisionHandler.colliderIntersectingIsCurrentlyActive = false;
        }

        UnlinkFootprintColliderHandlerToModelDyerMaterialChanging();
        interactionState = InteractionStateDefault;
        modelDyer.ChangeMaterialsBackToInitial(); 
        gameObjectToBePlaced = null;
        modelDyer = null;
    }


    private void OpenMenu(MenuManager menuManager)
    {
        menuManager.ShowMenu();
        interactionState = InteractionState.InMenu; 
    }


    public void CloseMenu(MenuManager menuManager)
    {
        menuManager.HideMenu();
        ResetInteractionState(); 
    }


    private void ResetInteractionState()
    {
        interactionState = InteractionStateDefault; 
    }


    // Method to get a child with name childObjectName of gameObject 
    public static GameObject GetChildObject(GameObject gameObject, string childObjectName)
    {
        foreach (Transform childTransform in gameObject.transform)
            if (String.Equals(childTransform.gameObject.name, childObjectName))
                return childTransform.gameObject;

        return null;
    }


    private void HandleIntentToPlaceDownOnBlockedSpace()
    {
        Debug.Log("You cannot place " + gameObjectToBePlaced + " here. "); 
    }


    private void LinkFootprintColliderHandlerToModelDyerMaterialChanging()
    {
        footprintCollisionHandler.OnCollisionHandlerEnter.AddListener(modelDyer.ChangeMaterialsToNegativeHover);
        footprintCollisionHandler.OnCollisionHandlerExit.AddListener(modelDyer.ChangeMaterialsToPositiveHover);
    }


    private void UnlinkFootprintColliderHandlerToModelDyerMaterialChanging()
    {
        footprintCollisionHandler.OnCollisionHandlerEnter.RemoveListener(modelDyer.ChangeMaterialsToNegativeHover);
        footprintCollisionHandler.OnCollisionHandlerExit.RemoveListener(modelDyer.ChangeMaterialsToPositiveHover);
    }

}
