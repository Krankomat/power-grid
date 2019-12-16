using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using CustomEvents; 

public class PlayerManager : MonoBehaviour
{

    public GameObject gameHUD;
    public Vector2 gridCellDimensions;
    public MenuManager buildingMenu;
    public ElectricNetworkManager electricNetworkManager;
    public SelectionHandler selectionHandler; 

    // Building Placement 
    private GameObject gameObjectToBePlaced;
    private ModelDyer modelDyer;
    private Ray placingPreviewRay;
    private RaycastHit placingPreviewHit;
    private LayerMask placingPreviewLayerMask;
    private Vector3 placementPosition;
    private CollisionHandler footprintCollisionHandler;
    private CollisionHandler electricCollisionHandler;


    // Demolishing 
    private Ray demolishingPreviewRay;
    private RaycastHit demolishingPreviewHit;
    private GameObject demolishingPreviewGameObject; 

    private GameHUDDisplayer hudDisplayer;
    private InteractionState currentInteractionState;
    private InteractionState previousInteractionState; 
    private const InteractionState InteractionStateDefault = InteractionState.Hovering;
    
    public InteractionStateEvent OnInteractionStateEntered; 
    public InteractionStateEvent OnInteractionStateLeft; 
    

    void Start()
    {
        placingPreviewLayerMask = LayerMask.GetMask("ObjectPlacing");
        hudDisplayer = gameHUD.GetComponent<GameHUDDisplayer>();
        currentInteractionState = InteractionState.Hovering;
        buildingMenu.OnMenuClose.AddListener(ResetInteractionState);
        
        OnInteractionStateEntered.AddListener(hudDisplayer.DisplayStateIndicatorFor);
        OnInteractionStateEntered.AddListener(HandleInteractionStateEntered);
        OnInteractionStateLeft.AddListener(HandleInteractionStateLeft); 
    }


    void Update()
    {
        HandleControlsInInteractionState();
        HandleCurrentInteractionState(); 

        if (currentInteractionState != previousInteractionState)
        {
            OnInteractionStateEntered.Invoke(currentInteractionState);
            OnInteractionStateLeft.Invoke(previousInteractionState); 
        }
        
        previousInteractionState = currentInteractionState; 
    }


    private void SwitchInteractionStateIfNecessary()
    {
        if (Input.GetKeyUp(KeyCode.E))
        {
            OpenMenu(buildingMenu);
            return; 
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            StartDemolishingOnClick();
            Debug.Log("Start demolishing! "); 
            return;
        }
    }


    private void HandleControlsInInteractionState()
    {

        if (currentInteractionState == InteractionState.Hovering)
        {
            // Check if there are button presses that will change the interaction state
            SwitchInteractionStateIfNecessary();

            // Select object with left mouse button 
            if (Input.GetMouseButtonDown(0))
            {
                if (selectionHandler.selectedGameObject != null && selectionHandler.hoveredGameObject == null)
                    selectionHandler.ClearSelection();
                else if (selectionHandler.hoveredGameObject != null)
                    selectionHandler.HandleSelectionOf(selectionHandler.hoveredGameObject);

                RefreshSelectionInfoPanel(selectionHandler);
            }

            // Clear selection with escape key 
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (selectionHandler.selectedGameObject != null)
                {
                    selectionHandler.ClearSelection();
                    RefreshSelectionInfoPanel(selectionHandler);
                }
            }

            return;
        }


        if (currentInteractionState == InteractionState.Placing)
        {

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                CancelPlacingGameObject();
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (footprintCollisionHandler.IsColliding())
                    HandleIntentToPlaceDownOnBlockedSpace();
                else
                    CompletePlacingGameObject();

                return;
            }


            return;
        }


        if (currentInteractionState == InteractionState.InMenu)
        {
            if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.E))
                CloseMenu(buildingMenu);

            return;
        }

        if (currentInteractionState == InteractionState.Demolishing)
        {
            if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.R))
                StopDemolishingOnClick();

            if (Input.GetMouseButtonDown(0))
                if (demolishingPreviewGameObject != null)
                    Demolish(demolishingPreviewGameObject); 
            
            return; 
        }

        // If not supported state 
        Debug.LogError("Unsupported interaction state ");

    }
    

    private void HandleCurrentInteractionState()
    {

        switch(currentInteractionState)
        {
            case InteractionState.Hovering:
                selectionHandler.HandleHovering();
                break;

            case InteractionState.Placing:
                HandlePlacing();
                break;

            case InteractionState.InMenu:
                break;

            case InteractionState.Demolishing:
                selectionHandler.HandleHovering(); 
                //HandleDemolishing();
                break;

            default:
                Debug.LogError("Unsupported interaction state ");
                break; 
        }
        
    }


    private void HandlePlacing()
    {
        MakePlacingPreviewRaycast();

        if (electricCollisionHandler == null)
            return;

        if (electricCollisionHandler.IsColliding())
            Debug.Log("The gameObject, which gets placed, is currently colliding! ");
    }


    private void MakeDemolishingPreview()
    {
        demolishingPreviewGameObject = selectionHandler.hoveredGameObject;
        demolishingPreviewGameObject.GetComponent<ModelDyer>().ChangeMaterialsToNegativeHover();
    }


    private void HideDemolishingPreview()
    {
        demolishingPreviewGameObject.GetComponent<ModelDyer>().ChangeMaterialsBackToInitial();
        demolishingPreviewGameObject = null; 
    }


    // Is called every time, the state changes 
    private void HandleInteractionStateEntered(InteractionState enteredState)
    {
        switch (enteredState)
        {
            case InteractionState.Hovering:
                // Used, when state is changed and display of hover is temporarily removed (?) 
                //TODO: Write method for SelectionHandler UpdateHover() 
                //if (hoveredSelector != null)
                //    hoveredSelector.Hover();
                break;

            case InteractionState.Placing:
            case InteractionState.InMenu:
                break;

            case InteractionState.Demolishing:
                selectionHandler.OnHoveringStart.AddListener(MakeDemolishingPreview);
                selectionHandler.OnHoveringEnd.AddListener(HideDemolishingPreview);
                break; 

            default:
                Debug.LogError("Unsupported interaction state ");
                break;
        }
    }


    private void HandleInteractionStateLeft(InteractionState lastState)
    {
        switch (lastState)
        {
            case InteractionState.Hovering:
                //TODO: Write method for SelectionHandler UpdateHover() 
                //if (hoveredSelector != null)
                //    hoveredSelector.Unhover();
                break;

            case InteractionState.Placing: 
            case InteractionState.InMenu: 
                break;

            case InteractionState.Demolishing:
                selectionHandler.OnHoveringStart.RemoveListener(MakeDemolishingPreview);
                selectionHandler.OnHoveringEnd.RemoveListener(HideDemolishingPreview);
                break; 

            default:
                Debug.LogError("Unsupported interaction state ");
                break; 
        }
    }


    private void RefreshSelectionInfoPanel(SelectionHandler selectionHandler)
    {
        string objectName, objectDescription;

        if (selectionHandler.selectedGameObject == null)
        {
            objectName = "";
            objectDescription = "";
        } else
        {
            objectName = selectionHandler.selectedGameObject.GetComponent<Descriptor>().objectName;
            objectDescription = selectionHandler.selectedGameObject.GetComponent<Descriptor>().description;
        }

        hudDisplayer.RefreshSelectionInfoPanel(objectName, objectDescription);
    }


    private void MakePlacingPreviewRaycast()
    {
        Vector2Int footprint = gameObjectToBePlaced.GetComponent<Descriptor>().footprint;
        Vector3 offset = GetFootprintOffsetFrom(footprint); 
        placingPreviewRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(placingPreviewRay, out placingPreviewHit, Selector.SelectionRaycastMaxDistance, placingPreviewLayerMask))
            return;

        placementPosition.x = MathUtil.SteppedNumber(placingPreviewHit.point.x + offset.x, gridCellDimensions.x) - offset.x; 
        placementPosition.z = MathUtil.SteppedNumber(placingPreviewHit.point.z + offset.z, gridCellDimensions.y) - offset.z;

        gameObjectToBePlaced.transform.position = placementPosition;
    }


    public void StartPlacingGameObject(GameObject gameObjectPrefab)
    {
        currentInteractionState = InteractionState.Placing;
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
        LinkElectricColliderToCablePreview(); 
    }


    public void CancelPlacingGameObject()
    {
        if (electricCollisionHandler != null)
        {
            electricCollisionHandler.GetComponent<Collider>().isTrigger = true;
            electricNetworkManager.ClearPreviewNetworkEdges();
        }

        UnlinkFootprintColliderHandlerToModelDyerMaterialChanging();
        currentInteractionState = InteractionStateDefault;
        Destroy(gameObjectToBePlaced);
        modelDyer = null;
    }


    public void CompletePlacingGameObject()
    {
        if (electricCollisionHandler != null)
        {
            ElectricNetworkConnector electricNetworkConnector = gameObjectToBePlaced.GetComponent<ElectricNetworkConnector>();

            UnlinkElectricColliderFromCablePreview(); 
            electricNetworkManager.ClearPreviewNetworkEdges();
            electricNetworkConnector.HandlePlacement(electricNetworkManager, electricCollisionHandler); 
        }

        UnlinkFootprintColliderHandlerToModelDyerMaterialChanging();
        currentInteractionState = InteractionStateDefault;
        modelDyer.ChangeMaterialsBackToInitial();
        gameObjectToBePlaced = null;
        modelDyer = null;
    }


    private void OpenMenu(MenuManager menuManager)
    {
        menuManager.ShowMenu();
        currentInteractionState = InteractionState.InMenu;
    }


    public void CloseMenu(MenuManager menuManager)
    {
        menuManager.HideMenu();
        ResetInteractionState();
    }


    private void ResetInteractionState()
    {
        currentInteractionState = InteractionStateDefault;
    }


    private static Vector3 GetFootprintOffsetFrom(Vector2Int footprint)
    {
        Vector3 offset = new Vector3();

        offset.x = ((float)(footprint.x % 2) / 2); 
        offset.z = ((float)(footprint.y % 2) / 2);

        return offset; 
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


    private void StartDemolishingOnClick()
    {
        currentInteractionState = InteractionState.Demolishing; 
    }


    private void StopDemolishingOnClick()
    {
        currentInteractionState = InteractionStateDefault;
    }
    

    private void Demolish(GameObject gameObject)
    {
        ElectricNetworkConnector electricNetworkConnector = gameObject.GetComponent<ElectricNetworkConnector>();
        
        if (electricNetworkConnector != null) { 
            electricNetworkConnector.HandleDemolishingBy(electricNetworkManager);
            return; 
        }

        Destroy(gameObject); 
    }


    private void LinkElectricColliderToCablePreview()
    {
        electricCollisionHandler.OnCollisionHandlerEnter.AddListener(UpdateCablePreview);
        electricCollisionHandler.OnCollisionHandlerExit.AddListener(UpdateCablePreview);
    }


    private void UnlinkElectricColliderFromCablePreview()
    {
        electricCollisionHandler.OnCollisionHandlerEnter.RemoveListener(UpdateCablePreview);
        electricCollisionHandler.OnCollisionHandlerExit.RemoveListener(UpdateCablePreview);
    }


    private void UpdateCablePreview()
    {
        ElectricNetworkConnector electricNetworkConnector = gameObjectToBePlaced.GetComponent<ElectricNetworkConnector>();

        // First, clear all preview cables 
        electricNetworkManager.ClearPreviewNetworkEdges(); 

        // Then add them for each electric node collider 
        //TODO: Due to refactoring this should probably only be called, when the Preview Updates (and not each tick) 
        electricNetworkConnector.ShowPlacementPreviewOfElectricNetworkNodeAddOn(electricNetworkManager, electricCollisionHandler); 
    }
}
