using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ゲーム内で使用する Layer を名前から取得し、一括管理するクラス。
/// Runner、Platform、Trap などの LayerID を取得して保持する。
/// Layer が存在しない場合は Warning を出す。
/// 初期化は UseLayerName_Init() を通して一度だけ実行される。
/// </summary>
public static class UseLayerName
{
    // RunnerController が使用する Layer
    public static int runnerLayer;           // Runnerのレイヤー
    public static int platformLayer;         // Runnerが乗れるレイヤー
    public static int oneWayPlatformLayer;   // 下から乗れる足場
    public static int triggersLayer;         // Runnerが入ったときに検知するレイヤー
    // Map 用 Layer
    public static int mapLayer;              // 通常のマップ Layer
    // Hunter 用 Layer
    public static int trapLayer;             // Trap の Layer
    public static int runnerCantSeeLayer;    // Runner から見えない Layer
    /// <summary>
    /// Layer 名から LayerID を取得する。
    /// 存在しない Layer の場合は Warning を表示する。
    /// </summary>
    private static int GetLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);

        if (layer == -1)
        {
            Debug.LogWarning($"{layerName} Layer not exist");
        }

        return layer;
    }
    // Layer がすでに初期化されているかどうか
    private static bool isLayerSetUp = false;
    /// <summary>
    /// 使用する Layer を初期化する。
    /// 初期化は一度だけ実行される。
    /// </summary>
    public static void UseLayerName_Init()
    {
        if (isLayerSetUp) return;
        runnerLayer = GetLayer("Runner");
        platformLayer = GetLayer("Platform");
        oneWayPlatformLayer = GetLayer("OneWayPlatform");
        triggersLayer = GetLayer("Triggers");
        mapLayer = GetLayer("Default");
        trapLayer = GetLayer("Trap");
        runnerCantSeeLayer = GetLayer("RunnerCantSee");


        isLayerSetUp = true;
    }

}


/// <summary>
/// ゲーム全体を管理するマネージャークラス。
/// 
/// 主な役割：
/// ・入力デバイス（Keyboard / Mouse / Gamepad）の初期化
/// ・プレイヤーオブジェクトの生成と初期化
/// ・Runner / Hunter カメラの管理
/// ・デュアルディスプレイ設定
/// ・Layer 初期化
/// ・プレイヤーの役職（Runner / Hunter）の切り替え
/// 
/// シングルトンとして動作し、シーン遷移後も保持される。
/// </summary>
public class GameManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static GameManager Instance;
    // 入力デバイス管理クラス
    public static InputDevice inputDevice;     
    // プレイヤーごとのカメラ
    public Camera runnerCamera;
    public Camera hunterCamera;
    // Hunter 用 GamePad コントローラー
    public HunterConTrollerPad hunterConTrollerPad;
    // プレイヤーインスタンス
    public Player player01;
    public Player player02;
<<<<<<< HEAD
    /// <summary>
    /// 指定された Job に対応する Camera を取得する
    /// </summary>
    public Camera TargetCamera(Player.Job targetJob)
=======

    public Runner runner;

    private Camera TargetCamera(Player.Job targetJob)
>>>>>>> origin/Shota
    {
        switch (targetJob)
        {
            case Player.Job.Runner: return runnerCamera;
            case Player.Job.Hunter: return hunterCamera;
        }
        return null;
    }
    /// <summary>
    /// 指定された Gamepad を取得する
    /// 接続されていない場合はエラーを出す
    /// </summary>
    public Gamepad TargetGamepad(int targetCode)
    {
        if (inputDevice.gamepad.Count == 0) return null;
        if(targetCode >= inputDevice.gamepad.Count)
        {
            Debug.LogError($"Not this Decives, The Max connenting Device max are : {inputDevice.gamepad.Count}" );
            return null;
        }
        return inputDevice.gamepad[targetCode];
    }
    /// <summary>
    /// 入力デバイスの初期化
    /// Keyboard / Mouse / Gamepad を取得する
    /// </summary>
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
    /// <summary>
    /// プレイヤーオブジェクトを生成して初期化する
    /// </summary>
    private void PlayerInit()
    {
        GameObject playerGroup = new GameObject();
        GameObject player1 = new GameObject();
        GameObject player2 = new GameObject();

        playerGroup.name = "------Player------";
        player1.name = "Player01";
        player2.name = "Player02";
        player1.transform.parent = playerGroup.transform;
        player2.transform.parent = playerGroup.transform;

        player01 = player1.AddComponent<Player>();
        player02 = player2.AddComponent<Player>();

        // Gamepad が2つある場合はそれぞれ割り当て
        if (inputDevice.gamepad.Count >= 2)
        {
            player01.PlayerInit(Player.Job.Runner, runnerDisplay,0);
            player02.PlayerInit(Player.Job.Hunter, hunterDisplay,1);
        }
        else
        {
            player01.PlayerInit(Player.Job.Runner, runnerDisplay, 0);
            player02.PlayerInit(Player.Job.Hunter, hunterDisplay, 0);
        }
    }
    // Display番号
    private const int runnerDisplay = 0;
    private const int hunterDisplay = 1;
    /// <summary>
    /// カメラの表示先ディスプレイを設定する
    /// </summary>
    private void DisPlayInit()
    {
        runnerCamera.targetDisplay = runnerDisplay;
        hunterCamera.targetDisplay = hunterDisplay;
    }

<<<<<<< HEAD
    /// <summary>
    /// GameManager 全体の初期化処理
    /// </summary>
=======
    private void RunnerInit()
    {
        runner.RunnerInit();
        runner.ControllerCode = player01.controllerCode;
    }

>>>>>>> origin/Shota
    private void GameManager_Init()
    {
        InputInit();
        // デュアルディスプレイ対応
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }
        DisPlayInit();

        // Layer 初期化
        UseLayerName.UseLayerName_Init();

        PlayerInit();
<<<<<<< HEAD

=======
        RunnerInit();
>>>>>>> origin/Shota
    }

    /// <summary>
    /// Runner と Hunter の役職を入れ替える
    /// </summary>
    private void JobSwitch()
    {
        Player.Job job1 = player01.job;
        Player.Job job2 = player02.job;

        player01.SetJop(job2);
        player02.SetJop(job1);

        Camera cam1 = TargetCamera(job2);
        Camera cam2 = TargetCamera(job1);

        cam1.targetDisplay = player01.displayCode;
        cam2.targetDisplay = player02.displayCode;
<<<<<<< HEAD
        hunterConTrollerPad.HunterSwitch((player01.job == Player.Job.Hunter ? player01 : player02));
        
=======

        runner.SwitchController();
>>>>>>> origin/Shota
    }


    /// <summary>
    /// シングルトン初期化
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else Destroy(this);

    }

    private void Start()
    {
        GameManager_Init();
    }

    // デバッグ用
    public bool test;
    private void Update()
    {
        if (test)
        {
            JobSwitch();
            test = false;
        }


    }

}

