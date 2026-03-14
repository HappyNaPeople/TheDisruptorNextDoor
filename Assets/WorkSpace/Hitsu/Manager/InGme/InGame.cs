using UnityEngine;
using System.Collections;

/// <summary>
/// ゲーム中の進行管理クラス。
///
/// 主な役割：
/// ・Runner / Hunter の役職管理
/// ・カメラ表示先の制御
/// ・ターン切り替え処理
/// ・ゲームタイマー管理
/// ・Hunter UI の更新
///
/// GameManager から Player 情報を取得し、
/// 各プレイヤーの役割と画面表示を管理する。
/// </summary>
public class InGame : MonoBehaviour
{
    /// <summary>
    /// Singleton インスタンス
    /// </summary>
    public static InGame Instance;

    // プレイヤーごとのカメラ
    public Camera runnerCamera;
    public Camera hunterCamera;

    // Hunter 用 GamePad コントローラー
    public HunterConTrollerPad hunterConTrollerPad;
    // Runner プレイヤー
    public Runner runner;

    // GameManager から取得するプレイヤーインスタンス
    public Player _player01 => GameManager.Instance.player01;
    public Player _player02 => GameManager.Instance.player02;

    /// <summary>
    /// タイマー開始値
    /// </summary>
    private const float timerStart = 150.0f;

    /// <summary>
    /// 現在の残り時間
    /// </summary>
    public float timer;

    /// <summary>
    /// タイムアップ判定
    /// </summary>
    public bool timesUp => timer <= 0;

    // タイマー Coroutine
    private Coroutine timerCountDown;

    /// <summary>
    /// タイマー開始処理
    /// </summary>
    private void TimerStart()
    {
        // 既存タイマー停止
        if (timerCountDown != null) StopCoroutine(timerCountDown);
        // タイマー初期化
        timer = timerStart;
        // カウントダウン開始
        timerCountDown = StartCoroutine(TimerCountDown());
    }

    /// <summary>
    /// タイマーを減少させる Coroutine
    /// </summary>
    private IEnumerator TimerCountDown()
    {
        // 時間が 0 になるまで減少
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// 指定された Job に対応する Camera を取得する
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
    /// Runner 初期化処理
    /// </summary>
    private void RunnerInit()
    {
        //runner.RunnerInit();
        //runner.ControllerCode = player01.controllerCode;
    }


    /// <summary>
    /// Runner と Hunter の役職を入れ替える
    /// </summary>
    private void TurnSwitch()
    {
        // 現在の Job を取得
        Player.Job job1 = _player01.job;
        Player.Job job2 = _player02.job;

        // Job を交換
        _player01.SetJop(job2);
        _player02.SetJop(job1);

        // Job に対応するカメラ取得
        Camera cam1 = TargetCamera(job2);
        Camera cam2 = TargetCamera(job1);

        // カメラの表示先ディスプレイ変更
        cam1.targetDisplay = (int)_player01.displayCode;
        cam2.targetDisplay = (int)_player02.displayCode;

        // Hunter UI 更新
        hunterConTrollerPad.HunterSwitch((_player01.job == Player.Job.Hunter ? _player01 : _player02));

        //runner.SwitchController();

        // カメラの表示先ディスプレイを設定する
        DisPlayInit();
        // タイマー再スタート
        TimerStart();
    }

    /// <summary>
    /// カメラの表示先ディスプレイを設定する
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
        //Testing
        _player01.SetJop(Player.Job.Runner);
        _player02.SetJop(Player.Job.Hunter);
        hunterConTrollerPad.HunterSwitch(_player02);
        RunnerInit();
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

}
