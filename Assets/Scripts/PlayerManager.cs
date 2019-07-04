using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    private RaycastHit hit;
    private Ray selectionRay;
    private GameObject hitGameObject; 
    private GameObject selectedGameObject; 

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectionRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            selectedGameObject = GameObject.FindWithTag("Selectable");

            if (Physics.Raycast(selectionRay, out hit, 100)) 
            {
                hitGameObject = hit.collider.gameObject; 

                if (hitGameObject.GetComponent<Selector>() == null)
                {
                    Debug.Log("Hit object has no Selector component!");
                    return; 
                }

                hit.collider.transform.tag = "select";
            }
        }
    }
}
