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

    
    public Vector3 SampleParabola(Vector3 start, Vector3 end, float height, float t)
    {
        float parabolicT = t * 2 - 1;
        if (Mathf.Abs(start.y - end.y) < 0.1f)
        {
            //start and end are roughly level, pretend they are - simpler solution with less steps
            Vector3 travelDirection = end - start;
            Vector3 result = start + t * travelDirection;
            result.y += (-parabolicT * parabolicT + 1) * height;
            return result;
        }
        else
        {
            //start and end are not level, gets more complicated
            Vector3 travelDirection = end - start;
            Vector3 levelDirecteion = end - new Vector3(start.x, end.y, start.z);
            Vector3 right = Vector3.Cross(travelDirection, levelDirecteion);
            Vector3 up = Vector3.Cross(right, travelDirection);
            if (end.y > start.y) up = -up;
            Vector3 result = start + t * travelDirection;
            result += ((-parabolicT * parabolicT + 1) * height) * up.normalized;
            return result;
        }
    }
    
}
