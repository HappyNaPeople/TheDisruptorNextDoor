using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayOneInputForDebug : MonoBehaviour
{
    public static PlayOneInputForDebug instance;
    public static bool isOnDebug = true;
    [SerializeField] bool DebugOn = false;
    public GameObject playerInputManager;

    PlayerInputData playerInputData;

    private void Awake()
    {
        if (instance != null && instance != this || !isOnDebug)
        {
            Destroy(gameObject);
            Debug.Log("Destroyed PlayOneInputForDebug");
            return;
        }
        instance = this;

        isOnDebug = DebugOn;
        if (DebugOn)
        {
            if(playerInputManager != null)
            {
                Destroy(playerInputManager);
            }
        }
        else
        {
            isOnDebug = false;
            Destroy(gameObject);
            return;
        }

        playerInputData = GetComponent<PlayerInputData>();
    }
}
