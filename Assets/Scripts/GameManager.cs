using UnityEngine;

/** 
 * Singleton that handles data, which has to be available across multiple scenes. 
 */

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    public bool isDebugging;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Destroys the GameObject, to prevent pooling multiple Persistent Managers when switching scenes 
            // and calling Awake() multiple times. 
            Destroy(gameObject); 
        }
    }


    private void Update()
    {
        HandleInputToEnterDebugMode();
    }


    private void HandleInputToEnterDebugMode()
    {
        if (Input.GetKeyUp(KeyCode.F4))
        {
            isDebugging = !isDebugging;
        }
    }
}
