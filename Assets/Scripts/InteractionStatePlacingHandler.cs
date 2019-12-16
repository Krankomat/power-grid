using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionStatePlacingHandler : MonoBehaviour, IInteractionStateHandleable
{
    public Vector2 gridCellDimensions;

    private GameObject gameObjectToBePlaced;
    private ModelDyer modelDyer;
    private Ray placingPreviewRay;
    private RaycastHit placingPreviewHit;
    private LayerMask placingPreviewLayerMask;
    private Vector3 placementPosition;
    private CollisionHandler footprintCollisionHandler;
    private CollisionHandler electricCollisionHandler;

    public PlayerManager PlayerManager { get; set; }
    public bool IsActive { get; set; }

    private void Start()
    {
        placingPreviewLayerMask = LayerMask.GetMask("ObjectPlacing");
    }

    public void Enter()
    {
        Debug.Log("INFO INTERACTION STATE: Entered PlacingHandler. ");
    }

    public void Exit()
    {
        Debug.Log("INFO INTERACTION STATE: Exited PlacingHandler. ");
    }

    public void HandleControls()
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
    }

    public void Process()
    {
        HandlePlacing();
    }

    private void HandlePlacing()
    {
        MakePlacingPreviewRaycast();

        if (electricCollisionHandler == null)
            return;

        if (electricCollisionHandler.IsColliding())
            Debug.Log("The gameObject, which gets placed, is currently colliding! ");
    }

    public void StartPlacingGameObject(GameObject gameObjectPrefab)
    {
        PlayerManager.ChangeInteractionStateTo(InteractionState.Placing);
        gameObjectToBePlaced = GameObject.Instantiate(gameObjectPrefab);
        modelDyer = gameObjectToBePlaced.GetComponent<ModelDyer>();
        modelDyer.ChangeMaterialsToPositiveHover();

        GameObject footprintCollider = PlayerManager.GetChildObject(gameObjectToBePlaced, "FootprintCollider");
        footprintCollisionHandler = footprintCollider.GetComponent<CollisionHandler>();
        LinkFootprintColliderHandlerToModelDyerMaterialChanging();

        GameObject electricNetworkNodeCollider = PlayerManager.GetChildObject(gameObjectToBePlaced, "ElectricNetworkNodeCollider");

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
            PlayerManager.electricNetworkManager.ClearPreviewNetworkEdges();
        }

        UnlinkFootprintColliderHandlerToModelDyerMaterialChanging();
        PlayerManager.ResetInteractionState();
        Destroy(gameObjectToBePlaced);
        modelDyer = null;
    }


    public void CompletePlacingGameObject()
    {
        if (electricCollisionHandler != null)
        {
            ElectricNetworkConnector electricNetworkConnector = gameObjectToBePlaced.GetComponent<ElectricNetworkConnector>();

            UnlinkElectricColliderFromCablePreview();
            PlayerManager.electricNetworkManager.ClearPreviewNetworkEdges();
            electricNetworkConnector.HandlePlacement(PlayerManager.electricNetworkManager, electricCollisionHandler);
        }

        UnlinkFootprintColliderHandlerToModelDyerMaterialChanging();
        PlayerManager.ResetInteractionState();
        modelDyer.ChangeMaterialsBackToInitial();
        gameObjectToBePlaced = null;
        modelDyer = null;
    }

    private void HandleIntentToPlaceDownOnBlockedSpace()
    {
        Debug.Log("You cannot place " + gameObjectToBePlaced + " here. ");
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
        PlayerManager.electricNetworkManager.ClearPreviewNetworkEdges();

        // Then add them for each electric node collider 
        //TODO: Due to refactoring this should probably only be called, when the Preview Updates (and not each tick) 
        electricNetworkConnector.ShowPlacementPreviewOfElectricNetworkNodeAddOn(PlayerManager.electricNetworkManager, electricCollisionHandler);
    }


    private static Vector3 GetFootprintOffsetFrom(Vector2Int footprint)
    {
        Vector3 offset = new Vector3();

        offset.x = ((float)(footprint.x % 2) / 2);
        offset.z = ((float)(footprint.y % 2) / 2);

        return offset;
    }

}
