using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

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

    public static int oneWayPlatformLayer;   // 下から乗れる足場
    public static int triggersLayer;         // Runnerが入ったときに検知するレイヤー

    // Map 用 Layer
    public static int platformLayer;         // Runnerが乗れるレイヤー
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
        trapLayer = GetLayer("Trap");
        runnerCantSeeLayer = GetLayer("RunnerCantSee");


        isLayerSetUp = true;
    }

}
public class TrapInformation
{
    public Sprite icon;
    public int cost;
    public string information;
    public GameObject prefab;
}

public static class CanUseTrap
{
    public static Dictionary<TrapName, TrapInformation> allTrap = new Dictionary<TrapName, TrapInformation>();
}

public enum DisPlayNumber{ DisPlay01, DisPlay02, None }
public enum ControllerNumber{ Controller01, Controller02, None }
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

    // プレイヤーインスタンス
    public Player player01;
    public Player player02;

    /// <summary>
    /// 指定された Gamepad を取得する
    /// 接続されていない場合はエラーを出す
    /// </summary>
    public Gamepad TargetGamepad(Player targetPlyer)
    {
        if (inputDevice.gamepad.Count == 0) return null;
        else if((int)targetPlyer.controllerCode >= inputDevice.gamepad.Count)
        {
            Debug.LogWarning($"Not this Decives, The Max connenting Device max are : {inputDevice.gamepad.Count}" );
            return null;
        }
        return inputDevice.gamepad[(int)targetPlyer.controllerCode];
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
        playerGroup.transform.parent = transform;

        player01 = player1.AddComponent<Player>();
        player02 = player2.AddComponent<Player>();

        player01.SetJop(Player.Job.None);
        player02.SetJop(Player.Job.None);

        if (GameManager.inputDevice.gamepad.Count >= 2)
        {
            player01.PlayerInit(DisPlayNumber.DisPlay01, ControllerNumber.Controller01);
            player02.PlayerInit(DisPlayNumber.DisPlay02, ControllerNumber.Controller02);
        }
        else
        {
            player01.PlayerInit(DisPlayNumber.DisPlay01, ControllerNumber.Controller01);
            player02.PlayerInit(DisPlayNumber.DisPlay02, ControllerNumber.Controller01);
        }

    }
    private void TrapInit()
    {
        //foreach (TrapName trapName in Enum.GetValues(typeof(TrapName)))
        //{
        //    CanUseTrap.allTrap[trapName] = new TrapInformation();
        //}

        CanUseTrap.allTrap[TrapName.Spikes] = new TrapInformation()
        {
            icon = Resources.Load<Sprite>("Texture/Traps/Spike"),
            cost = 2,
            information = "Spikes",
            prefab = Resources.Load<GameObject>("Prefabs/Traps/Spikes")
        };
        CanUseTrap.allTrap[TrapName.FallRock] = new TrapInformation()
        {
            icon = Resources.Load<Sprite>("Texture/Traps/FallRock"),
            cost = 4,
            information = "FallRock",
            prefab = Resources.Load<GameObject>("Prefabs/Traps/FallRock")
        };
        CanUseTrap.allTrap[TrapName.Boom] = new TrapInformation()
        {
            icon = Resources.Load<Sprite>("Texture/Traps/Boom"),
            cost = 8,
            information = "Boom",
            prefab = Resources.Load<GameObject>("Prefabs/Traps/Boom")
        };
        CanUseTrap.allTrap[TrapName.JumpPad] = new TrapInformation()
        {
            icon = Resources.Load<Sprite>("Texture/Traps/JumpPad"),
            cost = 5,
            information = "JumpPad",
            prefab = Resources.Load<GameObject>("Prefabs/Traps/JumpPad")
        };



    }

    private void GameManager_Init()
    {
        InputInit();
        // デュアルディスプレイ対応
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }

        // Layer 初期化
        UseLayerName.UseLayerName_Init();
        PlayerInit();
        TrapInit();
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

        GameManager_Init();
    }
    private void Start()
    {

    }



}

