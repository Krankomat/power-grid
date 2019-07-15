using System.Collections.Generic;
using UnityEngine;

public class ElectricNetwork 
{

    public int id;
    public List<ElectricNetworkConnector> connectedNodes; 


    private static int idCounter; 


    public ElectricNetwork()
    {
        id = idCounter;
        idCounter++; 
    }

}
