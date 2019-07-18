using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionMarkDisplayer : MonoBehaviour
{

    public GameObject markingRing;
    public Sprite selectionRingShape;
    public Sprite hoverRingShape;
    public float rotationSpeed;
    public DisplayState currentState;

    private Color selectionColor;
    private Color hoverColor; 
    private Image image; 

    
    public enum DisplayState
    {
        Selecting, 
        Hovering
    }


    void Start()
    {
        image = markingRing.GetComponent<Image>(); 
        selectionColor = image.color;
        hoverColor = new Color(selectionColor.r, selectionColor.g, selectionColor.b, selectionColor.a / 2); 
    }


    void Update()
    {
        if (currentState == DisplayState.Selecting)
        {
            markingRing.transform.localRotation = Quaternion.identity;
            image.sprite = selectionRingShape;
            image.color = selectionColor;
            return; 
        }

        if (currentState == DisplayState.Hovering)
        {
            markingRing.transform.Rotate(new Vector3(0, 0, 1f * rotationSpeed *  Time.deltaTime));
            image.sprite = hoverRingShape;
            image.color = hoverColor;
            return; 
        }

        Debug.LogError("Unsupported display state in SelectionMarkDisplayer of object " + gameObject); 
    }

}
