using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

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

    // プ?イ?ーごとのカ??
    public Camera runnerCamera;
    public Camera hunterCamera;

    // Hunter 用 GamePad コ?ト?ー?ー
    public HunterConTrollerPad hunterConTrollerPad;
    // Runner プ?イ?ー
    public Runner runner;

    // GameManager から取得するプ?イ?ーイ?スタ?ス
    public Player _player01 => GameManager.Instance.player01;
    public Player _player02 => GameManager.Instance.player02;


    #region Timer
    /// <summary>
    /// タイマー開始値
    /// </summary>
    public const float timerStart = 5.0f;

    /// <summary>
    /// 現在の残り?間
    /// </summary>
    public float timer { get; private set; }

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

        test = true;
    }

    #endregion

    #region ProgressBar

    /// <summary>
    /// 通過したチェックポイント数
    /// </summary>
    public int passCheckPoint = 0;

    /// <summary>
    /// プレイヤーの現在位置（Vector2）
    /// </summary>
    private Vector2 runningPlayerPos => (Vector2)runner.transform.position;

    /// <summary>
    /// スタート地点
    /// </summary>
    public Transform startingPoint;

    public void SetUpStartingPoint(Transform target) => startingPoint = target;

    /// <summary>
    /// ゴール地点
    /// </summary>
    public Transform goal;
    public const int startToGoalMeter = 300;
    public void SetUpGoal(Transform target) => goal = target;


    /// <summary>
    /// スタートからゴールまでの距離
    /// </summary>
    private float distanceOfStartToGoal =>Vector2.Distance(startingPoint.position, goal.position);
    /// <summary>
    /// プレイヤーからゴールまでの距離
    /// </summary>
    private float distanceOfPlayerPosToGoal =>Vector2.Distance(runningPlayerPos, goal.position);
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
            if (distanceOfStartToGoal <= 0.0001f)return 0f;
            // 「残り距離」から進行率を算出し、 0～1の範囲に制限
            else return Mathf.Clamp01(1f - (distanceOfPlayerPosToGoal / distanceOfStartToGoal));
        }
    }
    /// <summary>
    /// チェックポイント一覧
    /// </summary>
    public List<Transform> checkPoints;

    public void SetUpCheckPoints(List<Transform> target)
    {
        if (target == null)
        {
            Debug.LogWarning("Cant setup the CheckPoints");
            return;
        }
        checkPoints.Clear();
        checkPoints.AddRange(target);
        target.Clear();

        CheckPointsDictInit();
    }

    /// <summary>
    /// チェックポイント状態管理
    /// ・true = 通過済み
    /// ・false = 未通過
    /// </summary>
    public Dictionary<Transform, bool> checkPointsDict = new Dictionary<Transform, bool>();
    /// <summary>
    /// リスポーン地点
    /// </summary>
    public Transform playerRespawnPos { get; private set; }

    private bool IsPointsNull() => startingPoint == null || goal == null || checkPoints == null;

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
        playerRespawnPos = targetPoint;

        passCheckPoint = Mathf.Min(passCheckPoint + 1, checkPoints.Count);

    }
    /// <summary>
    /// チェックポイント初期化
    /// ・全て未通過にリセット
    /// </summary>
    private void CheckPointsDictInit()
    {
        // 既存データをクリア
        checkPointsDict.Clear();
        // 全チェックポイントを未通過（false）で登録

        foreach (Transform transform in checkPoints)
        {
            checkPointsDict[transform] = false;
        }

        // 通過数リセット
        passCheckPoint = 0;

        playerRespawnPos = startingPoint;
    }



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
        if (allTheTrap != null && allTheTrap.Count == 0)
        {
            // 既存 Trap をすべて削除
            foreach (GameObject trapGameObject in allTheTrap) if (trapGameObject != null) Destroy(trapGameObject);
        }
        // List を初期化（null の可能性にも対応）
        if (allTheTrap == null)allTheTrap = new List<GameObject>();
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

    #region GameSet

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
        Debug.Log("Game Set");
        // 自身のコルーチンを全停止
        //StopAllCoroutines();
        // Hunter 側のコルーチンも停止
        hunterConTrollerPad.StopAllCoroutines();

        // 結果計算（）
        float result01 = player01Ran * startToGoalMeter;
        float result02 = player02Ran * startToGoalMeter;

        Debug.Log($"P1: {result01} | P2: {result02}");
        GameManager.Instance.PlayerResult(result01, result02);


        //
        // ここは終了演出
        //
        yield return null;

    }

    #endregion


    /// <summary>
    /// 指定された Job に対?する Camera を取得する
    /// </summary>
    private Camera TargetCamera(Player.Job targetJob)
    {
        switch (targetJob)
        {
            case Player.Job.Runner: return runnerCamera;
            case Player.Job.Hunter: return hunterCamera;
        }
        return null;
    }

    #region Initialization

    /// <summary>
    /// Runner ?期化??
    /// </summary>
    private void RunnerInit()
    {
        runner.RunnerInit();
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
    /// <summary>
    /// ターン開始時の初期化処理
    /// ・現在の Hunter を判定し、UI/操作対象を切り替える
    /// ・各プレイヤーのカメラ表示先（ディスプレイ）を更新する
    /// ・チェックポイント情報を初期化する
    /// ・タイマーをリスタートする
    /// ・Trap リストを初期化する
    /// </summary>
    private void TurnInit()
    {

        hunterConTrollerPad.HunterSwitch((_player01.job == Player.Job.Hunter ? _player01 : _player02));

        //runner.SwitchController();

        DisPlayInit();

        CheckPointsDictInit();
        // タイマー再スタート
        TimerStart();

        TrapListInit();
    }

    #endregion

    private void InGame_Init()
    {
        Player.Job job1 = _player01.job;
        Player.Job job2 = _player02.job;

        if (job1 == Player.Job.None && job2 == Player.Job.None)
        {
            _player01.SetJob(Player.Job.Runner);
            _player02.SetJob(Player.Job.Hunter);
        }
        TurnInit();
    }

    /// <summary>
    /// Runner と Hunter の役職を入れ替える
    /// </summary>
    private void TurnSwitch()
    {
        // --- Turn 更新 ---
        nowPlayTurn++;
        Debug.Log($"Turn : {nowPlayTurn}");
        // --- GameSet 判定 ---
        if (gameSet)
        {
            StartCoroutine(GameSet());
            return;
        }
        Player.Job job1 = _player01.job;
        Player.Job job2 = _player02.job;
        // --- 安全檢查 ---
        if (job1 == Player.Job.None || job2 == Player.Job.None)
        {
            Debug.LogError($"Player 01's or Player 02's Job is Null\n" +
                $"Player01 : {_player01.job} , Player02 : {_player02.job}");

        }
        // --- 距離記錄 ---
        if (!IsPointsNull())
        {
            if (job1 == Player.Job.Runner) player01Ran = Mathf.Max(player01Ran, percentOfPassedDistance);
            else player02Ran = Mathf.Max(player02Ran, percentOfPassedDistance);
        }
        else
        {
            Debug.LogWarning("Starting Point, Goal, Check Points someone is null");
        }
        // --- Job 交換 ---
        _player01.SetJob(job2);
        _player02.SetJob(job1);

        GameManager.Instance.Game_PlayerInputAssign();
        // --- 初始化 ---
        TurnInit();

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
        RunnerInit();
        HunterInit();
    }

    // デバッグ用
    public bool test;
    private void Update()
    {
        if (test)
        {
            TurnSwitch();
            //PassCheckPoint(checkPoints[0]);
            test = false;
        }


    }

}
