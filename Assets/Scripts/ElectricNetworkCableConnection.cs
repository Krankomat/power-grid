using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricNetworkCableConnection : MonoBehaviour
{

    public GameObject cablePrefab;
    public ElectricNetworkConnector startConnector;
    public ElectricNetworkConnector endConnector; 

    private GameObject cableA;
    private GameObject cableB;


    private void Awake()
    {
        cableA = Instantiate(cablePrefab, gameObject.transform);
        cableB = Instantiate(cablePrefab, gameObject.transform); 

    }
    

    public void Connect(ElectricNetworkConnector startConnector, ElectricNetworkConnector endConnector)
    {
        this.startConnector = startConnector;
        this.endConnector = endConnector; 

        cableA.GetComponent<CableDrawer>().SetTransforms(startConnector.connectionPointA, endConnector.connectionPointA); 
        cableB.GetComponent<CableDrawer>().SetTransforms(startConnector.connectionPointB, endConnector.connectionPointB);

        startConnector.OnConnectorDemolished.AddListener(DestroyCable); 
        endConnector.OnConnectorDemolished.AddListener(DestroyCable);

        LinkConnectorDemolished(); 

        gameObject.transform.position = MathUtil.Midpoint(startConnector.transform.position, endConnector.transform.position); 
    }


    private void LinkConnectorDemolished()
    {
        startConnector.OnConnectorDemolished.AddListener(DestroyCable);
        endConnector.OnConnectorDemolished.AddListener(DestroyCable);
    }


    private void UnlinkConnectorDemolished()
    {
        startConnector.OnConnectorDemolished.RemoveListener(DestroyCable);
        endConnector.OnConnectorDemolished.RemoveListener(DestroyCable);
    }


    private void DestroyCable()
    {
        UnlinkConnectorDemolished();

        startConnector.cableConnections.Remove(this);
        // TODO? When creating a cable, add it as a cable connection to both power poles, but notify who's the owner 

        Destroy(this.gameObject); 

    }

}
