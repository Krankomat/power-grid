using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionStateLookingAroundHandler : MonoBehaviour, IInteractionStateHandleable
{
    public PlayerManager PlayerManager { get; set; }
    public bool IsActive { get; set; }

    public void Enter()
    {
        throw new System.NotImplementedException();
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }

    public void HandleControls()
    {
        throw new System.NotImplementedException();
    }

    public void Process()
    {
        if (IsActive)
            Debug.Log("Hello, this is the Interaction Handler speaking! "); 
        //if (IsActive)
        //    PlayerManager.selectionHandler.HandleHovering();
    }
}
