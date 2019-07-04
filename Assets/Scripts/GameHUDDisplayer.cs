using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHUDDisplayer : MonoBehaviour
{

    public Text selectedObjectNameLabel;
    public Text selectedObjectDescriptionLabel; 
    

    public void RefreshContent(string objectName, string objectDescription)
    {
        selectedObjectNameLabel.text = objectName;
        selectedObjectDescriptionLabel.text = objectDescription; 
    }

}
