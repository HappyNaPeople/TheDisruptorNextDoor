using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using UnityEngine.SceneManagement;
using static Player;
using Unity.Cinemachine;
using static InGame;
using Unity.VisualScripting;
using static Unity.Collections.Unicode;


[System.Serializable]
public class CameraData
{
    // 通常カメラ
    public Camera camera;
    // Cinemachineカメラ本体
    public CinemachineCamera cinemachineCamera;
    // Followコンポーネント（追従処理）
    private CinemachineFollow cinemaChineFollow;
    // 初期のズーム値（Orthographic Size）
    private float lens;
    // 初期のFollowオフセット
    private Vector3 basicFollowOffset;
    // 初期のDamping値（追従の滑らかさ）
    private Vector3 basicPositionDamping;
    // Ready状態で使うDamping
    private readonly Vector3 readyV3 = new Vector3(0.1f, 0.1f, 0.1f);
    /// <summary>
    /// カメラの初期状態を取得・保存
    /// </summary>
    public void CameraInit()
    {
        // Followコンポーネント取得
        cinemaChineFollow = cinemachineCamera.GetComponent<CinemachineFollow>();
        // 現在のズーム値を保存
        lens = cinemachineCamera.Lens.OrthographicSize;
        // 現在のFollowオフセットを保存
        basicFollowOffset = cinemaChineFollow.FollowOffset;
        // 現在のDampingを保存（Cinemachine 3.x仕様）
        basicPositionDamping = cinemaChineFollow.TrackerSettings.PositionDamping;
    }

    /// <summary>
    /// ゲーム開始前の演出用カメラ状態
    /// </summary>
    public void Ready()
    {
        cinemachineCamera.Lens.OrthographicSize = 9;
        cinemaChineFollow.FollowOffset = readyV3;
    }

    /// <summary>
    /// Dampingのみ初期状態に戻す
    /// </summary>
    /// </summary>
    public void ReSetLens()
    {
        cinemachineCamera.Lens.OrthographicSize = lens;
        cinemaChineFollow.FollowOffset = basicFollowOffset;

    }
    /// <summary>
    /// ゲーム開始時にカメラを通常状態へ戻す
    /// </summary>
    //public void GameStart(Transform trackingTarget)
    //{
    //    // 初期ズームに戻す
    //    cinemachineCamera.Lens.OrthographicSize = lens;
    //    // 初期オフセットに戻す
    //    cinemaChineFollow.FollowOffset = basicFollowOffset;
    //    // 追従対象を設定
    //    cinemachineCamera.Target.TrackingTarget = trackingTarget;
    //}

}

/// <summary>
/// GameStage を定義し、ゲーム全体の進行状態を明確に管理
/// 初期化 → ラウンド → プレイ → 終了 という流れを段階的に表現
/// </summary>
public enum GameStage
{
    // =========================
    // ゲーム初期化フェーズ
    // =========================

    InGameInitStart, // InGameの初期化開始
    InGameInitDone,  // 初期化完了

    // =========================
    // ラウンド進行フェーズ
    // =========================

    RoundInit,  // ラウンド開始前の準備
    Ready,      // プレイヤー準備状態（カウントダウンなど）
    Playing,    // 実際のゲームプレイ中
    EndRound,   // ラウンド終了処理

    // =========================
    // ゲーム終了フェーズ
    // =========================

    GameSet,    // ゲーム終了（結果表示など）

    // =========================
    // 例外・エラー状態
    // =========================

    Error       // 想定外の状態
}

/// <summary>
/// ゲー??の進行管?ク?ス。
///
/// 主な役?：
/// ・Runner / Hunter の役職管?
/// ・カ??表示先の制御
/// ・ター?切り替え??
/// ・ゲー?タイマー管?
/// ・Hunter UI の更新
///
/// GameManager から Player 情報を取得し、
/// 各プ?イ?ーの役?と画面表示を管?する。
/// </summary>
public class InGame : MonoBehaviour
{
    /// <summary>
    /// Singleton イ?スタ?ス
    /// </summary>
    public static InGame Instance;

    [Header("Audio")]
    public BgmData bgmData;

    [Header("Camera")]
    /// <summary>
    /// Runner用カメラデータ
    /// プレイヤー（逃げる側）の視点制御を担当
    /// </summary>
    public CameraData runnerCamera;

    /// <summary>
    /// Hunter用カメラデータ
    /// 追いかける側の視点制御を担当
    /// </summary>
    public CameraData hunterCamera;

    /// <summary>
    /// 指定された Job に対?する Camera を取得する
    /// </summary>
    private Camera TargetCamera(Player.Job targetJob)
    {
        switch (targetJob)
        {
            case Player.Job.Runner: return runnerCamera.camera;
            case Player.Job.Hunter: return hunterCamera.camera;
        }
        return null;
    }

    [Header("Player")]
    // Hunter 用 GamePad コ?ト?ー?ー
    public HunterConTrollerPad hunterConTrollerPad;
    public RunnerConTrollerPad runnerConTrollerPad;

    // Runner プ?イ?ー
    public Runner runner;

    // GameManager から取得するプ?イ?ーイ?スタ?ス
    public Player _player01 => GameManager.Instance.player01;
    public Player _player02 => GameManager.Instance.player02;


    public GameStage gameStage;

    #region Timer

    /// <summary>
    /// タイマー開始値
    /// </summary>
    public const float timerStart = 150.0f;

    [Header("Time")]
    /// <summary>
    /// 現在の残り?間
    /// </summary>
    public float timer;

    /// <summary>
    /// タイ?アップ判定
    /// </summary>
    public bool timesUp => timer <= 0;

    public int NowTimeToInt() => (int)timer;

    // タイマー Coroutine
    private Coroutine timerCountDown;

    /// <summary>
    /// タイマー開始??
    /// </summary>
    private void TimerStart()
    {
        // 既存タイマー停止
        if (timerCountDown != null) StopCoroutine(timerCountDown);
        // タイマー?期化
        timer = timerStart;
        //Debug.Log(timer);
        // カウ?トダウ?開始
        timerCountDown = StartCoroutine(TimerCountDown());
    }

    /// <summary>
    /// タイマーを減少させる Coroutine
    /// </summary>
    private IEnumerator TimerCountDown()
    {
        // ?間が 0 になるまで減少
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        TurnSwitch();
    }

    #endregion

    #region Map

    [Header("Map")]

    public MapBasic useMap; /* {  get; private set; }*/

    public void SettingMap(MapBasic target) => useMap = target;
    /// <summary>
    /// スタート地点
    /// </summary>
    public Transform startingPoint;
    public void SetUpStartingPoint(Transform target) => startingPoint = target;

    /// <summary>
    /// ゴール地点
    /// </summary>
    public Transform goal;
    public void SetUpGoal(Transform target) => goal = target;

    /// <summary>
    /// チェックポイント一覧
    /// </summary>
    public List<Transform> checkPoints;

    public void SetUpCheckPoints(Transform[] target)
    {
        if (target == null)
        {
            Debug.LogWarning("Cant setup the CheckPoints");
            return;
        }
        checkPoints.Clear();
        checkPoints.AddRange(target);

        CheckPointsDictInit();
    }


    /// <summary>
    /// リスポーン地点
    /// </summary>
    public Transform playerRespawnTs;

    /// <summary>
    /// チェックポイント初期化
    /// ・全て未通過にリセット
    /// </summary>
    private void CheckPointsDictInit()
    {
        // 既存データをクリア
        checkPointsDict.Clear();

        // 全チェックポイントを未通過（false）で登録
        useMap.ResetCheckPoints();
        foreach (Transform transform in checkPoints)
        {
            checkPointsDict[transform] = false;
        }

        // 通過数リセット
        passCheckPoint = 0;

        playerRespawnTs = startingPoint;
        runner.respawnPoint = playerRespawnTs;

    }


    /// <summary>
    /// チェックポイント通過処理
    /// </summary>
    /// <param name="targetPoint">通過したチェックポイント</param>
    public void PassCheckPoint(Transform targetPoint)
    {
        // チェックポイント一覧に存在しない場合は警告
        if (!checkPoints.Contains(targetPoint))
        {
            Debug.LogWarning($"This Point doesn't inside the checkPoint's List" +
                $"\n {targetPoint.gameObject.name}" +
                $"\n {targetPoint.position}");

            return;
        }
        // Dictionaryに登録（通過済みに設定）
        checkPointsDict[targetPoint] = true;
        // リスポーン地点更新
        playerRespawnTs = targetPoint;
        runner.respawnPoint = playerRespawnTs;

        passCheckPoint = Mathf.Min(passCheckPoint + 1, checkPoints.Count);

    }

    // エリア判定用の左上・右下ポイント
    private Transform _areaLeftTop;
    private Transform _areaRightDown;

    /// <summary>
    /// マップ初期化
    /// ・スタート地点、ゴール、チェックポイントを設定
    /// ・エリア監視用の範囲を取得
    /// </summary>
    private void MapInit()
    {
        if (useMap != null)
        {
            // スタート地点設定
            startingPoint = useMap.startingTs;
            // ゴール地点設定
            goal = useMap.goalTs;
            // チェックポイント設定
            SetUpCheckPoints(useMap.CheckPointsTs());
        }
        //StageGridManager.Instance.BuildGridMap();

        // スキャンエリアの取得（グリッドマネージャーから）
        _areaLeftTop = StageGridManager.Instance.scanAreaLeftTop;
        _areaRightDown = StageGridManager.Instance.scanAreaRightDown;
    }

    /// <summary>
    /// Runnerがエリア外に出たか監視
    /// ・範囲外ならリスポーン
    /// </summary>
    private void WatchRunnerInAreaOut()
    {
        Transform runnerPos = runner.transform;
        // エリア外判定
        bool isOutOfArea =
            (runnerPos.position.x < _areaLeftTop.position.x) ||   // 左に出た
            (runnerPos.position.x > _areaRightDown.position.x) || // 右に出た
            (runnerPos.position.y < _areaRightDown.position.y - 5); // 下に落ちた（余裕あり）

        // 範囲外ならリスポーン
        if (isOutOfArea) runner.Respawn();

    }


    #endregion

    #region ProgressBar

    [Header("ProgressBar")]

    /// <summary>
    /// 通過したチェックポイント数
    /// </summary>
    public int passCheckPoint = 0;

    /// <summary>
    /// プレイヤーの現在位置（Vector2）
    /// </summary>
    private Vector2 runningPlayerPos => (Vector2)runner.transform.position;


    public const int startToGoalMeter = 400;


    /// <summary>
    /// スタートからゴールまでの距離
    /// </summary>
    private float distanceOfStartToGoal => Vector2.Distance(startingPoint.position, goal.position);
    /// <summary>
    /// プレイヤーからゴールまでの距離
    /// </summary>
    private float distanceOfPlayerPosToGoal => Vector2.Distance(runningPlayerPos, goal.position);
    /// <summary>
    /// 進行率（0～1）
    /// ・0 = スタート地点
    /// ・1 = ゴール地点
    /// </summary>
    public float percentOfPassedDistance
    {
        get
        {
            // 距離が0に近い場合は計算できないため0を返す
            if (distanceOfStartToGoal <= 0.0001f) return 0f;
            // 「残り距離」から進行率を算出し、 0～1の範囲に制限
            else return Mathf.Clamp01(1f - (distanceOfPlayerPosToGoal / distanceOfStartToGoal));
        }
    }

    public int passedDistance => Mathf.FloorToInt(percentOfPassedDistance * startToGoalMeter);

    /// <summary>
    /// チェックポイント状態管理
    /// ・true = 通過済み
    /// ・false = 未通過
    /// </summary>
    public Dictionary<Transform, bool> checkPointsDict = new Dictionary<Transform, bool>();


    private bool IsPointsNull() => startingPoint == null || goal == null || checkPoints == null;




    #endregion

    #region Trap Count

    /// <summary>
    /// Trap 管理クラス内のフィールド・リスト初期化・追加・削除処理
    /// ・Trap の最大数を制限
    /// ・List 内の null（Destroy 済み）を適切に管理
    /// ・安全に追加 / 削除を行う
    /// </summary>
    public const int trapMax = 20;

    /// <summary>
    /// 現在存在している Trap の一覧
    /// 外部からは参照のみ可能（追加・削除は専用メソッド経由）
    /// </summary>
    public List<GameObject> allTheTrap { get; private set; }
    /// <summary>
    /// Trap リストの初期化
    /// ・既存の Trap を全て Destroy
    /// ・List をクリア
    /// </summary>
    private void TrapListInit()
    {
        // List が存在する場合のみ処理
        if (allTheTrap != null && allTheTrap.Count != 0)
        {
            allTheTrap.RemoveAll(trap => trap == null);
            // 既存 Trap をすべて削除
            foreach (GameObject trapGameObject in allTheTrap) if (trapGameObject != null) Destroy(trapGameObject);
        }
        // List を初期化（null の可能性にも対応）
        else if (allTheTrap == null) allTheTrap = new List<GameObject>();
        else allTheTrap.Clear();
    }

    /// <summary>
    /// Trap を追加
    /// ・null（Destroy 済み）を事前に削除
    /// ・最大数を超えた場合は最も古い Trap を削除
    /// </summary>
    /// <param name="trapGameObject">追加する Trap</param>
    public void AddTrap(GameObject trapGameObject)
    {
        // 新しい Trap を追加
        allTheTrap.Add(trapGameObject);
        // 最大数を超えた場合、一番古い Trap を削除
        if (allTheTrap.Count > trapMax) Destroy(allTheTrap[0]);
    }

    /// <summary>
    /// Trap をリストから削除
    /// ・List に存在する場合のみ削除
    /// ・存在しない場合は Warning を出す
    /// ※ OnDestroy() から呼ばれる
    /// </summary>
    /// <param name="trapGameObject">削除対象の Trap</param>
    public void RemoveTrap(GameObject trapGameObject)
    {
        if (allTheTrap.Contains(trapGameObject)) allTheTrap.Remove(trapGameObject);
        else Debug.LogWarning($"{trapGameObject.name} はリストに追加されていません ");
    }



    #endregion

    #region Initialization
    /// <summary>
    /// 全カメラの初期化処理
    /// ・Runner / Hunter 両方のカメラ状態を保存
    /// ・ゲーム開始前に一度だけ呼ぶ想定
    /// </summary>
    private void AllCameraInit()
    {
        // Runnerカメラの初期状態を保存
        runnerCamera.CameraInit();

        // Hunterカメラの初期状態を保存
        hunterCamera.CameraInit();
    }
    public void RunnerRespawn() => runner.Respawn();

    /// <summary>
    /// Runner ?期化??
    /// </summary>
    private void RunnerInit()
    {
        runner.RunnerInit();
        runnerConTrollerPad.Init();
        //runner.ControllerCode = player01.controllerCode;
    }

    private void HunterInit()
    {
        hunterConTrollerPad.HunterInit();
    }



    /// <summary>
    /// カメラの表示先ディスプレイを設定する
    /// ・各プレイヤーの Job に対応するカメラを取得
    /// ・それぞれの displayCode に基づいて出力先ディスプレイを設定する
    /// </summary>
    private void DisPlayInit()
    {
        // Player01 の Job に対応するカメラを取得し、表示先を設定
        TargetCamera(_player01.job).targetDisplay = (int)_player01.displayCode;
        // Player02 の Job に対応するカメラを取得し、表示先を設定
        TargetCamera(_player02.job).targetDisplay = (int)_player02.displayCode;

    }

    [Header("StartingCutScene")]
    public GameObject startingCutSceneOb; // カットシーン用の注視ポイント
    private Coroutine startingCutScene;  // 実行中のコルーチン参照

    /// <summary>
    /// ゲーム開始前のカットシーン処理
    /// ・カメラ演出 → Ready待機 → ゲーム開始
    /// </summary>
    private IEnumerator StartingCutScene()
    {
        // カメラをReady状態（引き・中央寄せ・低Damping）にする
        startingCutSceneOb.SetActive(true);
        runnerCamera.Ready();
        hunterCamera.Ready();
        // Ready状態になるまで待機
        yield return new WaitUntil(() => gameStage == GameStage.Ready);

        // 少し間を取る[演出から]

        yield return new WaitForSeconds(5);

        //[演出まで]

        // 各種初期化
        DisPlayInit();      // 表示切替
        TimerStart();       // タイマー開始
        TrapListInit();     // 罠初期化
        // BGM再生
        AudioManager.Instance.PlayMusic(bgmData);

        // Runnerの初期位置を記録（動いたかチェック用）
        Vector3 runnerPos = runner.transform.position;
        // ゲーム開始
        gameStage = GameStage.Playing;
        // 1フレーム待機（物理更新）
        yield return new WaitForFixedUpdate();

        runnerCamera.ReSetLens();
        hunterCamera.ReSetLens();
        startingCutSceneOb.SetActive(false);

    }

    /// <summary>
    /// ラウンド開始処理
    /// </summary>
    private void TurnInit()
    {
        // ラウンド初期状態へ
        gameStage = GameStage.RoundInit;
        // カットシーン開始（既にあれば停止して再スタート）
        if (startingCutScene != null)
        {
            StopCoroutine(startingCutScene);
            startingCutScene = StartCoroutine(StartingCutScene());
        }
        else startingCutScene = StartCoroutine(StartingCutScene());

        // チェックポイント初期化
        CheckPointsDictInit();
        // プレイヤーをリスポーン
        runner.Respawn();
        // Ready状態へ遷移（カットシーン側が待っている）
        gameStage = GameStage.Ready;
        // 入力割り当て
        GameManager.Instance.Game_PlayerInputAssign();
        // Hunterの操作対象を切替
        hunterConTrollerPad.HunterSwitch((_player01.job == Player.Job.Hunter ? _player01 : _player02));

        //runner.SwitchController();

    }

    #endregion

    #region GameSet

    [Header("GameSet")]
    public GameObject gameSetCutSceneOb;

    // Player の進行率（最大値を記録）
    [SerializeField] private float player01Ran;
    [SerializeField] private float player02Ran;

    // 最大ターン数（この値に到達したらゲーム終了）
    private const int playTurnMax = 2;
    // 現在のターン数
    private int nowPlayTurn = 1;
    // ゲーム終了判定
    private bool gameSet => nowPlayTurn > playTurnMax;

    /// <summary>
    /// ゲーム終了処理
    /// ・全てのコルーチンを停止
    /// ・Hunter 側の処理も停止
    /// </summary>
    private IEnumerator GameSet()
    {
        gameStage = GameStage.GameSet;
        Debug.Log("Game Set");
        // 自身のコルーチンを全停止
        //StopAllCoroutines();
        // Hunter 側のコルーチンも停止
        hunterConTrollerPad.StopAllCoroutines();


        gameSetCutSceneOb.SetActive(true);


        Debug.Log($"Player01 Result : Pass Time : {_player01.playerData.passTime} | Pass Distance : {_player01.playerData.passDistance}");
        Debug.Log($"Player02 Result : Pass Time : {_player02.playerData.passTime} | Pass Distance : {_player02.playerData.passDistance}");

        // 少し間を取る[演出から]

        yield return new WaitForSeconds(5);

        //[演出まで]

        GameManager.Instance.StartCoroutine(GameManager.Instance.ChangeScene(SceneState.Release));
    }

    #endregion


    /// <summary>
    /// ゲームの初期化
    /// 1.マップの初期化
    /// 2.プレイヤーの仕事の設定
    /// 3.仕事の初期化
    /// 4.ターンの初期化
    /// </summary>
    private void InGame_Init()
    {
        gameStage = GameStage.InGameInitStart;
        MapInit();
        AllCameraInit();
        gameSetCutSceneOb.SetActive(false);
        startingCutSceneOb.SetActive(false);


        _player01.SetJob(Player.Job.Runner);
        _player02.SetJob(Player.Job.Hunter);

        RunnerInit();

        //RunnerRespawn();
        HunterInit();

        //AudioManager.Instance.ChangeCrip(bgm, bgmName);
        gameStage = GameStage.InGameInitDone;
        TurnInit();
    }

    /// <summary>
    /// Runner と Hunter の役職を入れ替える
    /// </summary>
    private void TurnSwitch()
    {

        gameStage = GameStage.EndRound;

        AudioManager.Instance.EndMusic(bgmData);

        Player.Job job1 = _player01.job;
        Player.Job job2 = _player02.job;

        // --- 安全檢查 ---
        if (job1 == Player.Job.None || job2 == Player.Job.None)
        {
            Debug.LogError($"Player 01's or Player 02's Job is Null\n" +
                $"Player01 : {_player01.job} , Player02 : {_player02.job}");

        }

        float timeResult = timerStart - timer;

        // --- 距離記錄 ---
        if (!IsPointsNull())
        {
            if (job1 == Player.Job.Runner)
            {
                player01Ran = Mathf.Max(player01Ran, percentOfPassedDistance);
                _player01.playerData.WriteData(timeResult, player01Ran * startToGoalMeter);
                Debug.Log(_player01.playerData.passTime);
            }
            else
            {
                player02Ran = Mathf.Max(player02Ran, percentOfPassedDistance);
                _player02.playerData.WriteData(timeResult, player02Ran * startToGoalMeter);
                Debug.Log(_player02.playerData.passTime);

            }
        }
        else
        {
            Debug.LogWarning("Starting Point, Goal, Check Points someone is null");
        }

        // --- Turn 更新 ---
        nowPlayTurn++;
        Debug.Log($"Turn : {nowPlayTurn}");
        // --- GameSet 判定 ---
        if (gameSet)
        {
            StartCoroutine(GameSet());
            return;
        }


        // --- Job 交換 ---
        _player01.SetJob(job2);
        _player02.SetJob(job1);

        // --- 初始化 ---
        TurnInit();

    }

    public void ThroughGoal()
    {
        if (!IsPointsNull())
        {
            if (_player01.job == Player.Job.Runner) player01Ran = Mathf.Max(player01Ran, 1);
            else player02Ran = Mathf.Max(player02Ran, 1);
        }
        else
        {
            Debug.LogWarning("Starting Point, Goal, Check Points someone is null");
        }

        TurnSwitch();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(this);

    }

    private void Start()
    {
        InGame_Init();
    }

    // デバッグ用
    public bool test;
    private void Update()
    {
        if (test)
        {
            TurnSwitch();
            test = false;
        }


    }

    private void FixedUpdate()
    {
        WatchRunnerInAreaOut();
    }

}
