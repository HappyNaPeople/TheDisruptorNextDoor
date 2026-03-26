using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public const float timerStart = 150.0f;

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
    /// <summary>
    /// ゴール地点
    /// </summary>
    public Transform goal;

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
    /// <summary>
    /// チェックポイント状態管理
    /// ・true = 通過済み
    /// ・false = 未通過
    /// </summary>
    public Dictionary<Transform, bool> checkPointsDict = new Dictionary<Transform, bool>();
    /// <summary>
    /// リスポーン地点
    /// </summary>
    private Transform rebirthPoint;
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
        rebirthPoint = targetPoint;

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


    /// <summary>
    /// Runner ?期化??
    /// </summary>
    private void RunnerInit()
    {
        runner.RunnerInit();
        //runner.ControllerCode = player01.controllerCode;
    }

    private void InGame_Init()
    {
        Player.Job job1 = _player01.job;
        Player.Job job2 = _player02.job;

        if (job1 == Player.Job.None && job2 == Player.Job.None)
        {
            _player01.SetJob(Player.Job.Runner);
            _player02.SetJob(Player.Job.Hunter);
        }

        Camera cam1 = TargetCamera(_player01.job);
        Camera cam2 = TargetCamera(_player02.job);

        cam1.targetDisplay = (int)_player01.displayCode;
        cam2.targetDisplay = (int)_player02.displayCode;

        // Hunter UI 更新
        hunterConTrollerPad.HunterSwitch((_player01.job == Player.Job.Hunter ? _player01 : _player02));

        //runner.SwitchController();

        // カ??の表示先ディスプ?イを設定する
        DisPlayInit();

        CheckPointsDictInit();
        // タイマー再スタート
        TimerStart();
    }

    /// <summary>
    /// Runner と Hunter の役職を入れ替える
    /// </summary>
    private void TurnSwitch()
    {
        // 現在の Job を取得
        Player.Job job1 = _player01.job;
        Player.Job job2 = _player02.job;

        if (job1 == Player.Job.None && job2 == Player.Job.None)
        {
            _player01.SetJob(Player.Job.Runner);
            _player02.SetJob(Player.Job.Hunter);
        }
        else
        {
            // Job を交換
            _player01.SetJob(job2);
            _player02.SetJob(job1);
        }


        // Job に対?するカ??取得
        Camera cam1 = TargetCamera(job2);
        Camera cam2 = TargetCamera(job1);

        Debug.Log(_player01.displayCode);
        // カ??の表示先ディスプ?イ変更
        cam1.targetDisplay = (int)_player01.displayCode;
        cam2.targetDisplay = (int)_player02.displayCode;

        // Hunter UI 更新
        hunterConTrollerPad.HunterSwitch((_player01.job == Player.Job.Hunter ? _player01 : _player02));

        //runner.SwitchController();

        // カ??の表示先ディスプ?イを設定する
        DisPlayInit();
        // タイマー再スタート
        TimerStart();
    }

    /// <summary>
    /// カ??の表示先ディスプ?イを設定する
    /// </summary>
    private void DisPlayInit()
    {
        TargetCamera(_player01.job).targetDisplay = (int)_player01.displayCode;
        TargetCamera(_player02.job).targetDisplay = (int)_player02.displayCode;
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
    }

    // デバッグ用
    public bool test;
    private void Update()
    {
        if (test)
        {
            TurnSwitch();
            PassCheckPoint(checkPoints[0]);
            test = false;
        }


    }

}
