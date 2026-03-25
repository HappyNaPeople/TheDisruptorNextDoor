using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayOneInputForDebug : MonoBehaviour
{
    public static PlayOneInputForDebug instance;
    public static bool isOnDebug = false;
    [SerializeField] bool DebugOn = false;
    public GameObject playerInputManager;

    PlayerInputData playerInputData;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
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
            Destroy(gameObject);
            return;
        }

        playerInputData = GetComponent<PlayerInputData>();
    }
}
