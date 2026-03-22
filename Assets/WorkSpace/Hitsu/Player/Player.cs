using System.Drawing;
using UnityEngine;

/// <summary>
/// プレイヤーを管理するクラス。
/// 
/// 主な役割：
/// ・プレイヤーの役職（Runner / Hunter）の管理
/// ・表示するディスプレイ番号の管理
/// ・入力コントローラー番号の管理
/// ・役職ごとの処理を更新する
/// </summary>
public class Player : MonoBehaviour
{
    // Hunter 用クラス
    public Hunter hunter = new Hunter();
    // 表示するディスプレイ番号
    public DisPlayNumber displayCode = DisPlayNumber.None;

    public ControllerNumber controllerCode = ControllerNumber.None;
    public PlayerInputData inputData;

    /// <summary>
    /// プレイヤーの役職
    /// </summary>
    public enum Job
    {
        None,
        Runner,
        Hunter
    }
    /// <summary>
    /// プレイヤーの役職を変更する
    /// </summary>
    public Job job /*{ get; private set; }*/;
    /// <summary>
    /// プレイヤーの役職を変更する
    /// </summary>
    public void SetJob(Job targetJop) => job = targetJop;
    /// <summary>
    /// プレイヤーの初期化
    /// </summary>
    public void PlayerInit(DisPlayNumber targetDisplay, ControllerNumber targetController)
    {
        displayCode = targetDisplay;
        controllerCode = targetController;
    }
    /// <summary>
    /// 役職ごとの処理を更新する
    /// </summary>
    private void JopUpdate()
    {
        switch (job)
        {
            case Job.Runner:
                // Runner 用の処理
                break;

            case Job.Hunter:
                // Hunter 用の処理
                break;
        }
    }

    private void Update()
    {
        JopUpdate();
    }
}
