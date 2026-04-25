using UnityEngine;

/// <summary>
/// ランナーの足跡や着地エフェクトを管理する専用コンポーネント。
/// インビジブル状態などのモディファイアからの介入も受け付ける。
/// Runnerプレハブ（またはその子オブジェクト）にアタッチして使用する。
/// </summary>
public class RunnerEffectController : MonoBehaviour
{
    [Header("エフェクト設定")]
    [Tooltip("歩行時の足跡エフェクトのプレハブ")]
    public GameObject footstepPrefab;
    [Tooltip("ジャンプ着地時の砂埃エフェクトのプレハブ")]
    public GameObject landEffectPrefab;
    [Tooltip("エフェクトを生成する位置のオフセット(足元などを指定)")]
    public Vector3 effectOffset = Vector3.zero;

    [Header("デバッグ・設定")]
    [Tooltip("チェックを入れるとトラップに関係なくいつでも足跡が出ます")]
    public bool isAlwaysFootstepOn = true;

    // トラップ（インビジブル等）からオンオフされるフラグ
    [HideInInspector] 
    public bool forceShowFootstep = false;

    private Runner runner;
    private bool prevGrounded = true;

    private void Awake()
    {
        // 自身のアタッチ先、もしくは親階層からRunnerを探す
        runner = GetComponentInParent<Runner>();
    }

    private void Update()
    {
        if (runner == null) return;

        bool currentGrounded = runner.IsGrounded;

        // 空中から接地した瞬間の判定
        if (!prevGrounded && currentGrounded)
        {
            PlayLandEffect();
        }

        prevGrounded = currentGrounded;
    }

    /// <summary>
    /// 着地時のエフェクトを再生する
    /// </summary>
    public void PlayLandEffect()
    {
        if (landEffectPrefab != null)
        {
            // 着地エフェクトはフラグ関係なく常に出す（もしくはPrefab未設定なら出ない）
            Vector3 spawnPos = transform.position + effectOffset;
            Instantiate(landEffectPrefab, spawnPos, Quaternion.identity);
        }
    }

    /// <summary>
    /// Animation Event から呼び出される歩行エフェクト再生用メソッド
    /// （アニメーションカーブ上から呼び出せるように公開しておく）
    /// </summary>
    public void PlayFootstepEffect()
    {
        if (footstepPrefab != null)
        {
            // 常時足跡ON、または不可視トラップ等からの強制表示フラグがONの時だけ出す
            if (isAlwaysFootstepOn || forceShowFootstep)
            {
                Vector3 spawnPos = transform.position + effectOffset;
                Instantiate(footstepPrefab, spawnPos, Quaternion.identity);
            }
        }
    }
}
