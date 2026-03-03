using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static InputDevice inputDevice;

    private const int runnerDisplay = 0;
    private const int hunterDisplay = 1;

    private void InputInit()
    {
        inputDevice = new InputDevice();
        inputDevice.InputInit();

        Debug.Log(inputDevice.mouse.name);
        Debug.Log(inputDevice.keyboard.name);

        if (inputDevice.gamepad.Count > 0)
        {
            for (int i = 0; i < inputDevice.gamepad.Count; i++)
            {
                Debug.Log(inputDevice.gamepad[i].name);
            }
        }

    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else Destroy(this);

        InputInit();
        Camera.main.targetDisplay = 0;

        InputInit();
    }

    private void Start()
    {
        InputInit();
    }



}

