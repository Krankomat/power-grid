using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricNetworkConnector : MonoBehaviour
{

    public GameObject electricNetworkNodeCollider; 
    /*[HideInEditor]*/ public List<ElectricNetwork> connectedElectricNetworks; 


    private enum RoleInElectricityNetwork
    {
        Producer, 
        ElectricityConsumer, 
        Transporter 
    }

}
