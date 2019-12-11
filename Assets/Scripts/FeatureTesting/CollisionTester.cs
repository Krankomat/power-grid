using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionTester : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Huzzah! I have collided with another " + other); 
    }
}
