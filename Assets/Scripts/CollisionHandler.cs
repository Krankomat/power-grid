using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionHandler : MonoBehaviour
{

    [Tooltip("Set to true, if you want to register intersections with colliders from the same layer mask. ")]
    public bool isRegisteringColliderIntersections;
    // Intersection of colliders can be deactivated, if it is not necessary anymore 
    [HideInInspector] public bool colliderIntersectingIsCurrentlyActive; 
    /* should be hidden, but visible for debugging purposes */
    public bool isColliding = false; 
    [HideInInspector] public Collider[] intersectingColliders; 


    public UnityEvent OnCollisionHandlerEnter; 
    public UnityEvent OnCollisionHandlerExit;


    private bool previousIsColliding = false;
    private BoxCollider boxCollider; 


    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>(); 
    }


    // Detects, if isColliding changes state from one frame to another 
    private void Update()
    {
        if (previousIsColliding == true & isColliding == false)
            OnCollisionHandlerExit.Invoke();

        if (isRegisteringColliderIntersections && colliderIntersectingIsCurrentlyActive) 
            if (isColliding) 
                HandleIntersectingColliders();

        if (previousIsColliding == false && isColliding == true)
            OnCollisionHandlerEnter.Invoke();

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
        Debug.Log("There is a collision! "); 
    }


    private Collider[] GetIntersectingCollidersOf(BoxCollider collider)
    {
        Vector3 colliderHalfExtents = new Vector3(collider.size.x / 2, collider.size.y / 2, collider.size.z / 2);
        LayerMask colliderLayerMask = 1 << gameObject.layer;

        Collider[] otherColliders = Physics.OverlapBox(
                collider.gameObject.transform.position, 
                colliderHalfExtents, 
                Quaternion.identity, 
                colliderLayerMask, 
                QueryTriggerInteraction.Collide); 

        return otherColliders; 
    }


    private void HandleIntersectingColliders()
    {
        intersectingColliders = GetIntersectingCollidersOf(boxCollider);

        foreach (Collider collider in intersectingColliders)
            Debug.DrawLine(gameObject.transform.position, collider.gameObject.transform.position);

        Debug.Log("Intersecting colliders: " + intersectingColliders.Length); 
    }

}
