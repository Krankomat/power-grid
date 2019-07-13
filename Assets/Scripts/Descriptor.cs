using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Descriptor : MonoBehaviour
{

    public string objectName;
    [TextArea] public string description;
    public Vector2Int footprint = new Vector2Int(1, 1);


    public void Start()
    {
        if (footprint.x < 1)
            Debug.LogError("Descriptor (" + gameObject + "): The value footprint.x = " + footprint.x + " is too small. ");

        if (footprint.y < 1)
            Debug.LogError("Descriptor (" + gameObject + "): The value footprint.y = " + footprint.y + " is too small. ");
    }

}
