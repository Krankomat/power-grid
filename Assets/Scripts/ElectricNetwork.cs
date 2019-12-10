using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using CustomEvents;
using System;

[Serializable] 
public class ElectricNetwork 
{

    public int id;
    public List<ElectricNetworkConnector> connectedNodes = new List<ElectricNetworkConnector>();
    public List<ElectricNetworkCableConnection> cables;


    private static int idCounter;


    // Should not be called directly. 
    // Use ElectricNetworkManager.CreateElectricNetwork(), otherwise the network wont be added to the manager. 
    public ElectricNetwork()
    {
        id = idCounter;
        idCounter++; 
    }


    public override string ToString()
    {
        return base.ToString() + ", id: " + id; 
    }

    
    public static List<ElectricNetwork> SortBySize(List<ElectricNetwork> networks)
    {
        return networks.OrderByDescending(network => network.connectedNodes.Count).ToList();
    }


    public static ElectricNetwork[] SortBySize(ElectricNetwork[] networks)
    {
        List<ElectricNetwork> sortedNetworks = SortBySize(networks.ToList()); 
        return sortedNetworks.ToArray();
    }

}
