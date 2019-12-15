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
    private List<ElectricNetworkPanel> newlyCreatednetworkPanels = new List<ElectricNetworkPanel>(); 


    void Start()
    {
        RemoveExampleContent(); 
    }


    void Update()
    {
        // Clear newly created network panels for this frame 
        newlyCreatednetworkPanels.Clear();

        // Update network panels 
        CreateNewPanelsIfNecessary();
        RemoveOutdatedPanels();

        // Update content inside network panels 
        foreach (ElectricNetwork network in panelsByNetwork.Keys)
        {
            // You have to wait at least one frame before the child elements can be added to the electricNetworkPanel 
            if (newlyCreatednetworkPanels.Contains(panelsByNetwork[network]))
                continue; 
            UpdatePanelContent(network, panelsByNetwork[network]); 
        }
    }


    private void UpdatePanelContent(ElectricNetwork network, ElectricNetworkPanel networkPanel)
    {
        CreateNodePanelsForPanelIfNecessary(network, networkPanel);
        RemoveNodePanelsForPanelIfNecessary(network, networkPanel);
        CreateEdgePanelsForPanelIfNecessary(network, networkPanel);
        RemoveEdgePanelsForPanelIfNecessary(network, networkPanel);
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
            newlyCreatednetworkPanels.Add(networkPanel);
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


    private void CreateNodePanelsForPanelIfNecessary(ElectricNetwork network, ElectricNetworkPanel networkPanel)
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
            elementPanel.SetText("N " + node.connector.Id);
            elementPanel.type = ElectricNetworkElementPanel.Type.Node; 
            networkPanel.elementPanelsByNode.Add(node, elementPanel);
        }
    }


    private void RemoveNodePanelsForPanelIfNecessary(ElectricNetwork network, ElectricNetworkPanel networkPanel) 
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


    private void CreateEdgePanelsForPanelIfNecessary(ElectricNetwork network, ElectricNetworkPanel networkPanel)
    {
        // Create Edges (if necessary) 
        foreach (ElectricNetworkEdge edge in network.edges)
        {
            if (networkPanel.elementPanelsByEdge.Keys.Contains(edge))
                continue;

            // Create Edge-ElementPanel entry 
            GameObject elementPanelGameObject = Instantiate(networkElementPrefab);
            elementPanelGameObject.transform.SetParent(networkPanel.networkEdgesContainer.transform);
            ElectricNetworkElementPanel elementPanel = elementPanelGameObject.GetComponent<ElectricNetworkElementPanel>();
            elementPanel.SetText("E " + edge.cable.Id);
            elementPanel.type = ElectricNetworkElementPanel.Type.Edge;
            networkPanel.elementPanelsByEdge.Add(edge, elementPanel);
        }
    }


    private void RemoveEdgePanelsForPanelIfNecessary(ElectricNetwork network, ElectricNetworkPanel networkPanel)
    {
        List<ElectricNetworkEdge> possiblyOutdatedEdges = new List<ElectricNetworkEdge>(networkPanel.elementPanelsByEdge.Keys);
        foreach (ElectricNetworkEdge possiblyOutdatedEdge in possiblyOutdatedEdges)
        {
            if (network.edges.Contains(possiblyOutdatedEdge))
                continue;

            Destroy(networkPanel.elementPanelsByEdge[possiblyOutdatedEdge].gameObject);
            networkPanel.elementPanelsByEdge.Remove(possiblyOutdatedEdge);
        }
    }


    private void RemoveExampleContent()
    {
        foreach (Transform child in networkPanelContainer.transform)
            Destroy(child.gameObject);
    }
    
}
