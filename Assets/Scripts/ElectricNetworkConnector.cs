using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Synonym: ElectricNetworkNode 
public class ElectricNetworkConnector : MonoBehaviour
{
    
    public ElectricNetwork connectedNetwork = null; 


    private enum RoleInElectricityNetwork
    {
        Producer, 
        ElectricityConsumer, 
        Transporter 
    }

}
