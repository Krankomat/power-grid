﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricNetworkCableConnection : MonoBehaviour
{

    public GameObject cablePrefab;
    public ElectricNetworkConnector connectorA;
    public ElectricNetworkConnector connectorB; 

    private GameObject cableA;
    private GameObject cableB;


    private void Awake()
    {
        cableA = Instantiate(cablePrefab, gameObject.transform);
        cableB = Instantiate(cablePrefab, gameObject.transform); 

    }
    

    public void Connect(ElectricNetworkConnector startConnector, ElectricNetworkConnector endConnector)
    {
        cableA.GetComponent<CableDrawer>().SetTransforms(startConnector.connectionPointA, endConnector.connectionPointA); 
        cableB.GetComponent<CableDrawer>().SetTransforms(startConnector.connectionPointB, endConnector.connectionPointB);

        gameObject.transform.position = MathUtil.Midpoint(startConnector.transform.position, endConnector.transform.position); 
    }

}