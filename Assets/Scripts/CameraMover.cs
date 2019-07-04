using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    public Camera activeCamera;
    public float speed;

    private Vector3 inputDirection = new Vector2(); 
    private float inputHorizontal;
    private float inputVertical; 
    
    
    void FixedUpdate()
    {
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");

        inputDirection.x = inputHorizontal;
        inputDirection.z = inputVertical;
        inputDirection.Normalize();

        activeCamera.transform.position += inputDirection * speed * Time.deltaTime; 
    }
}
