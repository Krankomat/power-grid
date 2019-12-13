using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ElectricNetworkEdge 
{
    public ElectricNetwork connectedNetwork;
    public Tuple<ElectricNetworkNode, ElectricNetworkNode> nodes;
    public ElectricNetworkCableConnection cable;
    public Type type = Type.Normal; 

    public ElectricNetworkEdge(ElectricNetworkNode node1, ElectricNetworkNode node2)
    {
        nodes = new Tuple<ElectricNetworkNode, ElectricNetworkNode>(node1, node2); 
    }

    //TODO: Implement usage for type 
    public enum Type
    {
        Normal, 
        Preview
    }
}
