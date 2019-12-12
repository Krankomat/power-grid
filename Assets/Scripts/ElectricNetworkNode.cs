using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ElectricNetworkNode 
{
    public ElectricNetwork connectedNetwork;
    public List<ElectricNetworkNode> connectedNodes = new List<ElectricNetworkNode>();
    public List<ElectricNetworkEdge> connectedEdges = new List<ElectricNetworkEdge>();
    public ElectricNetworkConnector connector; 

    public ElectricNetworkNode(ElectricNetworkConnector connector)
    {
        this.connector = connector; 
    }

    //TODO: Implement 
    private enum Type
    {
        Producer,
        Consumer,
        Transporter
    }
}
