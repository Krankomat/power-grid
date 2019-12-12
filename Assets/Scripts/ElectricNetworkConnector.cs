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
        ElectricNetworkConnector[] interactedConnectors = 
            ElectricNetworkManager.GetInteractedNetworkConnectors(this, electricCollisionHandler.intersectingColliders);
        List<ElectricNetworkNode> interactedNodes = new List<ElectricNetworkNode>();
        foreach (ElectricNetworkConnector interactedConnector in interactedConnectors)
        {
            if (interactedConnector == this)
                continue;
            interactedNodes.Add(interactedConnector.node);
        }

        electricNetworkManager.HandleElectricNetworkNodeAddOn(this.node, interactedNodes);
    }


    public void Demolish()
    {
        OnConnectorDemolished.Invoke();
        Destroy(this); 
    }

}
