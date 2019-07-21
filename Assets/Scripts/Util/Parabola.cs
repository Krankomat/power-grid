using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// based on: https://forum.unity.com/threads/generating-dynamic-parabola.211681/ 
// author: hpjohn, edited by JOHNMCLAY 
// date: Nov 20, 2013 (edited: Jun 19, 2015) 

public class Parabola : MonoBehaviour
{
    public Transform someObject; //object that moves along parabola.
    float objectT = 0; //timer for that object

    public Transform Ta, Tb; //transforms that mark the start and end
    public float h; //desired parabola height

    Vector3 a, b; //Vector positions for start and end

    void Update()
    {
        if (Ta && Tb)
        {
            a = Ta.position; //Get vectors from the transforms
            b = Tb.position;

            if (someObject)
            {
                //Shows how to animate something following a parabola
                objectT = Time.time % 1; //completes the parabola trip in one second
                someObject.position = GetParabola(a, b, h, objectT);
            }
        }
    }


    void OnDrawGizmos()
    {
        //Draw the height in the viewport, so i can make a better gif :]
        Handles.BeginGUI();
        GUI.skin.box.fontSize = 16;
        GUI.Box(new Rect(10, 10, 100, 25), h + "");
        Handles.EndGUI();

        //Draw the parabola by sample a few times
        Gizmos.color = Color.red;
        Gizmos.DrawLine(a, b);
        float count = 20;
        Vector3 lastP = a;
        for (float i = 0; i < count + 1; i++)
        {
            Vector3 p = GetParabola(a, b, h, i / count);
            Gizmos.color = i % 2 == 0 ? Color.blue : Color.green;
            Gizmos.DrawLine(lastP, p);
            lastP = p;
        }
    }

    #region Parabola sampling function
    /// <summary>
    /// Get position from a parabola defined by start and end, height, and time
    /// </summary>
    /// <param name='start'>
    /// The start point of the parabola
    /// </param>
    /// <param name='end'>
    /// The end point of the parabola
    /// </param>
    /// <param name='height'>
    /// The height of the parabola at its maximum
    /// </param>
    /// <param name='t'>
    /// Normalized time (0->1)
    /// </param>
    
    Vector3 GetParabola(Vector3 start, Vector3 end, float height, float t, bool isAlignedToLevelDirection = false)
    {
        Vector3 travelDirection, levelDirection, upDirection, rightDirection, result; 
        float parabolicT = t * 2 - 1;
        bool startAndEndAreRoughlyLevel = Mathf.Abs(start.y - end.y) < 0.1f;


        travelDirection = end - start;

        //simpler solution with less steps, if start and end are roughly on same level (y axis) 
        if (startAndEndAreRoughlyLevel)
        {
            result = start + t * travelDirection;
            result.y += (-parabolicT * parabolicT + 1) * height;
            return result;
        }
        
        //start and end are not level (more complex solution) 
        levelDirection = end - new Vector3(start.x, end.y, start.z);
        rightDirection = Vector3.Cross(travelDirection, levelDirection);

        if (isAlignedToLevelDirection) 
            upDirection = Vector3.Cross(rightDirection, levelDirection);
        else 
            upDirection = Vector3.Cross(rightDirection, travelDirection);

        if (end.y > start.y)
            upDirection = -upDirection;

        result = start + t * travelDirection;
        result += ((-parabolicT * parabolicT + 1) * height) * upDirection.normalized;

        return result;
    }

    #endregion

}
