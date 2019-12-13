using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ElectricNetworkUtil 
{
    // Method is called "Register" and not "Connect" to distinguish between creating a connection between two nodes 
    // and registering a given node or edge in a eletric network. 
    public static void Register(ElectricNetwork network, ElectricNetworkNode node)
    {
        // Normally, these two error messages should always come at the same time. 
        // But of course, it is possible that the connection is only one sided because of some error. 
        if (network.nodes.Contains(node))
            Debug.LogError($"ERROR REGISTERING: Network \"{network}\" already contains Node \"{node}\". ");
        network.nodes.Add(node);

        if (node.connectedNetwork == network)
            Debug.LogError($"ERROR REGISTERING: Node \"{node}\" is already linked to Network \"{network}\". ");
        node.connectedNetwork = network;
    }

    public static void Register(ElectricNetwork network, ElectricNetworkEdge edge)
    {
        if (network.edges.Contains(edge))
            Debug.LogError($"ERROR REGISTERING: Network \"{network}\" already contains Edge \"{edge}\". ");
        network.edges.Add(edge);

        if (edge.connectedNetwork == network)
            Debug.LogError($"ERROR REGISTERING: Edge \"{edge}\" is already linked to Network \"{network}\". ");
        edge.connectedNetwork = network;
    }

    public static void Unregister(ElectricNetwork network, ElectricNetworkNode node)
    {
        if (!network.nodes.Contains(node))
            Debug.LogError($"ERROR UNREGISTERING: Network \"{network}\" does not contain Node \"{node}\". ");
        network.nodes.Remove(node);

        if (node.connectedNetwork != network)
            Debug.LogError($"ERROR UNREGISTERING: Node \"{node}\" was not connected to Network \"{network}\" in the first place. ");
        node.connectedNetwork = null;
    }

    public static void Unregister(ElectricNetwork network, ElectricNetworkEdge edge)
    {
        if (!network.edges.Contains(edge))
            Debug.LogError($"ERROR UNLINKING: Network \"{network}\" does not contain Edge \"{edge}\". ");
        network.edges.Remove(edge);

        if (edge.connectedNetwork != network)
            Debug.LogError($"ERROR UNLINKING: Edge \"{edge}\" was not connected to Network \"{network}\" in the first place. ");
        edge.connectedNetwork = null;
    }

    //TODO: Iterate over electric network and create cable objects for each edge 


    public static void Connect(ElectricNetworkNode node1, ElectricNetworkNode node2)
    {
        // Connect nodes with each other 
        node1.connectedNodes.Add(node2);
        node2.connectedNodes.Add(node1);

        // Create edge between node1 and node2 
        ElectricNetworkEdge edge = new ElectricNetworkEdge(node1, node2);

        // Add edge to nodes 
        node1.connectedEdges.Add(edge);
        node2.connectedEdges.Add(edge);

        // Check if both nodes have the same network 
        if (node1.connectedNetwork != node2.connectedNetwork)
            Debug.LogError($"ERROR CONNECTING: Node 1 and Node 2 have a different network. " +
                $"Node 1: {node1} - {node1.connectedNetwork}; Node 2: {node2} - {node2.connectedNetwork}. ");

        // Register edge in network and vice versa
        Register(node1.connectedNetwork, edge);
    }


    // Unlike the other Connect method, this one is used to create a preview edge. This preview edge is shown, when 
    // hovering a building, so the according cables are displayed. The preview edge is only added to the previewNetwork. 
    public static void ConnectPreview(ElectricNetworkNode node1, ElectricNetworkNode node2, ElectricNetwork previewNetwork)
    {
        // Create preview edge between node1 and node2 
        ElectricNetworkEdge previewEdge = new ElectricNetworkEdge(node1, node2);
        previewEdge.type = ElectricNetworkEdge.Type.Preview;

        // Register edge in previewNetwork and vice versa
        Register(previewNetwork, previewEdge);
    }


    // Effectively destroys the edge 
    public static void Disconnect(ElectricNetworkEdge edge)
    {
        // Disconnect nodes from each other 
        ElectricNetworkNode node1 = edge.nodes.Item1;
        ElectricNetworkNode node2 = edge.nodes.Item2;
        node1.connectedNodes.Remove(node2);
        node2.connectedNodes.Remove(node1);

        // "Destroy" edge between nodes 
        edge.nodes.Item1.connectedEdges.Remove(edge);
        edge.nodes.Item2.connectedEdges.Remove(edge);

        // Unregister edge from network 
        Unregister(edge.connectedNetwork, edge);
    }


    public static void Disconnect(ElectricNetworkNode node1, ElectricNetworkNode node2)
    {
        // Check if both nodes are connected with each other 
        if (!node1.connectedNodes.Contains(node2) && !node2.connectedNodes.Contains(node1))
            Debug.LogError($"ERROR DISCONNECTING: Node 1 {node1} and Node 2 {node2} are not connected with each other. ");
        else if (!node1.connectedNodes.Contains(node2))
            Debug.LogError($"ERROR DISCONNECTING: Node 1 {node1} is not connected to Node 2 {node2}. ");
        else if (!node2.connectedNodes.Contains(node1))
            Debug.LogError($"ERROR DISCONNECTING: Node 2 {node2} is not connected to Node 1 {node1}. ");

        List<ElectricNetworkEdge> commonEdges = node1.connectedEdges.Intersect(node2.connectedEdges).ToList();
        if (commonEdges.Count() == 0)
            Debug.LogError($"ERROR DISCONNECTING: There is no common edge between Node 1 {node1} and Node 2 {node2}. ");
        else if (commonEdges.Count() > 1)
            Debug.LogError($"ERROR DISCONNECTING: There are {commonEdges.Count()} edges between Node 1 {node1} and Node 2 {node2}, " +
                $"but there should be only 1. ");
        else
            Disconnect(commonEdges[0]);
    }


    public static bool IsInSameNetwork(List<ElectricNetworkNode> nodes)
    {
        ElectricNetwork previousNetwork = nodes[0].connectedNetwork;
        foreach (ElectricNetworkNode node in nodes)
        {
            if (node.connectedNetwork != previousNetwork)
                return false;
            previousNetwork = node.connectedNetwork;
        }
        return true;
    }


    public static ElectricNetwork[] GetDifferentNetworksOf(ElectricNetworkNode[] nodes)
    {
        List<ElectricNetwork> networks = new List<ElectricNetwork>();

        foreach (ElectricNetworkNode node in nodes)
        {
            if (node.connectedNetwork == null)
                continue;

            if (!networks.Contains(node.connectedNetwork))
                networks.Add(node.connectedNetwork);
        }

        return networks.ToArray();
    }

}
