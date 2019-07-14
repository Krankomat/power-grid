using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FootprintCollisionHandler : MonoBehaviour
{

    public bool isColliding = false;


    public UnityEvent OnFootprintCollisionEnter; 
    public UnityEvent OnFootprintCollisionExit;


    private bool previousIsColliding = false; 


    // Detects, if isColliding changes state from one frame to another 
    private void Update()
    {
        if (previousIsColliding == false && isColliding == true)
            OnFootprintCollisionEnter.Invoke(); 
        else if (previousIsColliding == true & isColliding == false)
            OnFootprintCollisionExit.Invoke();

        previousIsColliding = isColliding; 
    }


    // FixedUpdate is always called right before physics calculations, which includes OnTriggerStay. 
    // This way, isColliding will be true, if it collides with one or more colliders and otherwise be false. 
    // So there's no headache if multiple colliders get touched and then the user puts the mouse so, that only one 
    // will be touched. 
    private void FixedUpdate()
    {
        isColliding = false; 
    }


    // Collisions only happen between Objects that have the LayerMask "ObjectFootprint" 
    private void OnTriggerStay(Collider other)
    {
        isColliding = true; 
    }

}
