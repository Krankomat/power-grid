using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricNetworkInfoDrawer : MonoBehaviour
{
    public ElectricNetworkManager electricNetworkManager; 
    public GameObject networkElementPrefab;
    public GameObject networkPanelPrefab;
    public GameObject networkPanelContainer;

    private Dictionary<ElectricNetwork, ElectricNetworkPanel> panelByNetwork = new Dictionary<ElectricNetwork, ElectricNetworkPanel>(); 


    void Start()
    {
        RemoveExampleContent(); 
    }


    void Update()
    {
        CreateNewPanelsIfNecessary();
        RemoveOutdatedPanels(); 
    }


    private void CreateNewPanelsIfNecessary()
    {
        foreach (ElectricNetwork network in electricNetworkManager.electricNetworks)
        {
            if (panelByNetwork.ContainsKey(network))
                continue;

            GameObject networkPanelGameObject = Instantiate(networkPanelPrefab);
            networkPanelGameObject.transform.SetParent(networkPanelContainer.transform);
            ElectricNetworkPanel networkPanel = networkPanelGameObject.GetComponent<ElectricNetworkPanel>();
            networkPanel.SetTitle("Network " + network.id);
            panelByNetwork.Add(network, networkPanel);
        }
    }


    private void RemoveOutdatedPanels()
    {
        List<ElectricNetwork> possiblyOutdatedNetworks = new List<ElectricNetwork>(panelByNetwork.Keys);
        foreach (ElectricNetwork possiblyOutdatedNetwork in possiblyOutdatedNetworks)
        {
            if (electricNetworkManager.electricNetworks.Contains(possiblyOutdatedNetwork))
                continue;

            panelByNetwork[possiblyOutdatedNetwork].transform.SetParent(null);
            panelByNetwork.Remove(possiblyOutdatedNetwork);
        }
    }

    private void RemoveExampleContent()
    {
        networkPanelContainer.transform.DetachChildren(); 
    }
    
}
