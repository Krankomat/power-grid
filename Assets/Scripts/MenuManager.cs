using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{

    public GameObject closeButton;

    private MenuState menuState; 


    private enum MenuState
    {
        Opened, 
        Closed
    }


    private void Start()
    {
        closeButton.GetComponent<Button>().onClick.AddListener(CloseMenu);
        CloseMenu(); 
    }


    public void ShowMenu()
    {
        gameObject.SetActive(true);
        menuState = MenuState.Opened;
    }


    public void CloseMenu()
    {
        gameObject.SetActive(false);
        menuState = MenuState.Closed; 
    }

}
