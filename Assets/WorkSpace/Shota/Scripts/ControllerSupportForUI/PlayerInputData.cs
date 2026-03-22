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

    public Vector2 moveInput;
    public bool isJumpPressed;
    public bool isPunchPressed;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        playerInput = GetComponent<PlayerInput>();
    }

    // --- Invoke C Sharp Events 用のイベント登録 ---
    private void OnEnable()
    {
        if (playerInput != null)
        {
            playerInput.onActionTriggered += OnActionTriggered;
        }
    }

    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.onActionTriggered -= OnActionTriggered;
        }
    }

    void Start()
    {
        Assign();
    }

    void Assign()
    {
        multiplayerEventSystem = GetComponent<MultiplayerEventSystem>();
        inputSystemUIInputModule = GetComponent<InputSystemUIInputModule>();

        playerIndex = playerInput.playerIndex;
        gameObject.name = $"PlayerInput_Player_{(playerIndex + 1).ToString("00")}";

        if (GameManager.Instance.player01.inputData == null)
        {
            GameManager.Instance.player01.inputData = this;
        }
        else if (GameManager.Instance.player02.inputData == null)
        {
            GameManager.Instance.player02.inputData = this;
        }
        else
        {
            // ここに入るとオブジェクトが破棄され、入力は一切効かなくなります
            Debug.LogWarning("PlayerInputが多すぎるため破棄されました");
            Destroy(gameObject);
            return;
        }

        GameManager.Instance.Title_PlayerInputAssign(this);
        Debug.Log($"Player{playerIndex + 1} is assigned");
    }

    // --- 入力があった時に呼ばれる統合メソッド ---
    private void OnActionTriggered(InputAction.CallbackContext context)
    {
        // アクション名（文字列）で判定して処理を振り分けます
        if (context.action.name == "Move")
        {
            moveInput = context.ReadValue<Vector2>();
        }
        else if (context.action.name == "Jump")
        {
            isJumpPressed = context.ReadValueAsButton();
        }
        else if (context.action.name == "Punch")
        {
            isPunchPressed = context.ReadValueAsButton();
        }
    }
}