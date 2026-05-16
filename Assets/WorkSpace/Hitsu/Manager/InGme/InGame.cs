using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine.InputSystem;


[System.Serializable]
/// <summary>
/// Cinemachineカメラの状態管理を行うクラス
/// ズーム値・FollowOffset・Dampingなどの初期状態を保持し、
/// ゲーム状況に応じてカメラ演出を切り替える
/// </summary>
public class CameraData
{
    /// <summary> 通常カメラ </summary>
    public Camera camera;
    /// <summary> Cinemachineカメラ本体 </summary>
    public CinemachineCamera cinemachineCamera;
    /// <summary> Follow制御用コンポーネント </summary>
    private CinemachineFollow cinemaChineFollow;
    /// <summary> 初期のOrthographicSize </summary>
    private float lens;
    /// <summary> 初期のFollowOffset </summary>
    private Vector3 basicFollowOffset;
    /// <summary> 初期の追従Damping値 </summary>
    private Vector3 basicPositionDamping;
    /// <summary> Ready演出用のFollowOffset </summary>
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
        // 現在のDampingを保存
        basicPositionDamping = cinemaChineFollow.TrackerSettings.PositionDamping;
    }

    /// <summary>
    /// ゲーム開始前の演出用カメラ状態
    /// </summary>
    public void Ready()
    {
        cinemachineCamera.Lens.OrthographicSize = 9;            // 演出用ズーム値
        cinemaChineFollow.FollowOffset = readyV3;               // 演出用FollowOffset
    }

    /// <summary>
    /// Dampingのみ初期状態に戻す
    /// </summary>
    /// </summary>
    public void ReSetLens()
    {
        cinemachineCamera.Lens.OrthographicSize = lens;         // 初期ズーム値へ戻す
        cinemaChineFollow.FollowOffset = basicFollowOffset;     // 初期FollowOffsetへ戻す

    }
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

public class InGame : MonoBehaviour
{
    /// <summary> InGame のシングルトンインスタンス </summary>
    public static InGame Instance;

    [Header("Audio")]
    /// <summary> ゲーム内BGMデータ </summary>
    public BgmData bgmData;

    [Header("Camera")]
    /// <summary> Runner側カメラ管理 </summary>
    public CameraData runnerCamera;
    /// <summary> Hunter側カメラ管理 </summary>
    public CameraData hunterCamera;
    /// <summary>
    /// プレイヤーの役職に対応したカメラを取得する
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

    [Header("Player ")]
    /// <summary> Runner本体 </summary>
    public Runner runner;
    /// <summary> Player01情報取得 </summary>
    public Player _player01
    {
        get
        {
            if (GameManager.Instance != null) return GameManager.Instance.player01;
            Debug.LogError("GameManager.Instance == null");
            return null;
        }
    }
    /// <summary> Player02情報取得 </summary>
    public Player _player02
    {
        get
        {
            if (GameManager.Instance != null) return GameManager.Instance.player02;
            Debug.LogError("GameManager.Instance == null");
            return null;
        }
    }

    [Header("UI ConTroller Pad")]
    /// <summary> Hunter側UI管理 </summary>
    public HunterConTrollerPad hunterConTrollerPad;
    /// <summary> Runner側UI管理 </summary>
    public RunnerConTrollerPad runnerConTrollerPad;

    /// <summary> 現在のゲーム進行状態 </summary>
    public GameStage gameStage;

    #region Timer
    [Header("Time")]
    /// <summary> 現在の残り時間 </summary>
    public float timer;
    /// <summary> タイマー初期時間 </summary>
    public const float timerStart = 150.0f;
    /// <summary> 現在時間を整数値で取得する </summary>
    public int NowTimeToInt() => (int)timer;
    /// <summary> タイマー用Coroutine </summary>
    private Coroutine timerCountDown;

    /// <summary>
    /// タイマー処理を開始する
    /// </summary>
    private void TimerStart()
    {
        if (timerCountDown != null) StopCoroutine(timerCountDown);  // 既存タイマー停止
        timerCountDown = null;

        timer = timerStart;                                         // 初期時間設定
        timerCountDown = StartCoroutine(TimerCountDown());          // カウントダウン開始
    }
    /// <summary>
    /// 制限時間をカウントダウンするCoroutine
    /// </summary>
    private IEnumerator TimerCountDown()
    {
        while (timer > 0)
        {
            // 時間減る
            timer -= Time.deltaTime;
            yield return null;
        }
        // 時間切れ時にターン切り替え
        TurnSwitch();
    }
    #endregion

    #region Map
    [Header("Map")]
    /// <summary> 現在使用中のマップデータ </summary>
    public MapBasic useMap;
    /// <summary> スタート地点 </summary>
    public Transform startingPoint;
    /// <summary> ゴール地点 </summary>
    public Transform goal;
    /// <summary> チェックポイント一覧 </summary>
    public List<Transform> checkPoints;
    /// <summary> プレイヤーのリスポーン地点 </summary>
    public Transform playerRespawnTs;
    /// <summary> エリア左上座標 </summary>
    private Transform _areaLeftTop;
    /// <summary> エリア右下座標 </summary>
    private Transform _areaRightDown;
    /// <summary> スタートからゴールまでの距離 </summary>
    private float distanceOfStartToGoal;
    /// <summary>
    /// マップに必要なポイントが存在するか確認する
    /// </summary>
    private bool IsPointsNull(out bool haveStart, out bool haveThreeCheckPoints, out bool haveEnd)
    {
        // useMap未設定
        if (useMap == null)
        {
            haveStart = false;
            haveThreeCheckPoints = false;
            haveEnd = false;
            return false;
        }

        // マップ内ポイント確認
        return useMap.CheckAllThePoints(out haveStart, out haveThreeCheckPoints, out haveEnd);
    }

    /// <summary>
    /// マップ情報を初期化する
    /// </summary>
    private void MapInit()
    {
        // useMap未設定チェック
        if (useMap == null)
        {
            Debug.LogError("useMap == null");
            return;
        }
        // GridManager未設定チェック
        if (StageGridManager.Instance == null)
        {
            Debug.LogError("StageGridManager.Instance == null");
            return;
        }
        // 必須ポイント存在確認
        bool checkTheMap = IsPointsNull(out bool haveStart, out bool haveThreeCheckPoints, out bool haveEnd);
        // 必須ポイント不足
        if (!checkTheMap)
        {
            Debug.LogError(
            $"Points Error: Start : {haveStart} ," +
            $" CheckPoints : {haveThreeCheckPoints} ," +
            $" Goal : {haveEnd}");
            return;
        }
        // グリッド生成
        StageGridManager.Instance.BuildGridMap();
        // エリア範囲取得
        _areaLeftTop = StageGridManager.Instance.scanAreaLeftTop;
        _areaRightDown = StageGridManager.Instance.scanAreaRightDown;

        startingPoint = useMap.startingTs;              // スタート地点取得
        goal = useMap.goalTs;                           // ゴール地点取得

        // チェックポイント更新
        checkPoints.Clear();
        checkPoints.AddRange(useMap.CheckPointsTs());
        // スタートからゴールまでの距離計算
        distanceOfStartToGoal = Vector2.Distance(startingPoint.position, goal.position);
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

    #region CheckPoint
    /// <summary> チェックポイント通過状態管理 </summary>
    public Dictionary<Transform, bool> checkPointsDict = new Dictionary<Transform, bool>();
    /// <summary>
    /// チェックポイント状態を初期化する
    /// </summary>
    private void CheckPointsDictInit()
    {
        // 既存データをクリア
        checkPointsDict.Clear();
        // チェックポイント状態リセット
        useMap.ResetCheckPoints();
        // 全チェックポイントを未通過状態で登録
        foreach (Transform transform in checkPoints)
        {
            checkPointsDict[transform] = false;
        }

        // 通過数リセット
        passCheckPoint = 0;

        // 初期リスポーン地点設定
        playerRespawnTs = startingPoint;
        // Runner側リスポーン地点更新
        runner.respawnPoint = playerRespawnTs;

    }
    /// <summary>
    /// チェックポイント通過時の処理
    /// </summary>
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
        
        checkPointsDict[targetPoint] = true;        // Dictionaryに登録（通過済みに設定）
        playerRespawnTs = targetPoint;              // リスポーン地点更新
        runner.respawnPoint = playerRespawnTs;      // Runner側リスポーン地点更新
        // 通過数更新
        passCheckPoint = Mathf.Min(passCheckPoint + 1, checkPoints.Count);

    }
    #endregion

    #region WalkDistance
    [Header("Walk Distance")]
    /// <summary> 通過済みチェックポイント数 </summary>
    public int passCheckPoint = 0;
    /// <summary> Runnerの現在位置 </summary>
    private Vector2 runningPlayerPos => (Vector2)runner.transform.position;
    /// <summary> スタートからゴールまでの基準距離(m表記用) </summary>
    public const int startToGoalMeter = 400;
    /// <summary> Runnerからゴールまでの残り距離 </summary>
    private float distanceOfPlayerPosToGoal
    {
        get
        {
            // ゴール未設定チェック
            if (goal == null)
            {
                Debug.LogError("goal == null");
                return 0.0f;
            }
            // 現在位置からゴールまでの距離取得
            return Vector2.Distance(runningPlayerPos, goal.position);
        }
    }
    /// <summary>
    /// スタートからゴールまでの進行率
    /// 0 ～ 1 の範囲で返す
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
    /// <summary> 現在進んだ距離(m換算) </summary>
    public int passedDistance => Mathf.FloorToInt(percentOfPassedDistance * startToGoalMeter);

    #endregion

    #region Trap Count

    /// <summary> Trapの最大設置数 </summary>
    public const int trapMax = 20;

    /// <summary>
    /// 現在存在しているTrap一覧
    /// 外部からは参照のみ可能で、追加・削除は専用メソッド経由で行う
    /// </summary>
    public List<GameObject> allTheTrap { get; private set; } = new List<GameObject>();

    /// <summary>
    /// Trapリストを初期化する
    /// 既存Trapを削除し、リストを再利用可能な状態に戻す
    /// </summary>
    private void TrapListInit()
    {
        // List が存在する場合のみ処理
        if (allTheTrap != null && allTheTrap.Count != 0)
        {
            allTheTrap.RemoveAll(trap => trap == null);
            // 既存 Trap をすべて削除
            foreach (GameObject trapGameObject in allTheTrap) if (trapGameObject != null) Destroy(trapGameObject);
            allTheTrap.Clear();
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
        if (allTheTrap == null) allTheTrap = new List<GameObject>();
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
        if (allTheTrap == null) return;
        if (allTheTrap.Contains(trapGameObject)) allTheTrap.Remove(trapGameObject);
        // else Debug.LogWarning($"{trapGameObject.name} はリストに追加されていません ");
    }
    #endregion

    #region TurnInit
    [Header("StartingCutScene")]
    /// <summary> 開始演出用オブジェクト </summary>
    public GameObject startingCutSceneOb;
    /// <summary> 開始演出Coroutine </summary>
    private Coroutine startingCutScene;
    /// <summary>
    /// 各プレイヤーの役職に応じてカメラの表示先を設定する
    /// </summary>
    private void DisPlayInit()
    {
        // Player01 の Job に対応するカメラを取得し、表示先を設定
        TargetCamera(_player01.job).targetDisplay = (int)_player01.displayCode;
        // Player02 の Job に対応するカメラを取得し、表示先を設定
        TargetCamera(_player02.job).targetDisplay = (int)_player02.displayCode;

    }
    /// <summary>
    /// プレイヤーの入力割り当てと各操作UIを初期化する
    /// </summary>
    private void PlayerSwitch()
    {
        // プレイヤー入力を現在の役職に合わせて割り当て
        GameManager.Instance.Game_PlayerInputAssign();
        // Hunter側の操作・UIを切り替え
        hunterConTrollerPad.HunterSwitch((_player01.job == Player.Job.Hunter ? _player01 : _player02));
        // Hunterカーソル初期化
        hunterConTrollerPad.HunterInit();
        // Runner側操作UI初期化
        runnerConTrollerPad.Init();
    }

    /// <summary>
    /// ゲームプレイを開始する
    /// </summary>
    private void StartGame()
    {
        // StageGridManager未設定チェック
        if (StageGridManager.Instance == null)
        {
            Debug.LogError("StageGridManager.Instance == null");
            return;
        }
        
        StageGridManager.Instance.RespawnMapObstacles();    // ステージデザイン時に配置された木箱を復活させる
        AudioManager.Instance.PlayMusic(bgmData);           // BGM再生
        TimerStart();                                       // タイマー開始
        gameStage = GameStage.Playing;                      // ゲーム状態をPlayingへ変更

    }

    /// <summary>
    /// ラウンド開始前の演出処理
    /// </summary>
    private IEnumerator StartingCutScene()
    {
        // 開始演出表示
        startingCutSceneOb.SetActive(true);
        // カメラをReady演出状態へ変更
        runnerCamera.Ready();
        hunterCamera.Ready();

        // 演出時間初期化
        float cutSceneTimer = 0;

        while (cutSceneTimer < 5.0f)
        {
            // 演出時間加算
            cutSceneTimer += Time.deltaTime;
            // Runnerをスタート地点に固定
            runner.transform.position = startingPoint.position;
            yield return null;
        }

        // カメラを通常状態へ戻す
        runnerCamera.ReSetLens();
        hunterCamera.ReSetLens();

        // 開始演出非表示
        startingCutSceneOb.SetActive(false);

        // ゲーム開始
        StartGame();
    }

    /// <summary>
    /// ターン開始時の初期化処理
    /// </summary>
    private void TurnInit()
    {
        // ラウンド初期化状態へ変更
        gameStage = GameStage.RoundInit;

        CheckPointsDictInit();  // チェックポイント状態初期化

        runner.Respawn();       // Runnerをリスポーン

        PlayerSwitch();         // プレイヤー操作切り替え
        DisPlayInit();          // ディスプレイ設定
        TrapListInit();         // Trapリスト初期化

        // 既存の開始演出Coroutineを停止
        if (startingCutScene != null) StopCoroutine(startingCutScene);
        // 開始演出Coroutine開始
        startingCutScene = StartCoroutine(StartingCutScene());
    }

    #endregion

    #region GameSet
    [Header("GameSet")]
    /// <summary> ゲーム終了演出用オブジェクト </summary>
    public GameObject gameSetCutSceneOb;

    /// <summary>
    /// ゲーム終了時の演出処理
    /// </summary>
    private IEnumerator GameSet_CutScene()
    {
        // カメラを演出用状態へ変更
        runnerCamera.Ready();
        hunterCamera.Ready();

        // 終了演出表示
        gameSetCutSceneOb.SetActive(true);

        float timer = 0;
        // 一定時間待機
        while (timer < 2.0f)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        // リザルトシーンへ遷移
        GameManager.Instance.StartCoroutine(GameManager.Instance.ChangeScene(SceneState.Release));

    }
    /// <summary>
    /// ゲーム終了処理
    /// </summary>
    private void GameSet()
    {
        
        gameStage = GameStage.GameSet;              // ゲーム状態を終了へ変更
        Debug.Log("Game Set");
        hunterConTrollerPad.StopAllCoroutines();    // Hunter側の処理を停止
        GameManager.Instance.WriteInGameSet();      // ゲーム結果を書き込み
        StartCoroutine(GameSet_CutScene());         // 終了演出開始
    }

    #endregion

    #region TurnSwitch

    /// <summary> 最大プレイターン数 </summary>
    private const int playTurnMax = 2;
    /// <summary> 現在のプレイターン数 </summary>
    private int nowPlayTurn = 1;
    /// <summary> ゲーム終了条件 </summary>
    private bool gameSet => nowPlayTurn > playTurnMax;
    /// <summary>
    /// 現在Runnerを担当しているプレイヤーの結果を記録する
    /// </summary>
    /// <param name="isThroughGoal">ゴール到達済みかどうか</param>
    private void RecordPlayerData(bool isThroughGoal)
    {
        // 現在Runnerのプレイヤーを取得
        Player targetPlayer = _player01.job == Player.Job.Runner ? _player01 : _player02;
        // マップ情報確認
        bool checkTheMap = IsPointsNull(out bool haveStart, out bool haveThreeCheckPoints, out bool haveEnd);
        // 必須ポイント不足
        if (!checkTheMap)
        {
            Debug.LogError(
            $"Points Error: Start : {haveStart} ," +
            $" CheckPoints : {haveThreeCheckPoints} ," +
            $" Goal : {haveEnd}");
            return;
        }

        // 経過時間計算
        float timeResult = timerStart - timer;
        // 進行距離計算
        float ranResult = percentOfPassedDistance * startToGoalMeter;
        // ゴール到達時は最大距離にする
        ranResult = isThroughGoal ? startToGoalMeter : ranResult;
        // 既存記録より短くならないようにする
        ranResult = Mathf.Max(ranResult, targetPlayer.playerData.passDistance);
        // プレイヤーデータへ記録
        targetPlayer.playerData.WriteData(timeResult, ranResult);
    }

    /// <summary>
    /// Player01とPlayer02の役職を入れ替える
    /// </summary>
    private void JobSwitch()
    {
        // 現在の役職を保存
        Player.Job job1 = _player01.job;
        Player.Job job2 = _player02.job;

        // 役職未設定チェック
        if (job1 == Player.Job.None || job2 == Player.Job.None)
        {
            Debug.LogError($"Player 01's or Player 02's Job is Null\n" +
                $"Player01 : {_player01.job} , Player02 : {_player02.job}");
        }

        // 役職入れ替え
        _player01.SetJob(job2);
        _player02.SetJob(job1);

    }

    /// <summary>
    /// ターンを切り替える
    /// </summary>
    private void TurnSwitch()
    {

        gameStage = GameStage.EndRound;                 // ラウンド終了状態へ変更
        AudioManager.Instance.EndMusic(bgmData);        // BGM停止
        RecordPlayerData(false);                        // 現在Runnerの結果を記録

        nowPlayTurn++;                                  // ターン数加算

        if (gameSet)                                    // 最大ターンを超えた場合はゲーム終了
        {
            GameSet();
            return;
        }

        JobSwitch();                                    // 役職入れ替え
        TurnInit();                                     // 次ターン初期化

    }

    /// <summary>
    /// Runnerがゴールに到達した時の処理
    /// </summary>
    public void ThroughGoal()
    {
        RecordPlayerData(true);         // ゴール到達として結果記録
        TurnSwitch();                   // ターン切り替え
    }

    #endregion

    #region Initialization
    /// <summary>
    /// 全カメラの初期化処理
    /// ・Runner / Hunter 両方のカメラ状態を保存
    /// ・ゲーム開始前に一度だけ呼ぶ
    /// </summary>
    private void AllCameraInit()
    {
        runnerCamera.CameraInit();      // Runnerカメラの初期状態を保存
        hunterCamera.CameraInit();      // Hunterカメラの初期状態を保存
    }

    /// <summary>
    /// プレイヤー関連の初期化処理
    /// プレイヤーデータ・役職・Runner / Hunter操作を初期状態へ設定する
    /// </summary>
    public void PlayerInit()
    {

        _player01.playerData.ResetData();       // Player01の結果データをリセット
        _player02.playerData.ResetData();       // Player02の結果データをリセット

        _player01.SetJob(Player.Job.Runner);    // 初期役職設定
        _player02.SetJob(Player.Job.Hunter);

        runner.RunnerInit();                    // Runner本体初期化
        runnerConTrollerPad.Init();             // Runner側操作UI初期化
        hunterConTrollerPad.HunterInit();       // Hunter側操作UI初期化


    }

    /// <summary>
    /// InGame全体の初期化処理
    /// マップ・カメラ・プレイヤーを初期化し、
    /// 初期化完了後に最初のターンを開始する
    /// </summary>
    private void InGame_Init()
    {
        gameStage = GameStage.InGameInitStart;  // 初期化開始状態へ変更
        MapInit();                              // マップ初期化
        AllCameraInit();                        // カメラ初期化
        PlayerInit();                           // プレイヤー初期化
        gameStage = GameStage.InGameInitDone;   // 初期化完了状態へ変更
        TurnInit();                             // 最初のターン開始
    }

    #endregion


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
    private void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame) TurnSwitch();

    }
    private void FixedUpdate()
    {
        WatchRunnerInAreaOut();
    }

}

