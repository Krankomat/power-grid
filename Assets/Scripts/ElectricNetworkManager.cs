using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElectricNetworkManager : MonoBehaviour
{

    public GameObject cablePrefab;
    public GameObject debugConnectionLinePrefab; 
    public List<ElectricNetwork> electricNetworks = new List<ElectricNetwork>();;
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


    // Method is called "Register" and not "Connect" to distinguish between creating a connection between two nodes 
    // and registering a given node or edge in a eletric network. 
    public static void Register(ElectricNetwork network, ElectricNetworkNode node)
    {
        // Normally, these two error messages should always come at the same time. 
        // But of course, it is possible that the connection is only one sided because of some error. 
        if (network.nodes.Contains(node))
            Debug.LogError($"ERROR REGISTERING: Network \"{network}\" already contains Node \"{node}\". "); 
        network.nodes.Add(node);

        if (node.connectedNetwork == network)
            Debug.LogError($"ERROR REGISTERING: Node \"{node}\" is already linked to Network \"{network}\". ");
        node.connectedNetwork = network; 
    }

    public static void Register(ElectricNetwork network, ElectricNetworkEdge edge)
    {
        if (network.edges.Contains(edge))
            Debug.LogError($"ERROR REGISTERING: Network \"{network}\" already contains Edge \"{edge}\". ");
        network.edges.Add(edge);

        if (edge.connectedNetwork == network)
            Debug.LogError($"ERROR REGISTERING: Edge \"{edge}\" is already linked to Network \"{network}\". ");
        edge.connectedNetwork = network; 
    }

    public static void Unregister(ElectricNetwork network, ElectricNetworkNode node)
    {
        if (!network.nodes.Contains(node))
            Debug.LogError($"ERROR UNREGISTERING: Network \"{network}\" does not contain Node \"{node}\". ");
        network.nodes.Remove(node);

        if (node.connectedNetwork != network)
            Debug.LogError($"ERROR UNREGISTERING: Node \"{node}\" was not connected to Network \"{network}\" in the first place. ");
        node.connectedNetwork = null; 
    }

    public static void Unregister(ElectricNetwork network, ElectricNetworkEdge edge)
    {
        if (!network.edges.Contains(edge))
            Debug.LogError($"ERROR UNLINKING: Network \"{network}\" does not contain Edge \"{edge}\". ");
        network.edges.Remove(edge);

        if (edge.connectedNetwork != network)
            Debug.LogError($"ERROR UNLINKING: Edge \"{edge}\" was not connected to Network \"{network}\" in the first place. ");
        edge.connectedNetwork = null;
    }

    //TODO: Iterate over electric network and create cable objects for each edge 


    public static void Connect(ElectricNetworkNode node1, ElectricNetworkNode node2)
    {
        // Connect nodes with each other 
        node1.connectedNodes.Add(node2);
        node2.connectedNodes.Add(node1);

        // Create edge between node1 and node2 
        ElectricNetworkEdge edge = new ElectricNetworkEdge(node1, node2);

        // Add edge to nodes 
        node1.connectedEdges.Add(edge); 
        node2.connectedEdges.Add(edge);

        // Check if both nodes have the same network 
        if (node1.connectedNetwork != node2.connectedNetwork)
            Debug.LogError($"ERROR CONNECTING: Node 1 and Node 2 have a different network. " +
                $"Node 1: {node1} - {node1.connectedNetwork}; Node 2: {node2} - {node2.connectedNetwork}. ");

        // Register edge in network and vice versa
        Register(node1.connectedNetwork, edge); 
    }


    // Unlike the other Connect method, this one is used to create a preview edge. This preview edge is shown, when 
    // hovering a building, so the according cables are displayed. The preview edge is only added to the previewNetwork. 
    public static void ConnectPreview(ElectricNetworkNode node1, ElectricNetworkNode node2, ElectricNetwork previewNetwork)
    {
        // Create preview edge between node1 and node2 
        ElectricNetworkEdge previewEdge = new ElectricNetworkEdge(node1, node2);
        previewEdge.type = ElectricNetworkEdge.Type.Preview;

        // Register edge in previewNetwork and vice versa
        Register(previewNetwork, previewEdge); 
    }


    // Effectively destroys the edge 
    public static void Disconnect(ElectricNetworkEdge edge)
    {
        // Disconnect nodes from each other 
        ElectricNetworkNode node1 = edge.nodes.Item1; 
        ElectricNetworkNode node2 = edge.nodes.Item2;
        node1.connectedNodes.Remove(node2); 
        node2.connectedNodes.Remove(node1);

        // "Destroy" edge between nodes 
        edge.nodes.Item1.connectedEdges.Remove(edge); 
        edge.nodes.Item2.connectedEdges.Remove(edge);

        // Unregister edge from network 
        Unregister(edge.connectedNetwork, edge); 
    }


    public static void Disconnect(ElectricNetworkNode node1, ElectricNetworkNode node2)
    {
        // Check if both nodes are connected with each other 
        if (!node1.connectedNodes.Contains(node2) && !node2.connectedNodes.Contains(node1))
            Debug.LogError($"ERROR DISCONNECTING: Node 1 {node1} and Node 2 {node2} are not connected with each other. ");
        else if (!node1.connectedNodes.Contains(node2))
            Debug.LogError($"ERROR DISCONNECTING: Node 1 {node1} is not connected to Node 2 {node2}. ");
        else if (!node2.connectedNodes.Contains(node1))
            Debug.LogError($"ERROR DISCONNECTING: Node 2 {node2} is not connected to Node 1 {node1}. ");

        List<ElectricNetworkEdge> commonEdges = node1.connectedEdges.Intersect(node2.connectedEdges).ToList();
        if (commonEdges.Count() == 0)
            Debug.LogError($"ERROR DISCONNECTING: There is no common edge between Node 1 {node1} and Node 2 {node2}. ");
        else if (commonEdges.Count() > 1)
            Debug.LogError($"ERROR DISCONNECTING: There are {commonEdges.Count()} edges between Node 1 {node1} and Node 2 {node2}, " +
                $"but there should be only 1. ");
        else
            Disconnect(commonEdges[0]);
    }


    public static bool IsInSameNetwork(List<ElectricNetworkNode> nodes)
    {
        ElectricNetwork previousNetwork = nodes[0].connectedNetwork; 
        foreach (ElectricNetworkNode node in nodes)
        {
            if (node.connectedNetwork != previousNetwork)
                return false;
            previousNetwork = node.connectedNetwork; 
        }
        return true; 
    }


    public void HandleElectricNetworkNodeAddOn(ElectricNetworkNode addedNode, List<ElectricNetworkNode> interactedNodes)
    {
        int numberOfInvolvedNetworksInConnectionAttempt = GetDifferentNetworksOf(interactedNodes.ToArray()).Length;

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
            Connect(addedNode, interactedNode);
    }


    public void HandlePreviewOfElectricNetworkNodeAddOn(ElectricNetworkNode previewNode, List<ElectricNetworkNode> interactedNodes)
    {
        foreach (ElectricNetworkNode interactedNode in interactedNodes)
            ConnectPreview(previewNode, interactedNode, previewNetwork);
    }


    public void DestroyNode(ElectricNetworkNode node)
    {
        // Could throw error, when "node.connectedNodes = null" and then "adjacentNodes[0]"? 
        List<ElectricNetworkNode> adjacentNodes = new List<ElectricNetworkNode>(node.connectedNodes); 

        // Unregister from network 
        Unregister(node.connectedNetwork, node);

        // If no nodes are connected, return 
        if (node.connectedNodes.Count() == 0 && node.connectedEdges.Count() == 0)
            return; 

        // "Destroy" Edges 
        foreach (ElectricNetworkEdge edge in node.connectedEdges)
            Disconnect(edge);

        // Handle adjacent nodes 
        HandleAdjacentNodesAfterNodeRemoval(adjacentNodes); 
    }


    private ElectricNetwork[] GetDifferentNetworksOf(ElectricNetworkNode[] nodes)
    {
        List<ElectricNetwork> networks = new List<ElectricNetwork>();

        foreach (ElectricNetworkNode node in nodes)
        {
            if (node.connectedNetwork == null)
                continue; 

            if (!networks.Contains(node.connectedNetwork))
                networks.Add(node.connectedNetwork);
        }

        return networks.ToArray(); 
    }


    private void HandleCreationOfASingleNewNetwork(ElectricNetworkNode addedNode, List<ElectricNetworkNode> adjacentNodes)
    {
        //TODO: Check if adjacentNodes are needed in this method 
        ElectricNetwork network = CreateNewElectricNetwork();

        // Connect nodes to network 
        Register(network, addedNode); 

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
        Register(network, addedNode);

        // Handle orphan connectors 
        foreach (ElectricNetworkNode adjacentNode in adjacentNodes)
            if (adjacentNode.connectedNetwork == null)
                Register(network, adjacentNode);

        SortElectricNetworks();

        Debug.Log($"INFO: Node {addedNode} was added to network {network}. ");
    }


    private void HandleAddonToMultipleExistingNetworks(ElectricNetworkNode addedNode, List<ElectricNetworkNode> adjacentNodes)
    {
        ElectricNetwork[] existingNetworks = GetDifferentNetworksOf(adjacentNodes.ToArray());
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
        Register(biggestNetwork, addedNode);

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

        previewNetwork.edges.Clear(); 
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
                Register(electricNetwork, resolvedNode); 
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
