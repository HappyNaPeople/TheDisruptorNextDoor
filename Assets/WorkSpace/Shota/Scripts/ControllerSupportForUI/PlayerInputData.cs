using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(PlayerInput))]
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
        playerIndex = playerInput.playerIndex;


        if (!PlayOneInputForDebug.isOnDebug)
        {
            multiplayerEventSystem = GetComponent<MultiplayerEventSystem>();
        }
        inputSystemUIInputModule = GetComponent<InputSystemUIInputModule>();


        if (PlayOneInputForDebug.isOnDebug)
        {
            GameManager.Instance.player01.inputData = this;
            GameManager.Instance.player02.inputData = this;

            gameObject.name = $"PlayerInput_ForDebug";

            Debug.Log($"Player{1} is assigned");
            Debug.Log($"Player{2} is assigned");
            
            if(GameManager.Instance.currentScene == SceneState.InGame)
            {
                GameManager.Instance.Game_PlayerInputAssign();
            }
        }
        else
        {
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
            GameManager.Instance.Title_PlayerInputAssign(this, playerIndex);
            gameObject.name = $"PlayerInput_Player_{(playerIndex + 1).ToString("00")}";
            Debug.Log($"Player{playerIndex + 1} is assigned");
        }


    }

    public void SetFirstSelect(GameObject obj)
    {
        if (multiplayerEventSystem == null) return;

        multiplayerEventSystem.firstSelectedGameObject = obj;
    }

    public void SetPlayerRoot(GameObject obj)
    {
        if (multiplayerEventSystem == null) return;

        multiplayerEventSystem.playerRoot = obj;
    }

    public void SetInputCamera(Camera cam)
    {
        if (playerInput == null) return;
        playerInput.camera = cam;
    }

    public void SetActionMap(string name)
    {
        if (playerInput == null) return;

        playerInput.defaultActionMap = name;
        playerInput.SwitchCurrentActionMap(name);
    }

    public void SetSelect(GameObject obj)
    {
        if (multiplayerEventSystem == null) return;
        multiplayerEventSystem.SetSelectedGameObject(obj);
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
            if (context.phase == InputActionPhase.Performed) isJumpPressed = true;
            if (context.phase == InputActionPhase.Canceled) isJumpPressed = false;
        }
        else if (context.action.name == "Punch")
        {
            if (context.phase == InputActionPhase.Performed) isPunchPressed = true;
            if (context.phase == InputActionPhase.Canceled) isPunchPressed = false;
        }
    }
}