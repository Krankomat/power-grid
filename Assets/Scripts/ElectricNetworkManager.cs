using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElectricNetworkManager : MonoBehaviour
{

    public GameObject cablePrefab; 
    public List<ElectricNetwork> electricNetworks; 

    private ElectricNetworkConnector newlyAddedConnector;
    private ElectricNetworkConnector[] interactedConnectors;
    private List<ElectricNetworkCableConnection> previewCables; 


    private void Awake()
    {
        electricNetworks = new List<ElectricNetwork>();
        previewCables = new List<ElectricNetworkCableConnection>(); 
    }


    private void Update()
    {
        Debug.Log("There are currently " + electricNetworks.Count + " electronic networks. "); 
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
        List<ElectricNetworkConnector> integratedNodes = disintegratingNetwork.connectedNodes;
        
        foreach (ElectricNetworkConnector node in integratedNodes)
            node.connectedNetwork = targetNetwork; 
        
        targetNetwork.connectedNodes.AddRange(integratedNodes);

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

            if (connector.connectedNetwork.connectedNodes.Count < 2)
                connector.RemoveBothSidedFromNetwork(); 

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
        List<ElectricNetwork> resolvedNetworks = networkResolver.GetResolvedElectricNetworksFor(neighboringConnectors);

        electricNetworks.AddRange(resolvedNetworks);

        for (int i = 0; i < resolvedNetworks.Count; i++)
            Debug.Log("Resolved network " + i + " : There are " + resolvedNetworks[i].connectedNodes.Count + " nodes in it! ");

        if (resolvedNetworks.Count == 1)
            return; 

        foreach (ElectricNetwork resolvedNetwork in resolvedNetworks)
        {
            ElectricNetwork oldNetwork = resolvedNetwork.connectedNodes[0].connectedNetwork;
            SwapNetworkForAllConnectors(oldNetwork, resolvedNetwork);
            SwapCablesBetweenNetworks(oldNetwork, resolvedNetwork); 
        }

    }


    private static void SwapNetworkForAllConnectors(ElectricNetwork oldNetwork, ElectricNetwork newNetwork)
    {
        foreach (ElectricNetworkConnector node in newNetwork.connectedNodes)
        {
            node.RemoveBothSidedFromNetwork();
            node.ConnectBothSidedTo(newNetwork); 
        }
    }


    // It seems that there are actually no cables connected to a network when creating a connection 
    private static void SwapCablesBetweenNetworks(ElectricNetwork oldNetwork, ElectricNetwork newNetwork)
    {
        List<ElectricNetworkCableConnection> tempCables = new List<ElectricNetworkCableConnection>();

        if (oldNetwork.cables == null && newNetwork.cables == null)
            return; 

        tempCables.AddRange(oldNetwork.cables);
        oldNetwork.cables.Clear();
        oldNetwork.cables.AddRange(newNetwork.cables);
        newNetwork.cables.Clear();
        newNetwork.cables.AddRange(tempCables); 
    }


    // Class to recursively resolve networks, when a connector is being removed 
    private class NetworkResolver
    {

        ElectricNetwork resolverNetwork = new ElectricNetwork();

        
        public ElectricNetwork GetResolvedNetworkAt(ElectricNetworkConnector node)
        {
            TraverseNode(node);
            return resolverNetwork; 
        }


        public List<ElectricNetwork> GetResolvedElectricNetworksFor(List<ElectricNetworkConnector> nodes)
        {
            List<NetworkResolver> networkResolver = new List<NetworkResolver>(); 
            List<ElectricNetwork> resolvedNetworks = new List<ElectricNetwork>();
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

                resolvedNetworks.Add(currentlyResolvedNetwork.resolverNetwork); 
            }
            
            return resolvedNetworks; 
        }


        private void TraverseNode(ElectricNetworkConnector node)
        {
            foreach (ElectricNetworkConnector childNode in node.connectedNodes)
            {
                if (resolverNetwork.connectedNodes.Contains(childNode))
                    continue;

                resolverNetwork.connectedNodes.Add(childNode);
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

                if (resolverNetwork.connectedNodes.Contains(childNode))
                    continue;

                resolverNetwork.connectedNodes.Add(childNode);
                // Also transfer the cables referenced in the script? 
                TraverseNodeAndWatchOutForSubsequentlyTraversedNodes(childNode, subsequentlyTraversedNodes);
            }
        }
        

    }
}
