using System;

/** 
 * Base class to extend, if you want to create custom EnumAttributes. 
 * Look to EnumExtension.cs for more information. 
 * 
 * Based on https://www.codeproject.com/Articles/38666/Enum-Pattern.aspx  
 * Original author: Gong Liu 
 * Date: 22 Sep 2009 
 */

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class EnumAttribute : Attribute
{
    public EnumAttribute()
    {
    }
}
