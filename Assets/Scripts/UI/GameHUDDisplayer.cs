using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHUDDisplayer : MonoBehaviour
{

    public Text selectedObjectNameLabel;
    public Text selectedObjectDescriptionLabel;
    public UIStateIndicator stateIndicator; 
    

    public void RefreshSelectionInfoPanel(string objectName, string objectDescription)
    {
        selectedObjectNameLabel.text = objectName;
        selectedObjectDescriptionLabel.text = objectDescription; 
    }


    public void DisplayStateIndicatorFor(InteractionState state)
    {
        switch (state)
        {
            case InteractionState.LookingAround:
            case InteractionState.InMenu:
                stateIndicator.currentState = UIStateIndicator.IndicatorState.None;
                break;

            case InteractionState.Placing:
                stateIndicator.currentState = UIStateIndicator.IndicatorState.Placing;
                break;

            case InteractionState.Demolishing: 
                stateIndicator.currentState = UIStateIndicator.IndicatorState.Demolishing;
                break; 

            default:
                Debug.Log("Unsupported Interaction State in " + gameObject);
                break; 
        }
    }

}
