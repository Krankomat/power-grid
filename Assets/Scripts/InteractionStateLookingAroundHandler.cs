using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionStateLookingAroundHandler : MonoBehaviour, IInteractionStateHandleable
{
    public PlayerManager PlayerManager { get; set; }
    public bool IsActive { get; set; }

    //TODO: Should IsActive get set here? 
    public void Enter()
    {
        Debug.Log("INFO INTERACTION STATE: Entered LookingAroundHandler. "); 
    }

    public void Exit()
    {
        Debug.Log("INFO INTERACTION STATE: Exited LookingAroundHandler. ");
    }

    public void HandleControls()
    {
        // Select object with left mouse button 
        if (Input.GetMouseButtonDown(0))
        {
            if (PlayerManager.selectionHandler.selectedGameObject != null && PlayerManager.selectionHandler.hoveredGameObject == null)
                PlayerManager.selectionHandler.ClearSelection();
            else if (PlayerManager.selectionHandler.hoveredGameObject != null)
                PlayerManager.selectionHandler.HandleSelectionOf(PlayerManager.selectionHandler.hoveredGameObject);

            PlayerManager.RefreshSelectionInfoPanel(PlayerManager.selectionHandler);
            return; 
        }

        // Clear selection with escape key 
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (PlayerManager.selectionHandler.selectedGameObject != null)
            {
                PlayerManager.selectionHandler.ClearSelection();
                PlayerManager.RefreshSelectionInfoPanel(PlayerManager.selectionHandler);
                return; 
            }
        }

        // Open Build Menu 
        if (Input.GetKeyUp(KeyCode.E))
        {
            PlayerManager.OpenBuildMenu();
            return;
        }

        // Start demolishing 
        if (Input.GetKeyUp(KeyCode.R))
        {
            PlayerManager.StartDemolishingOnClick();
            Debug.Log("Start demolishing! ");
            return;
        }
    }

    public void Process()
    {
        if (!IsActive)
            return; 

        Debug.Log("Hello, this is the LookingAround Interaction Handler speaking! ");

        PlayerManager.selectionHandler.HandleHovering();
    }
}
