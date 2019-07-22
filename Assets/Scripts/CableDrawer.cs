using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CableDrawer : MonoBehaviour
{

    public GameObject cableModel;
    public Transform startPoint;
    public Transform endPoint;
    public float cableModelYOffset = 0.5f;
    public float cableModelLength = 2f; 
    
    private Vector3 direction;
    private Vector3 rotationDirection; 


    // Maybe add private event, that only is fired when position actually changes. 
    // So the position, rotation and scale is not set all the time, but only if there 
    // actually is any change. 
    #if UNITY_EDITOR 
    void Update()
    {
        direction = endPoint.position - startPoint.position;
        rotationDirection = Vector3.RotateTowards(transform.forward, direction, 99f, 0.0f); 

        cableModel.transform.rotation = Quaternion.LookRotation(direction);
        cableModel.transform.Rotate(new Vector3(0, -90, 0));

        cableModel.transform.position = (startPoint.position + endPoint.position) / 2;
        cableModel.transform.position += new Vector3(0, -cableModelYOffset, 0);

        cableModel.transform.localScale = new Vector3(direction.magnitude / cableModelLength, 1, 1); 
    }
    #endif


    public void SetPositions(Vector3 start, Vector3 end)
    {
        startPoint.position = start;
        endPoint.position = end;
    }


    public void SetTransforms(Transform start, Transform end)
    {
        startPoint = start;
        endPoint = end;
    }

}
