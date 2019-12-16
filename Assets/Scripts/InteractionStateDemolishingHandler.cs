using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionStateDemolishingHandler : MonoBehaviour, IInteractionStateHandleable
{
    private Ray demolishingPreviewRay;
    private RaycastHit demolishingPreviewHit;
    private GameObject demolishingPreviewGameObject;

    public PlayerManager PlayerManager { get; set; }
    public bool IsActive { get; set; }

    public void Enter()
    {
        PlayerManager.selectionHandler.OnHoveringStart.AddListener(MakeDemolishingPreview);
        PlayerManager.selectionHandler.OnHoveringEnd.AddListener(HideDemolishingPreview);
        Debug.Log("INFO INTERACTION STATE: Entered DemolishingHandler. ");
    }

    public void Exit()
    {
        PlayerManager.selectionHandler.OnHoveringStart.RemoveListener(MakeDemolishingPreview);
        PlayerManager.selectionHandler.OnHoveringEnd.RemoveListener(HideDemolishingPreview);
        Debug.Log("INFO INTERACTION STATE: Exited DemolishingHandler. ");
    }

    public void HandleControls()
    {
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.R))
        {
            PlayerManager.StopDemolishingOnClick();
            return; 
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (demolishingPreviewGameObject != null)
                Demolish(demolishingPreviewGameObject);
            return; 
        }

    }

    public void Process()
    {
        PlayerManager.selectionHandler.HandleHovering();
    }

    private void Demolish(GameObject gameObject)
    {
        ElectricNetworkConnector electricNetworkConnector = gameObject.GetComponent<ElectricNetworkConnector>();

        if (electricNetworkConnector != null)
        {
            electricNetworkConnector.HandleDemolishingBy(PlayerManager.electricNetworkManager);
            return;
        }

        Destroy(gameObject);
    }


    private void MakeDemolishingPreview()
    {
        demolishingPreviewGameObject = PlayerManager.selectionHandler.hoveredGameObject;
        demolishingPreviewGameObject.GetComponent<ModelDyer>().ChangeMaterialsToNegativeHover();
    }


    private void HideDemolishingPreview()
    {
        demolishingPreviewGameObject.GetComponent<ModelDyer>().ChangeMaterialsBackToInitial();
        demolishingPreviewGameObject = null;
    }
}
