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
    public ElectricNetworkManager electricNetworkManager;
    public SelectionHandler selectionHandler;
    public InteractionStateLookingAroundHandler lookingAroundHandler;
    public InteractionStatePlacingHandler placingHandler;
    public InteractionStateDemolishingHandler demolishingHandler;
    public InteractionStateInMenuHandler inMenuHandler;

    private List<IInteractionStateHandleable> interactionHandlers = new List<IInteractionStateHandleable>(); 

    private GameHUDDisplayer hudDisplayer;
    public InteractionState currentInteractionState;
    private InteractionState previousInteractionState; 
    private const InteractionState InteractionStateDefault = InteractionState.LookingAround;
    
    public InteractionStateEvent OnInteractionStateEntered; 
    public InteractionStateEvent OnInteractionStateLeft; 
    

    void Awake()
    {
        if (lookingAroundHandler != null)
            RegisterInteractionHandler(lookingAroundHandler);
        if (placingHandler != null)
            RegisterInteractionHandler(placingHandler);
        if (demolishingHandler != null)
            RegisterInteractionHandler(demolishingHandler);
        if (inMenuHandler != null)
            RegisterInteractionHandler(inMenuHandler);

        hudDisplayer = gameHUD.GetComponent<GameHUDDisplayer>();

        // Prepare default interaction state
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


    

    private void HandleControlsInInteractionState()
    {

        if (currentInteractionState == InteractionState.LookingAround)
        {
            return;
        }


        if (currentInteractionState == InteractionState.Placing)
        {
            return;
        }


        if (currentInteractionState == InteractionState.InMenu)
        {

            return;
        }

        if (currentInteractionState == InteractionState.Demolishing)
        {
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
                break;

            case InteractionState.InMenu:
                break;

            case InteractionState.Demolishing:
                break;

            default:
                Debug.LogError("Unsupported interaction state ");
                break; 
        }
        
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






    public void ResetInteractionState()
    {
        ChangeInteractionStateTo(InteractionStateDefault);
    }


    // Method to get a child with name childObjectName of gameObject 
    public static GameObject GetChildObject(GameObject gameObject, string childObjectName)
    {
        foreach (Transform childTransform in gameObject.transform)
            if (String.Equals(childTransform.gameObject.name, childObjectName))
                return childTransform.gameObject;

        return null;
    }

    public void StartPlacingGameObject(GameObject gameObjectPrefab)
    {
        ChangeInteractionStateTo(InteractionState.Placing);
        placingHandler.StartPlacingGameObject(gameObjectPrefab); 
    }


    public void StartDemolishingOnClick()
    {
        ChangeInteractionStateTo(InteractionState.Demolishing); 
    }


    public void StopDemolishingOnClick()
    {
        ResetInteractionState();
    }

    public void OpenBuildMenu()
    {
        ChangeInteractionStateTo(InteractionState.InMenu);
        inMenuHandler.OpenBuildMenu();
    }

        
    public void CloseBuildMenu()
    {
        inMenuHandler.CloseBuildMenu();
        ResetInteractionState();
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
            case InteractionState.Placing:
                placingHandler.Enter();
                placingHandler.IsActive = true;
                break;
            case InteractionState.Demolishing:
                demolishingHandler.Enter();
                demolishingHandler.IsActive = true;
                break;
            case InteractionState.InMenu:
                inMenuHandler.Enter();
                inMenuHandler.IsActive = true;
                break;
            default:
                Debug.LogError($"ERROR INTERACTION STATE: Unsupported state \"{interactionState.ToString("g")}\". ");
                break; 
        }
    }

}
