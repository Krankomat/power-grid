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

    // Hover 
    private RaycastHit hit;
    private Ray hoverRay;
    private GameObject hoveredGameObject; 
    private GameObject hoveredColliderContainer; 
    private Selector hoveredSelector;

    // Selection 
    private LayerMask selectionMask;
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
    private const InteractionState InteractionStateDefault = InteractionState.Hovering; 

    
    private enum InteractionState
    {
        Hovering, 
        Placing, 
        InMenu 
    }


    void Start()
    {
        selectionMask = LayerMask.GetMask("ObjectSelecting");
        placingPreviewLayerMask = LayerMask.GetMask("ObjectPlacing"); 
        hudDisplayer = gameHUD.GetComponent<GameHUDDisplayer>();
        interactionState = InteractionState.Hovering;
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
        
        if (interactionState == InteractionState.Hovering)
        {
            // Check if there are button presses that will change the interaction state
            SwitchInteractionStateIfNecessary();

            // Select object with left mouse button 
            if (Input.GetMouseButtonDown(0))
            {
                if (selectedGameObject != null && hoveredGameObject == null)
                    ClearSelection(); 
                else if (hoveredGameObject != null) 
                    HandleSelectionOf(hoveredGameObject); 

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

        if (interactionState == InteractionState.Hovering)
        {
            MakeHoverRaycast(); 
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
        {
            return; 
        }


        // If not supported state 
        Debug.LogError("Unsupported interaction state ");
    } 


    private void MakeHoverRaycast()
    {
        hoverRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Make raycast and return, if nothing was hit 
        if (!Physics.Raycast(hoverRay, out hit, Selector.SelectionRaycastMaxDistance, selectionMask))
        {
            ClearHover(); 
            return;
        }

        hoveredColliderContainer = hit.collider.gameObject;
        hoveredGameObject = hoveredColliderContainer.transform.parent.gameObject;
        hoveredSelector = hoveredGameObject.GetComponent<Selector>();

        if (hoveredSelector == null)
            Debug.LogError("Hit object " + gameObject + " has no Selector component! "); 
    }


    private void ClearHover()
    {
        hoveredColliderContainer = null;
        hoveredGameObject = null;
        hoveredSelector = null;
    }


    private void HandleSelectionOf(GameObject gameObject)
    {
        // If there is no game object in selection so far 
        if (selectedGameObject == null)
            Select(gameObject);

        // If the selected object is already in selection, remove it from it 
        else if (selectedGameObject == gameObject)
            ClearSelection();
        
        // Else clear the selection and select the current game object 
        else
        {
            ClearSelection();
            Select(gameObject); 
        }
    }


    private void ClearSelection()
    {
        selectedGameObject.GetComponent<Selector>().Deselect();
        selectedGameObject = null; 
    }


    private void Select(GameObject gameObject)
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
