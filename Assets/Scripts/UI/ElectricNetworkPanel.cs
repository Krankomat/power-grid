using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElectricNetworkPanel : MonoBehaviour
{
    public Text title; 
    public GameObject networkNodesContainer;
    public GameObject networkEdgesContainer;

    public void SetTitle(string newTitle)
    {
        title.text = newTitle; 
    }
}
