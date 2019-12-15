using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricNetworkCableConnection : MonoBehaviour
{
    public ElectricNetworkEdge edge; 
    public GameObject cablePrefab;
    private GameObject cableA;
    private GameObject cableB;
    public int Id { get { return gameObject.GetInstanceID(); } }

    //TODO: Optimize by only updating position of cables in CableDrawer, when edge.type == ElectricNetworkEdge.Type.Preview is. 
    // But not necessary at this point (and it could lead to some inconsistencies when moving things around in the editor). 

    private void Awake()
    {
        edge = new ElectricNetworkEdge(null, null); 
        cableA = Instantiate(cablePrefab, gameObject.transform);
        cableB = Instantiate(cablePrefab, gameObject.transform); 
    }


    private void Start()
    {
        gameObject.name += $"#{Id}";
    }


    public void Connect()
    {
        // Check if edge exists 
        if (edge == null)
        {
            Debug.LogError($"ERROR CONNECTING CABLE: Edge in {this} is null. ");
            return; 
        }

        // Check if the nodes in the pair are not null 
        if (edge.nodes.Item1 == null || edge.nodes.Item2 == null)
            Debug.LogError($"ERROR CONNECTING CABLE: One or both of the edge nodes are null. " +
                $"Node 1: {edge.nodes.Item1}; Node 2: {edge.nodes.Item2}. ");

        // Actually Connect 
        ElectricNetworkConnector connector1 = edge.nodes.Item1.connector; 
        ElectricNetworkConnector connector2 = edge.nodes.Item2.connector;
        Connect(connector1, connector2); 
    }


    /*
     * Should not be called directly; instead, add an edge and simply call Connect() 
     */ 
    public void Connect(ElectricNetworkConnector startConnector, ElectricNetworkConnector endConnector)
    {
        cableA.GetComponent<CableDrawer>().SetTransforms(startConnector.connectionPointA, endConnector.connectionPointA); 
        cableB.GetComponent<CableDrawer>().SetTransforms(startConnector.connectionPointB, endConnector.connectionPointB);

        gameObject.transform.position = MathUtil.Midpoint(startConnector.transform.position, endConnector.transform.position); 
    }

}
