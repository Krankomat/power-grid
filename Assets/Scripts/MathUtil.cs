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

}
