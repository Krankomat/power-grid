using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElectricNetworkElementPanel : MonoBehaviour
{
    public Text elementLabel;

    public void SetText(string newText)
    {
        elementLabel.text = newText; 
    }
}
