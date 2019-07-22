using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            return; 
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


    public void CreateCableConnectionTo(ElectricNetworkConnector targetConnector)
    {
        GameObject cableConnectionGameObject = Instantiate(cableConnectionPrefab);
        ElectricNetworkCableConnection cableConnection = 
                cableConnectionGameObject.GetComponent<ElectricNetworkCableConnection>(); 

        cableConnection.Connect(this, targetConnector); 
        cableConnections.Add(cableConnection); 
    }

}
