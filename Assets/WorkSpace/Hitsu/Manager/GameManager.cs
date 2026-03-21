using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

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
    public static int noPutAreaLayer;        // Ooshima: Added for StageGridManager (トラップ配置不可エリア)
    
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
        noPutAreaLayer = GetLayer("NoPutArea"); // Ooshima: Added for StageGridManager
        oneWayPlatformLayer = GetLayer("OneWayPlatform");
        triggersLayer = GetLayer("Triggers");
        trapLayer = GetLayer("Trap");
        runnerCantSeeLayer = GetLayer("RunnerCantSee");


        isLayerSetUp = true;
    }

}

/// <summary>
/// Trap の表示情報・データを保持するクラス。
///
/// 主な内容：
/// ・Trap アイコン
/// ・Trap 設置コスト
/// ・Trap 説明文
/// ・Trap プレハブ
/// </summary>
public class TrapInformation
{
    public Sprite icon;          // Trap アイコン
    public int cost;             // 設置コスト
    public string information;   // Trap 説明
    public GameObject prefab;    // Trap プレハブ
}

///// <summary>
///// ゲームで使用可能な Trap を管理するクラス。
/////
///// TrapName をキーとして、
///// TrapInformation を取得できる Dictionary。
///// </summary>
//public static class CanUseTrap
//{
//    /// <summary>
//    /// 使用可能 Trap 一覧
//    /// </summary>
//    public static Dictionary<TrapName, TrapInformation> allTrap = new Dictionary<TrapName, TrapInformation>();
//}

/// <summary>
/// プレイヤーの表示ディスプレイ番号
/// </summary>
public enum DisPlayNumber{ DisPlay01, DisPlay02, None }

/// <summary>
/// 使用するコントローラー番号
/// </summary>
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

    /// <summary>
    /// ゲームで使用可能な Trap 情報を保持する Dictionary。
    /// 
    /// Key : TrapName
    /// Value : TrapInformation
    /// 
    /// CSV から読み込んだ Trap データを登録し、
    /// UI や Trap 生成処理から参照される。
    /// 外部からは参照のみ可能で、
    /// 初期化は GameManager の TrapInit() で行われる。
    /// </summary>
    public static Dictionary<TrapName, TrapInformation> allTrap { get; }
        = new Dictionary<TrapName, TrapInformation>();

    [Header("プレイヤー")]
    // プレイヤーインスタンス
    public Player player01;
    public Player player02;

    /// <summary>
    /// 指定された Gamepad を取得する
    /// 接続されていない場合はエラーを出す
    /// </summary>
    public Gamepad TargetGamepad(Player targetPlyer)
    {
        // Gamepad 未接続
        if (inputDevice.gamepad.Count == 0) return null;
        // 指定番号の Gamepad が存在しない
        else if ((int)targetPlyer.controllerCode >= inputDevice.gamepad.Count)
        {
            Debug.LogWarning($"Not this Decives, The Max connenting Device max are : {inputDevice.gamepad.Count}" );
            return null;
        }

        // Gamepad 取得
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

        // Keyboard / Mouse 確認
        Debug.Log(inputDevice.mouse.name);
        Debug.Log(inputDevice.keyboard.name);

        // Gamepad 確認
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

        // Player グループ生成
        GameObject playerGroup = new GameObject();
        GameObject player1 = new GameObject();
        GameObject player2 = new GameObject();

        playerGroup.name = "------Player------";
        player1.name = "Player01";
        player2.name = "Player02";
        player1.transform.parent = playerGroup.transform;
        player2.transform.parent = playerGroup.transform;
        playerGroup.transform.parent = transform;

        // Player コンポーネント追加
        player01 = player1.AddComponent<Player>();
        player02 = player2.AddComponent<Player>();

        // Job 初期化
        player01.SetJop(Player.Job.None);
        player02.SetJop(Player.Job.None);

        // Controller 割り当て
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

    /// <summary>
    /// Trap CSV パス
    /// </summary>
    private const string csvPath = "Csv/TrapData/TrapData";

    /// <summary>
    /// CSV 読み込みデータ
    /// </summary>
    private string[] csvLines;

    /// <summary>
    /// CSV データの最大カラム数
    /// </summary>
    private const int dataIndexMax = 5;

    /// <summary>
    /// CSV 読み込み処理
    /// </summary>
    void LoadCSV()
    {
        string key = csvPath;

        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("CSV key null");
        }

        string path = Path.Combine(
            Application.streamingAssetsPath,
            key + ".csv"
        );

        // CSV 存在チェック
        if (!File.Exists(path))
        {
            Debug.LogWarning("文件不存在: " + path);
            return; 
        }

        // CSV 読み込み
        string[] lines = File.ReadAllLines(path, new UTF8Encoding(true));

        // 前後空白削除
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Trim();
        }

        csvLines = lines;
    }

    /// <summary>
    /// Trap データを初期化する。
    /// 
    /// 処理内容：
    /// ・CSV ファイルを読み込む
    /// ・TrapName に対応する TrapInformation を生成
    /// ・Icon / Cost / Information / Prefab を設定
    /// ・Dictionary(allTrap) に登録する
    /// </summary>
    private void TrapInit()
    {
        // CSV 読み込み
        LoadCSV();

        // CSV データが無い場合
        if (csvLines == null || csvLines.Length == 0)
        {
            Debug.LogError("CSV is Null");
            return;
        }

        // Trap Dictionary 初期化
        allTrap.Clear();

        // Enum の TrapName を順番に処理
        foreach (TrapName trapName in Enum.GetValues(typeof(TrapName)))
        {
            // CSV の行インデックス（1 行目はヘッダー想定）
            int index = (int)trapName + 1;
            // CSV 行不足チェック
            if (index >= csvLines.Length)
            {
                Debug.LogWarning($"CSV missing data for {trapName}");
                continue;
            }
            // CSV 行をカンマ区切りで分割
            string[] values = csvLines[index].Split(',');
            Debug.Log(string.Join(",", values));

            // CSV フォーマットチェック
            if (values.Length < dataIndexMax)
            {
                Debug.LogWarning($"{trapName} CSV format error");
                continue;
            }

            // TrapInformation 作成
            allTrap[trapName] = new TrapInformation();

            // ========================
            // Icon 読み込み
            // ========================
            Sprite icon = Resources.Load<Sprite>(values[1].Trim());
            if (icon != null) allTrap[trapName].icon = icon;
            else Debug.LogWarning($"{trapName} No Icon Data, Path:{values[1]}");

            // ========================
            // Cost 読み込み
            // ========================
            if (int.TryParse(values[2].Trim(), out int cost)) allTrap[trapName].cost = cost;
            else Debug.LogWarning($"{trapName} No Cost Data");

            // ========================
            // Information 読み込み
            // ========================
            if (!string.IsNullOrWhiteSpace(values[3])) allTrap[trapName].information = values[3];

            // ========================
            // Prefab 読み込み
            // ========================
            GameObject prefab = Resources.Load<GameObject>(values[4].Trim());
            if (prefab != null) allTrap[trapName].prefab = prefab;
            else Debug.LogWarning($"{trapName} No Prefab Data, Path:{values[4]}");
        }
        //    icon = Resources.Load<Sprite>("Texture/Traps/JumpPad"),
        //    cost = 5,
        //    information = "JumpPad",
        //    prefab = Resources.Load<GameObject>("Prefabs/Traps/JumpPad")

        // 読み込み完了ログ
        Debug.Log(allTrap.Count);

    }

    /// <summary>
    /// GameManager 全体初期化
    /// </summary>
    private void GameManager_Init()
    {
        // 入力初期化
        InputInit();
        // デュアルディスプレイ対応
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }

        // Layer 初期化
        UseLayerName.UseLayerName_Init();
        // Player 初期化
        PlayerInit();
        // Trap データ初期化
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
        else Destroy(gameObject);

        GameManager_Init();
    }
    private void Start()
    {

    }

    private IEnumerator SelectButtonWithDelay(MultiplayerEventSystem eventSystem, GameObject firstButton)
    {
        // 1フレームだけ待機して、EventSystemやUIの準備が完了するのを待つ
        yield return null;

        // 確実にフォーカスを当てるための小技（一度nullを入れてリセットしてから指定）
        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(firstButton);
    }

    public void Title_PlayerInputAssign(PlayerInputData playerInputData)
    {
        if (TitleUIManager.Instance == null) return;

        switch (playerInputData.playerIndex)
        {
            case 0:
                playerInputData.playerInput.camera = TitleUIManager.Instance.player01Camera;
                playerInputData.multiplayerEventSystem.playerRoot = TitleUIManager.Instance.player01TitlePlayerCanvas.gameObject;

                var p1StartButton = TitleUIManager.Instance.player01TitlePlayerCanvas.startButton.gameObject;
                playerInputData.multiplayerEventSystem.firstSelectedGameObject = p1StartButton;

                // ★直接呼ばずに、コルーチンを開始する
                playerInputData.StartCoroutine(SelectButtonWithDelay(playerInputData.multiplayerEventSystem, p1StartButton));
                break;

            case 1:
                playerInputData.playerInput.camera = TitleUIManager.Instance.player02Camera;
                playerInputData.multiplayerEventSystem.playerRoot = TitleUIManager.Instance.player02TitlePlayerCanvas.gameObject;

                var p2StartButton = TitleUIManager.Instance.player02TitlePlayerCanvas.startButton.gameObject;
                playerInputData.multiplayerEventSystem.firstSelectedGameObject = p2StartButton;

                // ★直接呼ばずに、コルーチンを開始する
                playerInputData.StartCoroutine(SelectButtonWithDelay(playerInputData.multiplayerEventSystem, p2StartButton));
                break;
        }
    }
}

