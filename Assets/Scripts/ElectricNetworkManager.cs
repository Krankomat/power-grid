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
            previewConnector.CreateCableConnectionTo(connector);

        previewCables.AddRange(previewConnector.cableConnections); 
    }


    public void HandleElectricNetworkNodeRemoval(ElectricNetworkConnector connectorToBeRemoved, CollisionHandler electricCollisionHandler)
    {
        connectorToBeRemoved.RemoveBothSidedFromNetwork();

        // Reverse iteration, because the elements are removed while iterating through the collection. 
        for (int i = connectorToBeRemoved.connectedNodes.Count - 1; i >= 0; i--)
        {
            ElectricNetworkConnector connectedNode = connectorToBeRemoved.connectedNodes[i]; 
            //connectorToBeRemoved.RemoveCableConnectionFrom(connectedNode);
            connectorToBeRemoved.RemoveBothSidedFrom(connectedNode);
            connectorToBeRemoved.Demolish(); 
        }

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

}
