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

    
    public void AddNode(ElectricNetworkNode addedNode, List<ElectricNetworkNode> interactedNodes)
    {
        int numberOfInvolvedNetworksInConnectionAttempt = ElectricNetworkUtil.GetDifferentNetworksOf(interactedNodes.ToArray()).Length;

        // Debug info 
        if (GameManager.Instance.isDebugging)
            for (int i = 0; i < interactedNodes.Count(); i++)
                Debug.Log($"INFO NETWORK ADDON: Interacted Node {i}: {interactedNodes[i]}. ");

        // Handle addon to network 
        if (numberOfInvolvedNetworksInConnectionAttempt == 0)
            // Interacted Nodes are passed as parameter, because there can be a connection attempt with 
            // interacted/adjacent nodes, that have connectedNetwork = null. 
            HandleCreationOfASingleNewNetwork(addedNode, interactedNodes);
        else if (numberOfInvolvedNetworksInConnectionAttempt == 1)
            HandleAddonToAnExistingNetwork(addedNode, interactedNodes);
        else if (numberOfInvolvedNetworksInConnectionAttempt > 1)
            HandleAddonToMultipleExistingNetworks(addedNode, interactedNodes);
        else
            Debug.LogError($"ERROR NETWORK ADDON: There is an illegal number of involved networks " + 
                $"({numberOfInvolvedNetworksInConnectionAttempt}) when trying to add a new node. "); 

        // Connect nodes with each other and create edges 
        foreach (ElectricNetworkNode interactedNode in interactedNodes)
            ElectricNetworkUtil.Connect(addedNode, interactedNode);

        // Create cables between power poles, if they are missing 
        CreateAllCables(); 
    }


    /*
     * This method should not be called each Update, rather only on changes. 
     */ 
    public void AddPreviewNode(ElectricNetworkNode previewNode, List<ElectricNetworkNode> interactedNodes)
    {
        foreach (ElectricNetworkNode interactedNode in interactedNodes)
            ElectricNetworkUtil.ConnectPreview(previewNode, interactedNode, previewNetwork);

        CreateCablesForNetwork(previewNetwork); 
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

        // Destroy cables and disconnect edges  
        List<ElectricNetworkEdge> connectedEdges = new List<ElectricNetworkEdge>(node.connectedEdges); 
        foreach (ElectricNetworkEdge edge in connectedEdges)
        {
            Destroy(edge.cable.gameObject);
            ElectricNetworkUtil.Disconnect(edge);
        }

        // Handle adjacent nodes 
        HandleAdjacentNodesAfterNodeRemoval(adjacentNodes); 
    }


    private void HandleCreationOfASingleNewNetwork(ElectricNetworkNode addedNode, List<ElectricNetworkNode> adjacentNodes)
    {
        // Check if adjacentNodes all have no network; 
        // If they do, another method about handling addon to network(s) should be called. 
        foreach (ElectricNetworkNode adjacentNode in adjacentNodes)
        {
            if (adjacentNode.connectedNetwork != null)
            {
                Debug.LogError($"ERROR CREATING NODE: Node {addedNode} should be only one with network, " +
                    $"but Node {adjacentNode} already has network {adjacentNode.connectedNetwork}. ");
                return; 
            }
        }
        ElectricNetwork network = CreateNewElectricNetwork();
        ElectricNetworkUtil.Register(network, addedNode);
        Debug.Log($"INFO CREATING NODE: Node {addedNode} was created with network {network}. ");

        if (adjacentNodes != null && adjacentNodes.Count > 0)
            foreach (ElectricNetworkNode adjacentNode in adjacentNodes)
            {
                ElectricNetworkUtil.Register(network, adjacentNode);
                Debug.Log($"INFO REGISTERING: Node {adjacentNode} was added to network {network}. ");
            }

        SortElectricNetworks();
    }


    private void HandleAddonToAnExistingNetwork(ElectricNetworkNode addedNode, List<ElectricNetworkNode> adjacentNodes)
    {
        ElectricNetwork network = adjacentNodes[0].connectedNetwork;
        ElectricNetworkUtil.Register(network, addedNode);
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
        // Create new lists. Otherwise, elements would be removed while iterating over the list. 
        List<ElectricNetworkNode> movedNodes = new List<ElectricNetworkNode>(disintegratingNetwork.nodes); 
        List<ElectricNetworkEdge> movedEdges = new List<ElectricNetworkEdge>(disintegratingNetwork.edges);

        // Transfer nodes 
        foreach (ElectricNetworkNode node in movedNodes)
        {
            ElectricNetworkUtil.Unregister(disintegratingNetwork, node); 
            ElectricNetworkUtil.Register(targetNetwork, node);
        }

        // Transfer edges 
        foreach (ElectricNetworkEdge edge in movedEdges)
        {
            ElectricNetworkUtil.Unregister(disintegratingNetwork, edge);
            ElectricNetworkUtil.Register(targetNetwork, edge);
        }

        // "Destroy" (aka unlink) network 
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

        DestroyCablesFromNetwork(previewNetwork);
        previewNetwork.edges.Clear();
    }


    /*
     * Creates the cables between two power poles for all networks. 
     * This is probably an expensive operation, because many objects can get created at the same time. 
     */
    private void CreateAllCables()
    {
        foreach (ElectricNetwork network in electricNetworks)
            CreateCablesForNetwork(network);
    }

    /*
     * Use with care. Destroys all CableConnection gameObjects for all networks. 
     */ 
    private void DestroyAllCables()
    {
        foreach (ElectricNetwork network in electricNetworks)
            DestroyCablesFromNetwork(network);
    }


    private void CreateCablesForNetwork(ElectricNetwork network)
    {
        if (network.edges.Count == 0)
            return; 

        foreach (ElectricNetworkEdge edge in network.edges)
        {
            if (edge.cable != null)
                continue;

            GameObject cable = Instantiate(cablePrefab);
            ElectricNetworkCableConnection cableConnection = cable.GetComponent<ElectricNetworkCableConnection>();
            cableConnection.edge = edge;
            edge.cable = cableConnection;
            cableConnection.Connect();
        }
    }


    private void DestroyCablesFromNetwork(ElectricNetwork network)
    {
        if (network.edges.Count == 0)
            return;

        foreach (ElectricNetworkEdge edge in network.edges)
            Destroy(edge.cable.gameObject);
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
