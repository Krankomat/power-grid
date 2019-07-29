using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Synonym: ElectricNetworkNode 
public class ElectricNetworkConnector : MonoBehaviour
{

    public GameObject cableConnectionPrefab; 
    public Transform connectionPointA;
    public Transform connectionPointB; 
    public ElectricNetwork connectedNetwork = null;
    /* for debugging */ public string connectedNetworkString; 
    /*[HideInInspector]*/ public List<ElectricNetworkConnector> connectedNodes;
    public List<ElectricNetworkCableConnection> cableConnections;


    public UnityEvent OnConnectorDemolished; 


    private enum RoleInElectricityNetwork
    {
        Producer, 
        ElectricityConsumer, 
        Transporter 
    }


    private void  Awake()
    {
        connectedNodes = new List<ElectricNetworkConnector>(); 
    }


    private void Update()
    {
        if (connectedNetwork == null)
            connectedNetworkString = ""; 
        else 
            connectedNetworkString = connectedNetwork.ToString(); 
    }


    public void ConnectBothSidedTo(ElectricNetwork network)
    {
        // Add network to this 
        connectedNetwork = network;

        // Add this to network 
        if(network.connectedNodes.Contains(this))
        {
            Debug.LogError("There already is a connector " + this + " connected to network "
                    + network + ", even though it shouldn't be. ");
            return; 
        }

        network.connectedNodes.Add(this); 
    }


    public void RemoveBothSidedFromNetwork()
    {
        if (connectedNetwork == null)
        {
            Debug.LogError("This ElectricnetworkConnector does not have a electric network, even though it should, " +
                    "because RemoveBothSidedFromNetwork() is called. ");
            return; 
        }

        if (!connectedNetwork.connectedNodes.Contains(this))
        {
            Debug.LogError("This ElectricNetworkConnector has a connection listed to a network, " +
                    "but the network itself does not have a connection to the connector. "); 
            return; 
        }

        connectedNetwork.connectedNodes.Remove(this); 
        connectedNetwork = null; 
    }


    public void ConnectBothSidedTo(ElectricNetworkConnector targetConnector)
    {
        if (connectedNodes.Contains(targetConnector))
        {
            Debug.Log("This connector " + this + " is already connected to target node " + targetConnector + ". ");
            return; 
        }

        if (targetConnector.connectedNodes.Contains(this))
        {
            Debug.Log("The target node " + targetConnector + " is already connected this node " + this + ". ");
            return;
        }

        connectedNodes.Add(targetConnector);
        targetConnector.connectedNodes.Add(this);

        CreateCableConnectionTo(targetConnector);
    }


    public void RemoveBothSidedFrom(ElectricNetworkConnector targetConnector)
    {
        if (!connectedNodes.Contains(targetConnector))
        {
            Debug.LogError("This connector does not contain the target connector as connected node, even tough it should. ");
            return; 
        }

        if (!targetConnector.connectedNodes.Contains(this))
        {
            Debug.LogError("The target connector does not contain this connector as connected node, even though it should. ");
            return; 
        }

        targetConnector.connectedNodes.Remove(this);
        connectedNodes.Remove(targetConnector); 

    }


    public void CreateCableConnectionTo(ElectricNetworkConnector targetConnector, bool isPreview = false)
    {
        GameObject cableConnectionGameObject = Instantiate(cableConnectionPrefab);
        ElectricNetworkCableConnection cableConnection = 
                cableConnectionGameObject.GetComponent<ElectricNetworkCableConnection>();

        if (isPreview)
            cableConnection.isPreviewCable = true; 

        // Ordering is important! ConnectorA should always be this connector! See RemoveCableConnectionFrom() for details. 
        cableConnection.Connect(this, targetConnector);
        cableConnections.Add(cableConnection); 
    }


    // It actually would be more efficient to create a dictionary with all connections at a singleton like the scene manager. 
    // The problem is, that this actually makes it more complicated, because now non-static classes access a static class 
    // for dynamically generated game objects. Because this seems careless at the moment, it is solved this (complicated) way. 
    public void RemoveCableConnectionFrom(ElectricNetworkConnector targetConnector)
    {
        GameObject cableToBeRemoved = null;
        ElectricNetworkCableConnection cableConnectionToBeRemoved = null;
        bool cableWasNotConnectedToThisButToTargetConnector = false; 

        // Either the cable is linked to this connector ... 
        foreach (ElectricNetworkCableConnection cableConnection in cableConnections)
            // Connector A should always be the the connector, which initially called the cable creation method. 
            if (cableConnection.endConnector == targetConnector)
            {
                cableToBeRemoved = cableConnection.gameObject;
                cableConnectionToBeRemoved = cableConnection;
                break; 
            }

        // ... or the cable is linked to the target connector. 
        if (cableToBeRemoved == null)
            foreach (ElectricNetworkCableConnection cableConnection in targetConnector.cableConnections)
                if (cableConnection.endConnector == this)
                {
                    cableToBeRemoved = cableConnection.gameObject;
                    cableConnectionToBeRemoved = cableConnection;
                    cableWasNotConnectedToThisButToTargetConnector = true; 
                    break;
                }

        // But if there still is no cable, then there's a problem. 
        if (cableToBeRemoved == null)
        {
            Debug.LogError("Cable connection could no be removed. " +
                    "There is no cable connection between " + this + " and " + targetConnector + ". ");
            return; 
        }

        // Actually destroy the connection 
        if (cableWasNotConnectedToThisButToTargetConnector)
            targetConnector.cableConnections.Remove(cableConnectionToBeRemoved);
        else
            cableConnections.Remove(cableConnectionToBeRemoved);

        Destroy(cableToBeRemoved); 
    }


    public void Demolish()
    {
        OnConnectorDemolished.Invoke();
        Destroy(this); 
    }

}
