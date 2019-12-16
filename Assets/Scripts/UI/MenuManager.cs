using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{

    public GameObject closeButton;
    public UnityEvent OnMenuClose; 


    private void Start()
    {
        closeButton.GetComponent<Button>().onClick.AddListener(HideMenu);
        HideMenu(); 
    }


    public void ShowMenu()
    {
        gameObject.SetActive(true);
    }


    public void HideMenu()
    {
        gameObject.SetActive(false);
        OnMenuClose.Invoke(); 
    }

}
