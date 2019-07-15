using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricNetworkManager : MonoBehaviour
{

    [HideInInspector] public List<ElectricNetwork> electricNetworks; 

    private ElectricNetworkConnector newlyAddedConnector;
    private ElectricNetworkConnector[] interactedConnectors; 


    private void Awake()
    {
        electricNetworks = new List<ElectricNetwork>(); 
    }


    private void Update()
    {
        Debug.Log("There are currently " + electricNetworks.Count + " electronic networks. "); 
    }


    public void HandleElectricNetworkNodeAddOn(ElectricNetworkConnector placedConnector, CollisionHandler electricCollisionHandler)
    {
        newlyAddedConnector = placedConnector; 
        interactedConnectors = GetInteractedNetworkNodes(placedConnector, electricCollisionHandler.intersectingColliders);
        int numberOfInvolvedNetworksInConnectionAttempt = GetNumberOfInvolvedNetworksInConnectionAttempt(placedConnector, interactedConnectors);

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
        } else if (numberOfInvolvedNetworksInConnectionAttempt == 1)
        {
            // Connect to existing network 
        } else if (numberOfInvolvedNetworksInConnectionAttempt > 1)
        {
            // Check who is bigger network and then add to it 
        } else
        {
            Debug.LogError("There is an illegal number of involved networks (" 
                    + numberOfInvolvedNetworksInConnectionAttempt + ") when trying to add a new node. "); 
        }

        //Handle nodes connecting with each other 
        foreach (ElectricNetworkConnector connector in interactedConnectors)
            connector.ConnectBothSidedTo(newlyAddedConnector); 

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


    private int GetNumberOfInvolvedNetworksInConnectionAttempt(ElectricNetworkConnector justAddedconnector, ElectricNetworkConnector[] connectors)
    {
        int involvedNetworksCount = 0;

        foreach (ElectricNetworkConnector connector in connectors)
            if (connector.connectedNetwork != null)
                involvedNetworksCount++;

        return involvedNetworksCount; 
    }


    private void HandleCreationOfANewNetwork()
    {
        ElectricNetwork network = CreateNewElectricNetwork(); 

        // Connect nodes to network 
        newlyAddedConnector.ConnectBothSidedTo(network); 

        // Connect each node with the network 
        foreach (ElectricNetworkConnector connector in interactedConnectors)
            connector.ConnectBothSidedTo(network);

        Debug.Log("New Network was created and added! "); 
    }


    private ElectricNetwork CreateNewElectricNetwork()
    {
        ElectricNetwork network = new ElectricNetwork();
        electricNetworks.Add(network);

        return network; 
    }


    private void DestroyElectricNetwork(ElectricNetwork network)
    {
        electricNetworks.Remove(network);
        network = null; 
    }

}
