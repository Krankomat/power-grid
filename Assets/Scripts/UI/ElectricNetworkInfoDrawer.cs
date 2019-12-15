using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElectricNetworkInfoDrawer : MonoBehaviour
{
    public ElectricNetworkManager electricNetworkManager; 
    public GameObject networkElementPrefab;
    public GameObject networkPanelPrefab;
    public GameObject networkPanelContainer;

    private Dictionary<ElectricNetwork, ElectricNetworkPanel> panelsByNetwork = new Dictionary<ElectricNetwork, ElectricNetworkPanel>(); 


    void Start()
    {
        RemoveExampleContent(); 
    }


    void Update()
    {
        // Update network panels 
        CreateNewPanelsIfNecessary();
        RemoveOutdatedPanels();

        // Update content inside network panels 
        foreach (ElectricNetwork network in panelsByNetwork.Keys)
            UpdatePanelContent(network, panelsByNetwork[network]); 
    }


    private void CreateNewPanelsIfNecessary()
    {
        foreach (ElectricNetwork network in electricNetworkManager.electricNetworks)
        {
            if (panelsByNetwork.ContainsKey(network))
                continue;

            GameObject networkPanelGameObject = Instantiate(networkPanelPrefab);
            networkPanelGameObject.transform.SetParent(networkPanelContainer.transform);
            ElectricNetworkPanel networkPanel = networkPanelGameObject.GetComponent<ElectricNetworkPanel>();
            networkPanel.SetTitle("Network " + network.id);
            panelsByNetwork.Add(network, networkPanel);
        }
    }


    private void RemoveOutdatedPanels()
    {
        List<ElectricNetwork> possiblyOutdatedNetworks = new List<ElectricNetwork>(panelsByNetwork.Keys);
        foreach (ElectricNetwork possiblyOutdatedNetwork in possiblyOutdatedNetworks)
        {
            if (electricNetworkManager.electricNetworks.Contains(possiblyOutdatedNetwork))
                continue;

            Destroy(panelsByNetwork[possiblyOutdatedNetwork].gameObject);
            panelsByNetwork.Remove(possiblyOutdatedNetwork);
        }
    }


    private void UpdatePanelContent(ElectricNetwork network, ElectricNetworkPanel networkPanel)
    {
        CreatePanelElementsForPanelIfNecessary(network, networkPanel);
        RemovePanelElementsForPanelIfNecessary(network, networkPanel); 
    }


    private void CreatePanelElementsForPanelIfNecessary(ElectricNetwork network, ElectricNetworkPanel networkPanel)
    {
        // Create Nodes (if necessary) 
        foreach (ElectricNetworkNode node in network.nodes)
        {
            if (networkPanel.elementPanelsByNode.Keys.Contains(node))
                continue;

            // Create Node-ElementPanel entry 
            GameObject elementPanelGameObject = Instantiate(networkElementPrefab);
            elementPanelGameObject.transform.SetParent(networkPanel.networkNodesContainer.transform);
            ElectricNetworkElementPanel elementPanel = elementPanelGameObject.GetComponent<ElectricNetworkElementPanel>();
            elementPanel.SetText("N " + node.connector.GetInstanceID());
            networkPanel.elementPanelsByNode.Add(node, elementPanel);
        }
    }


    private void RemovePanelElementsForPanelIfNecessary(ElectricNetwork network, ElectricNetworkPanel networkPanel)
    {
        List<ElectricNetworkNode> possiblyOutdatedNodes = new List<ElectricNetworkNode>(networkPanel.elementPanelsByNode.Keys);
        foreach (ElectricNetworkNode possiblyOutdatedNode in possiblyOutdatedNodes)
        {
            if (network.nodes.Contains(possiblyOutdatedNode))
                continue;

            Destroy(networkPanel.elementPanelsByNode[possiblyOutdatedNode].gameObject);
            networkPanel.elementPanelsByNode.Remove(possiblyOutdatedNode); 
        }
    }


    private void RemoveExampleContent()
    {
        foreach (Transform child in networkPanelContainer.transform)
            Destroy(child.gameObject);
    }
    
}
