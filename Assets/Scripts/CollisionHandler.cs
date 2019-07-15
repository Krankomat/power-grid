using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionHandler : MonoBehaviour
{

    public bool isColliding = false;


    public UnityEvent OnCollisionHandlerEnter; 
    public UnityEvent OnCollisionHandlerExit;


    private bool previousIsColliding = false; 


    // Detects, if isColliding changes state from one frame to another 
    private void Update()
    {
        if (previousIsColliding == false && isColliding == true)
            OnCollisionHandlerEnter.Invoke(); 
        else if (previousIsColliding == true & isColliding == false)
            OnCollisionHandlerExit.Invoke();

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


    // Collisions should only happen between Objects, that have the same LayerMask like "ObjectFootprint" 
    private void OnTriggerStay(Collider other)
    {
        isColliding = true; 
    }

}
