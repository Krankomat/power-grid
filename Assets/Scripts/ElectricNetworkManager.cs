using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricNetworkManager : MonoBehaviour
{

    [HideInInspector] public List<ElectricNetwork> electricNetworks;


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

        ElectricNetworkConnector[] connectors = GetInteractedNetworkNodes(placedConnector, electricCollisionHandler.intersectingColliders);
        int numberOfInvolvedNetworksInConnectionAttempt = GetNumberOfInvolvedNetworksInConnectionAttempt(placedConnector, connectors);

        if (GameManager.Instance.isDebugging) 
            for (int i = 0; i < connectors.Length; i++) 
                Debug.Log(i + ": " + connectors[i]); 
        
        if (numberOfInvolvedNetworksInConnectionAttempt == 0)
        {
            // Create new network 
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


}
