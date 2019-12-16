using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using CustomEvents;
using System.Linq;

public class PlayerManager : MonoBehaviour
{

    public GameObject gameHUD;
    public Vector2 gridCellDimensions;
    public MenuManager buildingMenu;
    public ElectricNetworkManager electricNetworkManager;
    public SelectionHandler selectionHandler;
    public InteractionStateLookingAroundHandler lookingAroundHandler;

    private List<IInteractionStateHandleable> interactionHandlers = new List<IInteractionStateHandleable>(); 

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
    private const InteractionState InteractionStateDefault = InteractionState.LookingAround;
    
    public InteractionStateEvent OnInteractionStateEntered; 
    public InteractionStateEvent OnInteractionStateLeft; 
    

    void Start()
    {
        if (lookingAroundHandler != null)
            RegisterInteractionHandler(lookingAroundHandler); 

        placingPreviewLayerMask = LayerMask.GetMask("ObjectPlacing");
        hudDisplayer = gameHUD.GetComponent<GameHUDDisplayer>();
        buildingMenu.OnMenuClose.AddListener(ResetInteractionState);

        // prepare default interactio state
        currentInteractionState = InteractionStateDefault;
        lookingAroundHandler.IsActive = true; 

        OnInteractionStateEntered.AddListener(hudDisplayer.DisplayStateIndicatorFor);
        OnInteractionStateEntered.AddListener(HandleInteractionStateEntered);
        OnInteractionStateLeft.AddListener(HandleInteractionStateLeft); 
    }


    void Update()
    {
        // Create this list before, so if another interactionHandler gets active, it has to wait for the next frame. 
        List<IInteractionStateHandleable> activeInteractionHandlers = 
            new List<IInteractionStateHandleable>(interactionHandlers.Where(handler => handler.IsActive)); 

        // If no handler is active (for some reason), go back to default interaction state 
        if (activeInteractionHandlers.Count == 0)
        {
            Debug.LogWarning($"WARNING INTERACTION STATE: For some reason, no interaction handler was active at start of frame. " +
                $"Resetting the interaction state, so the corresponding default interaction handler gets active. ");
            ResetInteractionState(); 
        }

        // Handle Controls and Process 
        foreach (IInteractionStateHandleable interactionHandler in activeInteractionHandlers)
        {
            interactionHandler.HandleControls();
            interactionHandler.Process();
        }

        // vvv Old way to handle interaction state and controls; will be replaced with handlers 
        HandleControlsInInteractionState();
        HandleCurrentInteractionState(); 

        //TODO: Move inside method ChangeInteractionStateTo() 
        if (currentInteractionState != previousInteractionState)
        {
            OnInteractionStateEntered.Invoke(currentInteractionState);
            OnInteractionStateLeft.Invoke(previousInteractionState); 
        }
        
        previousInteractionState = currentInteractionState; 
    }


    public void OpenBuildMenu()
    {
        OpenMenu(buildingMenu);
    }
    

    private void HandleControlsInInteractionState()
    {

        if (currentInteractionState == InteractionState.LookingAround)
        {
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
            case InteractionState.LookingAround:
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
            case InteractionState.LookingAround:
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
            case InteractionState.LookingAround:
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


    public void RefreshSelectionInfoPanel(SelectionHandler selectionHandler)
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

        if (!Physics.Raycast(placingPreviewRay, out placingPreviewHit, SelectionHandler.SelectionRaycastMaxDistance, placingPreviewLayerMask))
            return;

        placementPosition.x = MathUtil.SteppedNumber(placingPreviewHit.point.x + offset.x, gridCellDimensions.x) - offset.x; 
        placementPosition.z = MathUtil.SteppedNumber(placingPreviewHit.point.z + offset.z, gridCellDimensions.y) - offset.z;

        gameObjectToBePlaced.transform.position = placementPosition;
    }


    public void StartPlacingGameObject(GameObject gameObjectPrefab)
    {
        ChangeInteractionStateTo(InteractionState.Placing);
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
        ResetInteractionState();
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
        ResetInteractionState();
        modelDyer.ChangeMaterialsBackToInitial();
        gameObjectToBePlaced = null;
        modelDyer = null;
    }


    private void OpenMenu(MenuManager menuManager)
    {
        menuManager.ShowMenu();
        ChangeInteractionStateTo(InteractionState.InMenu);
    }


    public void CloseMenu(MenuManager menuManager)
    {
        menuManager.HideMenu();
        ResetInteractionState();
    }


    private void ResetInteractionState()
    {
        ChangeInteractionStateTo(InteractionStateDefault);
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


    public void StartDemolishingOnClick()
    {
        ChangeInteractionStateTo(InteractionState.Demolishing); 
    }


    private void StopDemolishingOnClick()
    {
        ResetInteractionState();
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


    private void RegisterInteractionHandler(IInteractionStateHandleable interactionHandler)
    {
        interactionHandlers.Add(interactionHandler);
        interactionHandler.PlayerManager = this; 
    }


    public void ChangeInteractionStateTo(InteractionState interactionState)
    {
        if (interactionState == currentInteractionState)
        {
            Debug.Log($"INFO INTERACTION STATE: State stayed the same: State {currentInteractionState.ToString("g")}. ");
            return;
        }
        ActivateCorrespondingInteractionHandler(interactionState);
        currentInteractionState = interactionState; 
    }
    

    private void ActivateCorrespondingInteractionHandler(InteractionState interactionState)
    {
        // Check how many interaction handlers are active 
        int numberOfActiveInteractionHandlers = interactionHandlers.Count(handler => handler.IsActive);
        if (numberOfActiveInteractionHandlers != 1)
            Debug.LogError($"ERROR INTERACTION STATE: When trying to change the current interaction state " +
                $"\"{currentInteractionState.ToString("g")}\" to the new state \"{interactionState.ToString("g")}\", a total of " +
                $"{numberOfActiveInteractionHandlers} Interaction Handlers were active, but this number should be 1. "); 

        // Deactivate currently active interaction Handler 
        foreach (IInteractionStateHandleable interactionHandler in interactionHandlers)
        {
            if (!interactionHandler.IsActive)
                continue;

            interactionHandler.Exit(); 
            interactionHandler.IsActive = false;
        }

        // Activate corresponding interaction handler 
        switch (interactionState)
        {
            case InteractionState.LookingAround:
                lookingAroundHandler.Enter(); 
                lookingAroundHandler.IsActive = true;
                break;
            //TODO: Implement missing handlers 
        }
    }

}
