using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElectricNetworkPanel : MonoBehaviour
{
    public Text title;
    public Text nodesTitleLabel;
    public Text edgesTitleLabel;
    public GameObject networkNodesContainer;
    public GameObject networkEdgesContainer;
    public Dictionary<ElectricNetworkNode, ElectricNetworkElementPanel> elementPanelsByNode = new Dictionary<ElectricNetworkNode, ElectricNetworkElementPanel>();
    public Dictionary<ElectricNetworkEdge, ElectricNetworkElementPanel> elementPanelsByEdge = new Dictionary<ElectricNetworkEdge, ElectricNetworkElementPanel>();

    private void Start()
    {
        // Remove example content 
        foreach (Transform child in networkNodesContainer.transform)
            Destroy(child.gameObject);
        foreach (Transform child in networkEdgesContainer.transform)
            Destroy(child.gameObject);
    }

    public void SetTitle(string newTitle)
    {
        title.text = newTitle; 
    }
}
