using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionStateInMenuHandler : MonoBehaviour, IInteractionStateHandleable
{
    public MenuManager buildingMenu;

    public PlayerManager PlayerManager { get; set; }
    public bool IsActive { get; set; }

    public void Enter()
    {
        Debug.Log("INFO INTERACTION STATE: Entered InMenuHandler. ");
    }

    public void Exit()
    {
        Debug.Log("INFO INTERACTION STATE: Exited InMenuHandler. ");
    }

    public void HandleControls()
    {
        if (Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.E))
        {
            CloseBuildMenu(); 
            return; 
        }
    }

    public void Process()
    {
        // Nothing to process 
    }

    private void Start()
    {
        buildingMenu.OnMenuClose.AddListener(PlayerManager.ResetInteractionState);
    }

    public void OpenBuildMenu()
    {
        OpenMenu(buildingMenu);
    }


    public void CloseBuildMenu()
    {
        CloseMenu(buildingMenu);
    }

    private void OpenMenu(MenuManager menuManager)
    {
        menuManager.ShowMenu();
    }

    private void CloseMenu(MenuManager menuManager)
    {
        menuManager.HideMenu();
    }

}
