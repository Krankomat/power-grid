using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElectricNetworkManager : MonoBehaviour
{

    public GameObject cablePrefab;
    public GameObject debugConnectionLinePrefab; 
    public List<ElectricNetwork> electricNetworks; 

    private ElectricNetworkConnector newlyAddedConnector;
    private ElectricNetworkConnector[] interactedConnectors;
    private List<ElectricNetworkCableConnection> previewCables;
    private DebugDrawer debugDrawer; 


    private void Awake()
    {
        electricNetworks = new List<ElectricNetwork>();
        previewCables = new List<ElectricNetworkCableConnection>(); 
    }


    private void Update()
    {
        Debug.Log("There are currently " + electricNetworks.Count + " electronic networks. ");

        if (Input.GetKeyUp(KeyCode.U))
        {
            if (debugDrawer == null)
                debugDrawer = new DebugDrawer(electricNetworks, debugConnectionLinePrefab);

            debugDrawer.CreateDebugLinesForNetworks(); 
        }
    }


    // Method is called "Link" and not "Connect" to distinguish between creating a connection between two nodes 
    // and linking a given node or edge to a eletric network. 
    public static void Link(ElectricNetwork network, ElectricNetworkNode node)
    {
        // Normally, these two error messages should always come at the same time. 
        // But of course, it is possible that the connection is only one sided because of some error. 
        if (network.nodes.Contains(node))
            Debug.LogError($"ERROR LINKING: Network \"{network}\" already contains Node \"{node}\". "); 
        network.nodes.Add(node);

        if (node.connectedNetwork == network)
            Debug.LogError($"ERROR LINKING: Node \"{node}\" is already linked to Network \"{network}\". ");
        node.connectedNetwork = network; 
    }

    public static void Link(ElectricNetwork network, ElectricNetworkEdge edge)
    {
        if (network.edges.Contains(edge))
            Debug.LogError($"ERROR LINKING: Network \"{network}\" already contains Edge \"{edge}\". ");
        network.edges.Add(edge);

        if (edge.connectedNetwork == network)
            Debug.LogError($"ERROR LINKING: Edge \"{edge}\" is already linked to Network \"{network}\". ");
        edge.connectedNetwork = network; 
    }

    public static void Unlink(ElectricNetwork network, ElectricNetworkNode node)
    {
        if (!network.nodes.Contains(node))
            Debug.LogError($"ERROR UNLINKING: Network \"{network}\" does not contain Node \"{node}\". ");
        network.nodes.Remove(node);

        if (node.connectedNetwork != network)
            Debug.LogError($"ERROR UNLINKING: Node \"{node}\" was not connected to Network \"{network}\" in the first place. ");
        node.connectedNetwork = null; 
    }

    public static void Unlink(ElectricNetwork network, ElectricNetworkEdge edge)
    {
        if (!network.edges.Contains(edge))
            Debug.LogError($"ERROR UNLINKING: Network \"{network}\" does not contain Edge \"{edge}\". ");
        network.edges.Remove(edge);

        if (edge.connectedNetwork != network)
            Debug.LogError($"ERROR UNLINKING: Edge \"{edge}\" was not connected to Network \"{network}\" in the first place. ");
        edge.connectedNetwork = null;
    }


    public void HandleElectricNetworkNodeAddOn(ElectricNetworkConnector placedConnector, CollisionHandler electricCollisionHandler)
    {
        newlyAddedConnector = placedConnector; 
        interactedConnectors = GetInteractedNetworkNodes(placedConnector, electricCollisionHandler.intersectingColliders);
        int numberOfInvolvedNetworksInConnectionAttempt = GetDifferentNetworksOf(interactedConnectors).Length;

        Debug.Log("Number of involved Networks: " + numberOfInvolvedNetworksInConnectionAttempt);
        Debug.Log("Involved Networks: " + GetDifferentNetworksOf(interactedConnectors));

        if (GameManager.Instance.isDebugging) 
            for (int i = 0; i < interactedConnectors.Length; i++) 
                Debug.Log(i + ": " + interactedConnectors[i]);

        // If there is no interaction with any other connector, return without creating a network 
        if (interactedConnectors.Length == 0)
            return;

        // Handle addon to network 
        if (numberOfInvolvedNetworksInConnectionAttempt == 0)
        {
            HandleCreationOfANewNetwork(); 
        }
        else if (numberOfInvolvedNetworksInConnectionAttempt == 1)
        {
            HandleAddonToAnExistingNetwork(); 
        }
        else if (numberOfInvolvedNetworksInConnectionAttempt > 1)
        {
            HandleAddonToMultipleExistingNetworks(); 
        }
        else
        {
            Debug.LogError("There is an illegal number of involved networks (" 
                    + numberOfInvolvedNetworksInConnectionAttempt + ") when trying to add a new node. "); 
        }

        //Handle nodes connecting with each other 
        foreach (ElectricNetworkConnector connector in interactedConnectors)
            connector.ConnectBothSidedTo(newlyAddedConnector); 

    }


    public void ShowPreviewOfElectricNetworkNodeAddOn(ElectricNetworkConnector previewConnector, CollisionHandler electricCollisionHandler)
    {
        interactedConnectors = GetInteractedNetworkNodes(previewConnector, electricCollisionHandler.intersectingColliders);

        foreach (ElectricNetworkConnector connector in interactedConnectors)
            previewConnector.CreateCableConnectionTo(connector, true);

        previewCables.AddRange(previewConnector.cableConnections); 
    }


    public void HandleElectricNetworkNodeRemoval(ElectricNetworkConnector connectorToBeRemoved, CollisionHandler electricCollisionHandler)
    {
        List<ElectricNetworkConnector> neighboringConnectorsThatWillRemainInANetwork = new List<ElectricNetworkConnector>();

        neighboringConnectorsThatWillRemainInANetwork.AddRange(connectorToBeRemoved.connectedNodes);
        connectorToBeRemoved.RemoveBothSidedFromNetwork();

        // Reverse iteration, because the elements are removed while iterating through the collection. 
        for (int i = connectorToBeRemoved.connectedNodes.Count - 1; i >= 0; i--)
        {
            ElectricNetworkConnector connectedNode = connectorToBeRemoved.connectedNodes[i]; 
            connectorToBeRemoved.RemoveBothSidedFrom(connectedNode);

            if (connectedNode.connectedNodes.Count == 0)
            {
                connectedNode.RemoveBothSidedFromNetwork();
                neighboringConnectorsThatWillRemainInANetwork.Remove(connectedNode); 
            }
        }

        connectorToBeRemoved.Demolish();

        HandleNeighboringConnectorsNetworkResolvement(neighboringConnectorsThatWillRemainInANetwork); 
    }


    // Returns the Connector/Nodes, which are already there and get interacted with by the justAddedConnector. 
    // The justAddedConnector is not included in the returned value. 
    private ElectricNetworkConnector[] GetInteractedNetworkNodes(ElectricNetworkConnector justAddedConnector, Collider[] colliders)
    {
        List<ElectricNetworkConnector> connectorsList = new List<ElectricNetworkConnector>();

        foreach (Collider collider in colliders)
        {
            ElectricNetworkConnector connector = collider.transform.parent.gameObject.GetComponent<ElectricNetworkConnector>();

            if (connector == null)
                Debug.LogError("There is no ElectricNetworkConnector component connected to " 
                        + collider.transform.parent.gameObject + ". "); 

            if (connector == justAddedConnector)
                continue;

            connectorsList.Add(connector);
        }

        return connectorsList.ToArray();
    }


    private ElectricNetwork[] GetDifferentNetworksOf(ElectricNetworkConnector[] connectors)
    {
        List<ElectricNetwork> networks = new List<ElectricNetwork>();

        foreach (ElectricNetworkConnector connector in connectors)
        {
            if (connector.connectedNetwork == null)
                continue; 

            if (!networks.Contains(connector.connectedNetwork))
                networks.Add(connector.connectedNetwork);
        }

        return networks.ToArray(); 
    }


    private void HandleCreationOfANewNetwork()
    {
        ElectricNetwork network = CreateNewElectricNetwork(); 

        // Connect nodes to network 
        newlyAddedConnector.ConnectBothSidedTo(network); 

        // Connect each node with the network 
        foreach (ElectricNetworkConnector connector in interactedConnectors)
            connector.ConnectBothSidedTo(network);

        SortElectricNetworks();

        Debug.Log("New Network was created and added! ");
    }


    // Idealy, a new connector without a network gets added 
    // But it can happen, that the newly placed connector also connects with an orphan node. Then, 
    // this orphan node also has to be added to the single existing network, that gets an addon. 
    private void HandleAddonToAnExistingNetwork()
    {
        ElectricNetwork network = interactedConnectors[0].connectedNetwork;

        newlyAddedConnector.ConnectBothSidedTo(network);

        // Handle orphan connectors 
        foreach (ElectricNetworkConnector connector in interactedConnectors)
            if (connector.connectedNetwork == null)
                connector.ConnectBothSidedTo(network);

        SortElectricNetworks();
    }


    private void HandleAddonToMultipleExistingNetworks()
    {
        ElectricNetwork[] existingNetworks = GetDifferentNetworksOf(interactedConnectors);
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

        // Add placed down connetor to the network, all other networks get integrated into 
        newlyAddedConnector.ConnectBothSidedTo(biggestNetwork);

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
        List<ElectricNetworkConnector> integratedNodes = disintegratingNetwork.nodes;
        
        foreach (ElectricNetworkConnector node in integratedNodes)
            node.connectedNetwork = targetNetwork; 
        
        targetNetwork.nodes.AddRange(integratedNodes);

        DestroyElectricNetwork(disintegratingNetwork); 
    }
    

    private void SortElectricNetworks()
    {
        electricNetworks = ElectricNetwork.SortBySize(electricNetworks); 
    }


    public void ClearPreviewCablesOf(ElectricNetworkConnector previewConnector)
    {
        if (previewCables.Count == 0)
            return; 

        //foreach (ElectricNetworkCableConnection cableConnection in previewCables)
        //    Destroy(cableConnection.gameObject);
        //
        //previewCables.Clear(); 
        
        for (int i = previewCables.Count -1; i >= 0; i--)
        {
            previewConnector.cableConnections.Clear(); 
            Destroy(previewCables[i].gameObject); 
            previewCables.RemoveAt(i); 
        }

    }


    private void HandleNeighboringConnectorsNetworkResolvement(List<ElectricNetworkConnector> neighboringConnectors)
    {
        bool allNeighborsAreConnectedWithEachOther = true;

        // Case 1: There is only one neighbor 
        if (neighboringConnectors.Count == 1)
        {
            ElectricNetworkConnector connector = neighboringConnectors[0];

            if (connector.connectedNetwork.nodes.Count < 2)
            {
                electricNetworks.Remove(connector.connectedNetwork); 
                connector.RemoveBothSidedFromNetwork(); 
            }

            return; 
        }

        // Case 2: There are at least two neighbors and they are connected with each other 
        foreach (ElectricNetworkConnector neighborA in neighboringConnectors)
            foreach(ElectricNetworkConnector neighborB in neighboringConnectors)
            {
                if (neighborA == neighborB)
                    continue; 

                if (!neighborA.connectedNodes.Contains(neighborB))
                {
                    allNeighborsAreConnectedWithEachOther = false;
                    break;
                }
            }
        if (allNeighborsAreConnectedWithEachOther)
            Debug.Log("All Neighbors (neighbor = node with at least one connected node) are connected with each other! "); 

        if (allNeighborsAreConnectedWithEachOther) 
            return; 

        // Case 3: There are at least two neighbors and AT LEAST ONE is NOT connected with each other 
        NetworkResolver networkResolver = new NetworkResolver();
        List<List<ElectricNetworkConnector>> listsOfResolvedConnectors = 
                networkResolver.GetListsOfResolvedNodesAt(neighboringConnectors);

        foreach(List<ElectricNetworkConnector> resolvedConnectors in listsOfResolvedConnectors)
        {
            ElectricNetwork electricNetwork = CreateNewElectricNetwork(); 
            
            foreach (ElectricNetworkConnector connector in resolvedConnectors)
            {
                connector.RemoveBothSidedFromNetwork();
                connector.ConnectBothSidedTo(electricNetwork);
            }
        }

    }


    private void HandleElectricNetworkConnectorRemoval(ElectricNetwork network)
    {
        // If there are at least two connected nodes in network, do nothing 
        if (network.nodes.Count > 1)
            return; 

        // Else if there is only one or no node left, destroy the network 
        if (network.nodes.Count == 1)
            network.nodes[0].RemoveBothSidedFromNetwork();
        
        electricNetworks.Remove(network); 
    }


    // Class to recursively resolve networks, when a connector is being removed 
    private class NetworkResolver
    {

        List<ElectricNetworkConnector> resolvedNodes = new List<ElectricNetworkConnector>();

        
        public List<ElectricNetworkConnector> GetResolvedNodesAt(ElectricNetworkConnector node)
        {
            TraverseNode(node);
            return resolvedNodes; 
        }


        public List<List<ElectricNetworkConnector>> GetListsOfResolvedNodesAt(List<ElectricNetworkConnector> nodes)
        {
            List<NetworkResolver> networkResolver = new List<NetworkResolver>();
            List<List<ElectricNetworkConnector>> listOfResolvedNodes = new List<List<ElectricNetworkConnector>>();
            List<ElectricNetworkConnector> nodesToBeTraversed = new List<ElectricNetworkConnector>();

            nodesToBeTraversed.AddRange(nodes); 

            ElectricNetworkConnector currentConnector = nodesToBeTraversed[0]; 
            
            while (nodesToBeTraversed.Count > 0)
            {
                ElectricNetworkConnector currentlyTraversedNode = nodesToBeTraversed[0];
                NetworkResolver currentlyResolvedNetwork = new NetworkResolver();
                
                nodesToBeTraversed.Remove(currentlyTraversedNode);
                currentlyResolvedNetwork.TraverseNodeAndWatchOutForSubsequentlyTraversedNodes(
                        currentlyTraversedNode, nodesToBeTraversed);

                listOfResolvedNodes.Add(currentlyResolvedNetwork.resolvedNodes); 
            }
            
            return listOfResolvedNodes; 
        }


        private void TraverseNode(ElectricNetworkConnector node)
        {
            foreach (ElectricNetworkConnector childNode in node.connectedNodes)
            {
                if (resolvedNodes.Contains(childNode))
                    continue;

                resolvedNodes.Add(childNode);
                TraverseNode(childNode); 
            }
        }

        
        private void TraverseNodeAndWatchOutForSubsequentlyTraversedNodes(ElectricNetworkConnector node, 
                                                                          List<ElectricNetworkConnector> subsequentlyTraversedNodes)
        {
            foreach (ElectricNetworkConnector childNode in node.connectedNodes)
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

        private Dictionary<ElectricNetworkCableConnection, GameObject> debugLinesByConnection; 


        public DebugDrawer(List<ElectricNetwork> electricNetworks, GameObject debugConnectionLinePrefab)
        {
            this.electricNetworks = electricNetworks;
            this.debugConnectionLinePrefab = debugConnectionLinePrefab; 
        }


        public void CreateDebugLinesForNetworks()
        {
            debugLinesByConnection = new Dictionary<ElectricNetworkCableConnection, GameObject>(); 

            foreach (ElectricNetwork network in electricNetworks)
                foreach (ElectricNetworkCableConnection cable in network.edges)
                {
                    GameObject debugLine = Instantiate(debugConnectionLinePrefab);
                    debugLinesByConnection.Add(cable, debugLine);
                    Debug.Log("Debug Line created! ");
                }
        }
    }
}
