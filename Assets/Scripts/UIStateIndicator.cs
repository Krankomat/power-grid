using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection; 
using UnityEngine;
using UnityEngine.UI;

public class UIStateIndicator : MonoBehaviour
{

    public GameObject border;
    public GameObject label;
    [HideInInspector] public IndicatorState currentState; 

    private Text labelText;


    public enum IndicatorState
    {
        None, 
        Placing, 
        Demolishing
    }


    private enum IndicatorStyle
    {
        [IndicatorStyle("#00ff00c8", "Placing")]
        Placing,
        [IndicatorStyle("#ff0000c8", "Demolishing")]
        Demolishing
    }


    private void Start()
    {
        labelText = PlayerManager.GetChildObject(label, "Text").GetComponent<Text>(); 
    }


    private void Update()
    {

        if (currentState == IndicatorState.None)
        {
            Hide(); 
            return; 
        }

        if (currentState == IndicatorState.Placing)
        {
            DisplayStyle(IndicatorStyle.Placing); 
            return; 
        }

        if (currentState == IndicatorState.Demolishing)
        {
            DisplayStyle(IndicatorStyle.Demolishing);
            return; 
        }
        
        Debug.LogError("Unsupported UI indicator state ");
    }


    private void Show()
    {
        border.SetActive(true);
        label.SetActive(true);
    }


    private void Hide()
    {
        border.SetActive(false);
        label.SetActive(false);
    }


    private void DisplayStyle(IndicatorStyle style)
    {
        Color styleColor = GetColorFrom(style);
        string styleText = GetTextFrom(style);

        border.GetComponent<Image>().color = styleColor;
        label.GetComponent<Image>().color = styleColor;
        labelText.text = styleText; 
    }


    private Color GetColorFrom(IndicatorStyle style)
    {
        Color color; 
        IndicatorStyleAttribute attribute = (IndicatorStyleAttribute)style.GetAttr();
        ColorUtility.TryParseHtmlString(attribute.ColorCode, out color); 

        return color; 
    }


    private string GetTextFrom(IndicatorStyle style)
    {
        IndicatorStyleAttribute attribute = (IndicatorStyleAttribute)style.GetAttr();
        return attribute.Text; 
    }


    private class IndicatorStyleAttribute : EnumAttribute
    {
        public IndicatorStyleAttribute(string colorCode, string text)
        {
            this.ColorCode = colorCode;
            this.Text = text; 
        }

        public string ColorCode { get; private set; }
        public string Text { get; private set; } 
    }

}
