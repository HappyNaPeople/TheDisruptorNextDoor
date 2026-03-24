using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(PlayerInputData))]
public class PlayOneInputForDebug : MonoBehaviour
{
    public static PlayOneInputForDebug instance;
    public bool DebugOn = false;
    public GameObject playerInputManager;
    public GameObject playOneInput;

    PlayerInputData playerInputData;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        playerInputManager.SetActive(!DebugOn);
        playOneInput.SetActive(DebugOn);

        playerInputData.playerInput = playOneInput.GetComponent<PlayerInput>();
        playerInputData.inputSystemUIInputModule = playOneInput.GetComponent<InputSystemUIInputModule>();
    }

}
