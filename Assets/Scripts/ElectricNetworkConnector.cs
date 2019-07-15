﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Synonym: ElectricNetworkNode 
public class ElectricNetworkConnector : MonoBehaviour
{
    
    public ElectricNetwork connectedNetwork = null;

    /*[HideInInspector]*/ public List<ElectricNetworkConnector> connectedNodes; 


    private enum RoleInElectricityNetwork
    {
        Producer, 
        ElectricityConsumer, 
        Transporter 
    }


    private void Awake()
    {
        connectedNodes = new List<ElectricNetworkConnector>(); 
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
    }

}
