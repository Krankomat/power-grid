using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    private RaycastHit hit;
    private Ray selectionRay;
    private GameObject hitGameObject; 
    private GameObject selectedGameObject;
    private LayerMask selectionMask; 


    void Start()
    {
        selectionMask = LayerMask.GetMask("ObjectSelecting"); 
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectionRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            selectedGameObject = GameObject.FindWithTag("Selectable");

            if (Physics.Raycast(selectionRay, out hit, Selector.SelectionRaycastMaxDistance, selectionMask)) 
            {
                hitGameObject = hit.collider.gameObject;

                Debug.Log(hitGameObject); 

                if (hitGameObject.GetComponent<Selector>() == null)
                {
                    Debug.Log("Hit object has no Selector component!");
                    return; 
                }

                Debug.Log("Hit Object does have a selector component! "); 

                hit.collider.transform.tag = "select";
            }
        }
    }
}
