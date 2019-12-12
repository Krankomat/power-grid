using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using CustomEvents; 

public class ElectricNetworkConnector : MonoBehaviour
{

    public GameObject cableConnectionPrefab; 
    public Transform connectionPointA;
    public Transform connectionPointB; 
    public ElectricNetworkNode node; 
    
    public UnityEvent OnConnectorDemolished;
    

    private void  Awake()
    {
        node = new ElectricNetworkNode(this);
    }


    public void HandlePlacement(ElectricNetworkManager electricNetworkManager, CollisionHandler electricCollisionHandler)
    {
        List<ElectricNetworkNode> interactedNodes = GetInteractedNetworkNodes(this, electricCollisionHandler); 
        electricNetworkManager.HandleElectricNetworkNodeAddOn(node, interactedNodes);
    }


    public void ShowPlacementPreviewOfElectricNetworkNodeAddOn(ElectricNetworkManager electricNetworkManager, CollisionHandler electricCollisionHandler)
    {
        List<ElectricNetworkNode> interactedNodes = GetInteractedNetworkNodes(this, electricCollisionHandler);
        electricNetworkManager.HandlePreviewOfElectricNetworkNodeAddOn(node, interactedNodes);
    }


    private static List<ElectricNetworkNode> GetInteractedNetworkNodes(ElectricNetworkConnector triggeringConnector, CollisionHandler electricCollisionHandler)
    {
        ElectricNetworkConnector[] interactedConnectors = GetInteractedNetworkConnectors(triggeringConnector, electricCollisionHandler.intersectingColliders);
        List<ElectricNetworkNode> interactedNodes = new List<ElectricNetworkNode>();
        foreach (ElectricNetworkConnector interactedConnector in interactedConnectors)
        {
            if (interactedConnector == triggeringConnector)
                continue;
            interactedNodes.Add(interactedConnector.node);
        }
        return interactedNodes;
    }


    // Returns the Connector/Nodes, which are already there and get interacted with by the justAddedConnector. 
    // The justAddedConnector is not included in the returned value. 
    private static ElectricNetworkConnector[] GetInteractedNetworkConnectors(ElectricNetworkConnector justAddedConnector, Collider[] colliders)
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


    public void Demolish()
    {
        OnConnectorDemolished.Invoke();
        Destroy(this); 
    }

}
