using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class InGame : MonoBehaviour
{
    public static InGame Instance;
    // プレイヤーごとのカメラ
    public Camera runnerCamera;
    public Camera hunterCamera;
    // Hunter 用 GamePad コントローラー
    public HunterConTrollerPad hunterConTrollerPad;
    // プレイヤーインスタンス
    public Player _player01 => GameManager.Instance.player01;
    public Player _player02 => GameManager.Instance.player02;

    public Runner runner;

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
    /// GameManager 全体の初期化処理
    /// </summary>
    private void RunnerInit()
    {
        //runner.RunnerInit();
        //runner.ControllerCode = player01.controllerCode;
    }


    /// <summary>
    /// Runner と Hunter の役職を入れ替える
    /// </summary>
    private void JobSwitch()
    {
        Debug.Log(_player01);
        Debug.Log(_player02);

        Player.Job job1 = _player01.job;
        Player.Job job2 = _player02.job;

        _player01.SetJop(job2);
        _player02.SetJop(job1);

        Camera cam1 = TargetCamera(job2);
        Camera cam2 = TargetCamera(job1);

        Debug.Log(cam1);
        Debug.Log(cam2);

        cam1.targetDisplay = (int)_player01.displayCode;
        cam2.targetDisplay = (int)_player02.displayCode;

        hunterConTrollerPad.HunterSwitch((_player01.job == Player.Job.Hunter ? _player01 : _player02));
        //runner.SwitchController();
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
        _player01.SetJop(Player.Job.Runner);
        _player02.SetJop(Player.Job.Hunter);
        hunterConTrollerPad.HunterSwitch(_player02);

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
