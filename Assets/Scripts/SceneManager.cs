using UnityEngine;

public class SceneManager : MonoBehaviour
{

    public GameObject debuggingModeLabel;


    private void Update()
    {
        
        if (GameManager.Instance.isDebugging)
            debuggingModeLabel.SetActive(true);
        else
            debuggingModeLabel.SetActive(false);
    }

}
