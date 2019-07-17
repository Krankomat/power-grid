using System;
using System.Reflection;

/** 
 * An extension to Enums, so that they can store values within Attributes. 
 * This way, they are more similar to Enums in Java. 
 * 
 * Based on https://www.codeproject.com/Articles/38666/Enum-Pattern.aspx  
 * Original author: Gong Liu 
 * Date: 22 Sep 2009 
 */

public static class EnumExtension  
{

    // This is an extension method 
    public static EnumAttribute GetAttr(this Enum value)
    {
        Type type = value.GetType();
        FieldInfo fieldInfo = type.GetField(value.ToString());
        EnumAttribute[] attributes = (EnumAttribute[]) fieldInfo.GetCustomAttributes(typeof(EnumAttribute), false);

        if (attributes.Length > 0)
            return attributes[0];

        return null; 

    }
}
