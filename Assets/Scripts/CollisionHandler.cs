using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionHandler : MonoBehaviour
{
    public List<Collider> enteredColliders = new List<Collider>(); 
    
    public UnityEvent OnCollisionHandlerEnter; 
    public UnityEvent OnCollisionHandlerExit;
    

    public bool IsColliding()
    {
        if (enteredColliders.Count > 0)
            return true;

        return false; 
    }


    private void OnTriggerEnter(Collider other)
    {
        enteredColliders.Add(other);
        OnCollisionHandlerEnter.Invoke();
    }


    private void OnTriggerExit(Collider other)
    {
        enteredColliders.Remove(other);
        OnCollisionHandlerExit.Invoke();
    }
}
