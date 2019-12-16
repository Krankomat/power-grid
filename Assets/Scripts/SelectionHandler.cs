using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SelectionHandler : MonoBehaviour
{
    public GameObject hoveredGameObject;
    public GameObject selectedGameObject;

    public const float SelectionRaycastMaxDistance = 100f;
    private Selector hoveredSelector;
    private GameObject previouslyHoveredGameObject;
    private LayerMask selectionMask;

    public UnityEvent OnHoveringStart;
    public UnityEvent OnHoveringEnd;

    
    void Start()
    {
        selectionMask = LayerMask.GetMask("ObjectSelecting");
    }
    

    void Update()
    {
        previouslyHoveredGameObject = hoveredGameObject;
    }


    private void StartHover(Selector selector)
    {
        selector.Hover();
        OnHoveringStart.Invoke();
        Debug.Log($"INFO SELECTION: Hover started on object {selector.gameObject}. ");
    }


    private void EndHover(Selector selector)
    {
        selector.Unhover();
        OnHoveringEnd.Invoke();
        Debug.Log($"INFO SELECTION: Hover ended on object {selector.gameObject}. ");
    }


    public void HandleHovering()
    {
        MakeHoverRaycast(Input.mousePosition);

        // nothing hovered --> something hovered 
        if (hoveredGameObject != null &&
            previouslyHoveredGameObject == null)
        {
            StartHover(hoveredSelector); 
        }
        // something hovered --> nothing hovered 
        else if (hoveredGameObject == null &&
            previouslyHoveredGameObject != null)
        {
            EndHover(previouslyHoveredGameObject.GetComponent<Selector>());
        }
        // something hovered --> something else hovered 
        else if (hoveredGameObject != previouslyHoveredGameObject)
        {
            StartHover(hoveredSelector);
            EndHover(previouslyHoveredGameObject.GetComponent<Selector>());
        }

    }


    public void ClearHover()
    {
        hoveredGameObject = null;
        hoveredSelector = null;
    }


    public void HandleSelectionOf(GameObject gameObject)
    {
        // If there is no game object in selection so far 
        if (selectedGameObject == null)
            Select(gameObject);

        // If the selected object is already in selection, remove it from it 
        else if (selectedGameObject == gameObject)
            ClearSelection();

        // Else clear the selection and select the current game object 
        else
        {
            ClearSelection();
            Select(gameObject);
        }
    }


    private void MakeHoverRaycast(Vector3 inputPosition)
    {
        Ray hoverRay = Camera.main.ScreenPointToRay(inputPosition);
        RaycastHit hit;

        // Make raycast and return, if nothing was hit 
        if (!Physics.Raycast(hoverRay, out hit, SelectionRaycastMaxDistance, selectionMask))
        {
            ClearHover();
            return;
        }

        GameObject hoveredColliderContainer = hit.collider.gameObject;
        hoveredGameObject = hoveredColliderContainer.transform.parent.gameObject;
        hoveredSelector = hoveredGameObject.GetComponent<Selector>();

        if (hoveredSelector == null)
            Debug.LogError($"ERROR HOVERING: Hit object {gameObject} has no Selector component! ");
    }


    public void Select(GameObject gameObject)
    {
        gameObject.GetComponent<Selector>().Select();
        selectedGameObject = gameObject;
    }


    public void ClearSelection()
    {
        selectedGameObject.GetComponent<Selector>().Deselect();
        selectedGameObject = null;
    }

}
