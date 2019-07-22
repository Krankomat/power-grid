using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathUtil 
{
    
    // Returns a number in specific steps. 
    // E.g. if you have GetSteppedNumbers(<<someValue>>, 0.2f), you will get -0.4, -0.2, 0, 0.2, 0.4 ... 
    public static float SteppedNumber(float value, float step)
    {
        float steppedNumber = 0f;
        float remainder = 0f; 
        int timesDivided = 0;
        bool isNegativeValue = false; 
        bool isNegativeStep = false;

        if (value < 0)
        {
            isNegativeValue = true;
            value *= -1;
        }

        if (step < 0)
        {
            isNegativeStep = true;
            step *= -1; 
        }

        timesDivided = (int) (value / step);
        remainder = value - timesDivided * step;

        if ((remainder) >= (step / 2))
            timesDivided++;

        steppedNumber = timesDivided * step;

        if (isNegativeValue)
            steppedNumber *= -1;

        if (isNegativeStep)
            steppedNumber *= -1; 

        return steppedNumber; 
    }


    // based on: https://forum.unity.com/threads/generating-dynamic-parabola.211681/ 
    // author: hpjohn, edited by JOHNMCLAY 
    // date: Nov 20, 2013 (edited: Jun 19, 2015) 

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

    public static Vector3 Parabola(Vector3 start, Vector3 end, float height, float t, bool isAlignedToLevelDirection = false)
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
