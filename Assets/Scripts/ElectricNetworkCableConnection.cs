using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricNetworkCableConnection : MonoBehaviour
{

    public GameObject cablePrefab;
    public ElectricNetworkConnector startConnector;
    public ElectricNetworkConnector endConnector;
    public ElectricNetwork connectedNetwork; 
    public bool isPreviewCable; 

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

        if (!isPreviewCable)
            ConnectBothSidedTo(startConnector.connectedNetwork);

        if (startConnector.connectedNetwork != endConnector.connectedNetwork)
            Debug.Log("ERROR: startConnector and endConnector in " + this + " have a different connectedNetwork. " 
                + "Start: " + startConnector.connectedNetwork + ", End: " + endConnector.connectedNetwork); 

        cableA.GetComponent<CableDrawer>().SetTransforms(startConnector.connectionPointA, endConnector.connectionPointA); 
        cableB.GetComponent<CableDrawer>().SetTransforms(startConnector.connectionPointB, endConnector.connectionPointB);

        startConnector.OnConnectorDemolished.AddListener(DestroyCable); 
        endConnector.OnConnectorDemolished.AddListener(DestroyCable);

        LinkConnectorDemolished(); 

        gameObject.transform.position = MathUtil.Midpoint(startConnector.transform.position, endConnector.transform.position); 
    }


    private void ConnectBothSidedTo(ElectricNetwork electricNetwork)
    {
        electricNetwork.cables.Add(this);
        connectedNetwork = electricNetwork;
        Debug.Log(this + " was added as edge to " + connectedNetwork);
    }


    private void RemoveBothSidedFromElectricNetwork()
    {
        Debug.Log(this + " was removed as edge from " + connectedNetwork);
        connectedNetwork.cables.Remove(this);
        connectedNetwork = null;
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

        // For some reason, this method (DestroyCable()) gets called twice when deleting the cable connection. 
        // TODO: Fix the exception and/or the second time calling the method. 
        if (!isPreviewCable)
            RemoveBothSidedFromElectricNetwork(); 

        Destroy(this.gameObject); 

    }

}
