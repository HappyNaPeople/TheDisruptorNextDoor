using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(MultiplayerEventSystem))]
[RequireComponent(typeof(InputSystemUIInputModule))]
public class PlayerInputData : MonoBehaviour
{
    public int playerIndex;

    public PlayerInput playerInput;
    public MultiplayerEventSystem multiplayerEventSystem;
    public InputSystemUIInputModule inputSystemUIInputModule;
    // デバイスの入力があったときに生成され、Startが走る

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Assign();
    }

    void Assign()
    {
        playerInput = GetComponent<PlayerInput>();
        multiplayerEventSystem = GetComponent<MultiplayerEventSystem>();
        inputSystemUIInputModule = GetComponent<InputSystemUIInputModule>();

        playerIndex = playerInput.playerIndex;
        gameObject.name = $"PlayerInput_Player_{(playerIndex + 1).ToString("00")}";

        if(GameManager.Instance.player01.inputData == null)
        {
            GameManager.Instance.player01.inputData = this;
        }
        else if(GameManager.Instance.player02.inputData == null)
        {
            GameManager.Instance.player02.inputData = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        GameManager.Instance.Title_PlayerInputAssign(this);
        Debug.Log($"Player{playerIndex + 1} is assigned");
        Debug.Log($"Player{playerIndex + 1} device is {playerInput.devices[0].name}");
    }
}
