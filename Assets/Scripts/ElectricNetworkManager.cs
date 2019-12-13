using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElectricNetworkManager : MonoBehaviour
{

    public GameObject cablePrefab;
    public GameObject debugConnectionLinePrefab; 
    public List<ElectricNetwork> electricNetworks = new List<ElectricNetwork>();
    public ElectricNetwork previewNetwork = new ElectricNetwork(); 
    
    private DebugDrawer debugDrawer; 
    

    private void Update()
    {
        Debug.Log($"INFO: There are currently {electricNetworks.Count} electric networks. ");


        if (Input.GetKeyUp(KeyCode.U))
        {
            if (debugDrawer == null)
                debugDrawer = new DebugDrawer(electricNetworks, debugConnectionLinePrefab);

            debugDrawer.CreateDebugLinesForNetworks(); 
        }
    }

    
    public void HandleElectricNetworkNodeAddOn(ElectricNetworkNode addedNode, List<ElectricNetworkNode> interactedNodes)
    {
        int numberOfInvolvedNetworksInConnectionAttempt = ElectricNetworkUtil.GetDifferentNetworksOf(interactedNodes.ToArray()).Length;

        // Debug info 
        if (GameManager.Instance.isDebugging)
            for (int i = 0; i < interactedNodes.Count(); i++)
                Debug.Log($"INFO NETWORK ADDON: Interacted Node {i}: {interactedNodes[i]}. ");

        // If there is no interaction with any other node, return 
        if (interactedNodes == null || interactedNodes.Count() == 0)
            return; 

        // Handle addon to network 
        if (numberOfInvolvedNetworksInConnectionAttempt == 0)
            HandleCreationOfASingleNewNetwork(addedNode, interactedNodes);
        else if (numberOfInvolvedNetworksInConnectionAttempt == 1)
            HandleAddonToAnExistingNetwork(addedNode, interactedNodes);
        else if (numberOfInvolvedNetworksInConnectionAttempt > 1)
            HandleAddonToMultipleExistingNetworks(addedNode, interactedNodes);
        else
            Debug.LogError($"ERROR NETWORK ADDON: There is an illegal number of involved networks " + 
                $"({numberOfInvolvedNetworksInConnectionAttempt}) when trying to add a new node. "); 

        //Handle nodes connecting with each other 
        foreach (ElectricNetworkNode interactedNode in interactedNodes)
            ElectricNetworkUtil.Connect(addedNode, interactedNode);
    }


    /*
     * This method should not be called each Update, rather only on changes. 
     */ 
    public void HandlePreviewOfElectricNetworkNodeAddOn(ElectricNetworkNode previewNode, List<ElectricNetworkNode> interactedNodes)
    {
        foreach (ElectricNetworkNode interactedNode in interactedNodes)
            ElectricNetworkUtil.ConnectPreview(previewNode, interactedNode, previewNetwork);

        CreateCablesForPreviewEdges(); 
    }


    public void DestroyNode(ElectricNetworkNode node)
    {
        // Could throw error, when "node.connectedNodes = null" and then "adjacentNodes[0]"? 
        List<ElectricNetworkNode> adjacentNodes = new List<ElectricNetworkNode>(node.connectedNodes);

        // Unregister from network 
        ElectricNetworkUtil.Unregister(node.connectedNetwork, node);

        // If no nodes are connected, return 
        if (node.connectedNodes.Count() == 0 && node.connectedEdges.Count() == 0)
            return; 

        // "Destroy" Edges 
        foreach (ElectricNetworkEdge edge in node.connectedEdges)
            ElectricNetworkUtil.Disconnect(edge);

        // Handle adjacent nodes 
        HandleAdjacentNodesAfterNodeRemoval(adjacentNodes); 
    }


    private void HandleCreationOfASingleNewNetwork(ElectricNetworkNode addedNode, List<ElectricNetworkNode> adjacentNodes)
    {
        //TODO: Check if adjacentNodes are needed in this method 
        ElectricNetwork network = CreateNewElectricNetwork();

        // Connect nodes to network 
        ElectricNetworkUtil.Register(network, addedNode); 

        SortElectricNetworks();

        Debug.Log($"INFO: New Network {network} was created and added! ");
    }


    // Idealy, a new connector without a network gets added 
    // But it can happen, that the newly placed connector also connects with an orphan node. Then, 
    // this orphan node also has to be added to the single existing network, that gets an addon. 
    private void HandleAddonToAnExistingNetwork(ElectricNetworkNode addedNode, List<ElectricNetworkNode> adjacentNodes)
    {
        //TODO: This follows the old system, that a graph needs at least two members. But now, every node should have a network. 
        ElectricNetwork network = adjacentNodes[0].connectedNetwork;
        ElectricNetworkUtil.Register(network, addedNode);

        // Handle orphan connectors 
        foreach (ElectricNetworkNode adjacentNode in adjacentNodes)
            if (adjacentNode.connectedNetwork == null)
                ElectricNetworkUtil.Register(network, adjacentNode);

        SortElectricNetworks();

        Debug.Log($"INFO: Node {addedNode} was added to network {network}. ");
    }


    private void HandleAddonToMultipleExistingNetworks(ElectricNetworkNode addedNode, List<ElectricNetworkNode> adjacentNodes)
    {
        ElectricNetwork[] existingNetworks = ElectricNetworkUtil.GetDifferentNetworksOf(adjacentNodes.ToArray());
        existingNetworks = ElectricNetwork.SortBySize(existingNetworks);
        ElectricNetwork biggestNetwork = existingNetworks[0]; 

        // Integrate smaller networks into the biggest one 
        foreach (ElectricNetwork disintegratedNetwork in existingNetworks)
        {
            if (disintegratedNetwork == biggestNetwork)
                continue;

            // Could be problematic, because it's iterating over a list that destroys its members 
            IntegrateElectricNetworkIntoAnother(biggestNetwork, disintegratedNetwork); 
        }

        // Register added node into biggest network all other networks get integrated into 
        ElectricNetworkUtil.Register(biggestNetwork, addedNode);

        SortElectricNetworks();
    }


    private ElectricNetwork CreateNewElectricNetwork()
    {
        ElectricNetwork network = new ElectricNetwork();
        electricNetworks.Add(network);

        Debug.Log("New network created! "); 

        return network; 
    }


    private void DestroyElectricNetwork(ElectricNetwork network)
    {
        electricNetworks.Remove(network);
        network = null; 
    }


    private void IntegrateElectricNetworkIntoAnother(ElectricNetwork targetNetwork, ElectricNetwork disintegratingNetwork)
    {
        List<ElectricNetworkNode> integratedNodes = disintegratingNetwork.nodes;
        
        foreach (ElectricNetworkNode node in integratedNodes)
            node.connectedNetwork = targetNetwork; 
        
        targetNetwork.nodes.AddRange(integratedNodes);

        DestroyElectricNetwork(disintegratingNetwork); 
    }
    

    private void SortElectricNetworks()
    {
        electricNetworks = ElectricNetwork.SortBySize(electricNetworks); 
    }


    public void ClearPreviewNetworkEdges()
    {
        if (previewNetwork.edges.Count == 0)
            return;

        DestroyCablesFromPreviewEdges(); 
        previewNetwork.edges.Clear(); 
    } 


    private void CreateCablesForPreviewEdges()
    {
        foreach (ElectricNetworkEdge previewEdge in previewNetwork.edges)
        {
            if (previewEdge.cable != null)
                continue; 

            GameObject cable = Instantiate(cablePrefab);
            ElectricNetworkCableConnection cableConnection = cable.GetComponent<ElectricNetworkCableConnection>();
            cableConnection.edge = previewEdge;
            previewEdge.cable = cableConnection; 
            cableConnection.Connect();
        }
    }


    private void DestroyCablesFromPreviewEdges()
    {
        if (previewNetwork.edges.Count == 0)
            return; 

        foreach (ElectricNetworkEdge previewEdge in previewNetwork.edges)
            Destroy(previewEdge.cable.gameObject); 
    }


    private void HandleAdjacentNodesAfterNodeRemoval(List<ElectricNetworkNode> adjacentNodes)
    {
        if (adjacentNodes == null || adjacentNodes.Count == 0)
            return; 

        // Case 1: There is only one adjacent node on removal 
        if (adjacentNodes.Count == 1)
            return;

        // Case 2: There are at least two adjacent nodes and AT LEAST ONE is NOT connected with each other 
        NetworkResolver networkResolver = new NetworkResolver();
        List<List<ElectricNetworkNode>> listsOfResolvedNodes = 
                networkResolver.GetListsOfResolvedNodesAt(adjacentNodes);

        foreach(List<ElectricNetworkNode> resolvedNodes in listsOfResolvedNodes)
        {
            ElectricNetwork electricNetwork = CreateNewElectricNetwork(); 
            foreach (ElectricNetworkNode resolvedNode in resolvedNodes)
                ElectricNetworkUtil.Register(electricNetwork, resolvedNode); 
        }

        // Case 3: (Special case) There are at least two adjacent nodes and all are connected with each other on removal. 
        // This case is not handled here, because it also gets resolved by the NetworkResolver. 
    }


    // Class to recursively resolve networks, when a connector is being removed 
    //TODO: Handle edges and their connectedNetwork when traversing nodes 
    private class NetworkResolver
    {

        List<ElectricNetworkNode> resolvedNodes = new List<ElectricNetworkNode>();

        
        public List<ElectricNetworkNode> GetResolvedNodesAt(ElectricNetworkNode node)
        {
            TraverseNode(node);
            return resolvedNodes; 
        }


        public List<List<ElectricNetworkNode>> GetListsOfResolvedNodesAt(List<ElectricNetworkNode> nodes)
        {
            List<NetworkResolver> networkResolver = new List<NetworkResolver>();
            List<List<ElectricNetworkNode>> listOfResolvedNodes = new List<List<ElectricNetworkNode>>();
            List<ElectricNetworkNode> nodesToBeTraversed = new List<ElectricNetworkNode>();

            nodesToBeTraversed.AddRange(nodes); 

            ElectricNetworkNode currentConnector = nodesToBeTraversed[0]; 
            
            while (nodesToBeTraversed.Count > 0)
            {
                ElectricNetworkNode currentlyTraversedNode = nodesToBeTraversed[0];
                NetworkResolver currentlyResolvedNetwork = new NetworkResolver();
                
                nodesToBeTraversed.Remove(currentlyTraversedNode);
                currentlyResolvedNetwork.TraverseNodeAndWatchOutForSubsequentlyTraversedNodes(
                        currentlyTraversedNode, nodesToBeTraversed);

                listOfResolvedNodes.Add(currentlyResolvedNetwork.resolvedNodes); 
            }
            
            return listOfResolvedNodes; 
        }


        private void TraverseNode(ElectricNetworkNode node)
        {
            foreach (ElectricNetworkNode childNode in node.connectedNodes)
            {
                if (resolvedNodes.Contains(childNode))
                    continue;

                resolvedNodes.Add(childNode);
                TraverseNode(childNode); 
            }
        }

        
        private void TraverseNodeAndWatchOutForSubsequentlyTraversedNodes(ElectricNetworkNode node, 
                                                                          List<ElectricNetworkNode> subsequentlyTraversedNodes)
        {
            foreach (ElectricNetworkNode childNode in node.connectedNodes)
            {
                if (subsequentlyTraversedNodes.Contains(childNode))
                    subsequentlyTraversedNodes.Remove(childNode); 

                if (resolvedNodes.Contains(childNode))
                    continue;

                resolvedNodes.Add(childNode);
                // Also transfer the cables referenced in the script? 
                TraverseNodeAndWatchOutForSubsequentlyTraversedNodes(childNode, subsequentlyTraversedNodes);
            }
        }


    }
    private class DebugDrawer
    {
        List<ElectricNetwork> electricNetworks;
        GameObject debugConnectionLinePrefab;

        private Dictionary<ElectricNetworkEdge, GameObject> debugLinesByConnection; 


        public DebugDrawer(List<ElectricNetwork> electricNetworks, GameObject debugConnectionLinePrefab)
        {
            this.electricNetworks = electricNetworks;
            this.debugConnectionLinePrefab = debugConnectionLinePrefab; 
        }


        public void CreateDebugLinesForNetworks()
        {
            debugLinesByConnection = new Dictionary<ElectricNetworkEdge, GameObject>(); 

            foreach (ElectricNetwork network in electricNetworks)
                foreach (ElectricNetworkEdge edge in network.edges)
                {
                    GameObject debugLine = Instantiate(debugConnectionLinePrefab);
                    debugLinesByConnection.Add(edge, debugLine);
                    Debug.Log("Debug Line created! ");
                }
        }
    }
}
