using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricNetworkManager : MonoBehaviour
{

    [HideInInspector] public List<ElectricNetwork> electricNetworks; 
    

    private void Update()
    {
        Debug.Log("There are currently " + electricNetworks.Count + " electronic networks. "); 
    }

}
