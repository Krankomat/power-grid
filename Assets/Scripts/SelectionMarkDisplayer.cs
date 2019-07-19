using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionMarkDisplayer : MonoBehaviour
{

    public GameObject selectionRing;
    public GameObject hoverRing; 
    public float rotationSpeed;

    public bool isSelected;
    public bool isHovered; 


    private void Awake()
    {
        Unhover();
        Deselect(); 
    }


    private void Update()
    {
        if (isHovered)
            hoverRing.transform.Rotate(new Vector3(0, 0, 1f * rotationSpeed * Time.deltaTime));
    }

    
    public void Hover()
    {
        isHovered = true;
        hoverRing.SetActive(true);
    }


    public void Unhover()
    {
        isHovered = false;
        hoverRing.SetActive(false);
    }


    public void Select()
    {
        isSelected = true;
        selectionRing.SetActive(true);
    }


    public void Deselect()
    {
        isSelected = false;
        selectionRing.SetActive(false);
    }

}
