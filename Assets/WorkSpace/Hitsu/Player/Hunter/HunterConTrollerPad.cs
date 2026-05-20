using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ハンターに関する各種制御を行う管理クラス
/// 罠設置・コスト管理・UI更新など、
/// </summary>
public class HunterConTrollerPad : MonoBehaviour
{
    /// <summary> HunterConTrollerPad のシングルトンインスタンス </summary>
    public static HunterConTrollerPad Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("Basic")]
    /// <summary> ハンター側で使用するカメラ </summary>
    public Camera hunterCamera;
    /// <summary> ハンター側UI用Canvas </summary>
    public Canvas hunterCanvas;

    [Header("InPut")]
    /// <summary> ハンターカーソル制御 </summary>
    public HunterCursor hunterCursor;
    /// <summary> プレイヤー入力データ </summary>
    public PlayerInputData inputData;

    [Header("CommonUI")]
    /// <summary> 共通UI管理 </summary>
    public CommonUI commonUI;

    #region Cost
    /// <summary> コスト表示用UIイメージ </summary>
    public Image costImage;
    /// <summary> コスト数値表示用SpriteRenderer </summary>
    public SpriteRenderer[] costSpriteRenderers;
    /// <summary> 初期コスト量 </summary>
    private const int startCostCanUse = 5;
    /// <summary> 使用可能コスト最大値 </summary>
    private int maxCostCanUse => 20 + (5 * InGame.Instance.passCheckPoint);
    /// <summary> コスト回復量 </summary>
    private int costRecovery => 1 + InGame.Instance.passCheckPoint;
    /// <summary> 現在使用可能なコスト </summary>
    public int nowCostCanUse { get; private set; }
    /// <summary> コスト回復までの時間 </summary>
    private const float recoverCountDown = 3;
    /// <summary> 現在の回復タイマー </summary>
    private float recoverTimer = 0;

    /// <summary>
    /// コストUIを更新する
    /// </summary>
    private void CostUIUpdate()
    {
        var numbers = GameManager.Instance.numberSprites;
        // 数字Spriteが不足している場合
        if (numbers == null || numbers.Count < 10)
        {
            Debug.LogError("numberSprites not set correctly");
            return;
        }
        // SpriteRendererが不足している場合
        if (costSpriteRenderers == null || costSpriteRenderers.Length < 2)
        {
            Debug.LogError("costSpriteRenderers not enough");
            return;
        }
        // 十の位を取得
        int tens = (nowCostCanUse / 10) % 10;
        // 一の位を取得
        int ones = nowCostCanUse % 10;
        // UIへ数字を反映
        costSpriteRenderers[0].sprite = GameManager.Instance.numberSprites[tens];
        costSpriteRenderers[1].sprite = GameManager.Instance.numberSprites[ones];
    }
    /// <summary> コスト回復Coroutine </summary>
    private Coroutine costRecover;

    /// <summary>
    /// コストを一定時間ごとに回復する
    /// </summary>
    private IEnumerator CostRecover()
    {
        // ゲーム開始待機
        yield return new WaitUntil(() => InGame.Instance.gameStage == GameStage.Playing);

        while (true)
        {
            // コストが最大値以上の場合
            if (nowCostCanUse >= maxCostCanUse)
            {
                nowCostCanUse = maxCostCanUse;          //コストが最大値
                costImage.fillAmount = 1;               // UIゲージ最大表示
                CostUIUpdate();                         //コストUIを更新する
                recoverTimer = recoverCountDown;        // タイマー初期化

                // コスト消費待機
                yield return new WaitUntil(() => nowCostCanUse < maxCostCanUse);
            }
            else
            {
                // 回復タイマー更新
                recoverTimer += Time.deltaTime;
                // UIゲージ更新
                costImage.fillAmount = recoverTimer / recoverCountDown;
                // 回復時間到達
                if (recoverTimer >= recoverCountDown)
                {
                    costImage.fillAmount -= recoverCountDown;
                    // コスト回復
                    nowCostCanUse += costRecovery;
                    // 最大値制限
                    nowCostCanUse = nowCostCanUse > maxCostCanUse ? maxCostCanUse : nowCostCanUse;
                    //コストUIを更新する
                    CostUIUpdate();
                    // タイマー調整
                    recoverTimer -= recoverCountDown;

                    TrapRingsUpdate();
                }

                yield return null;
            }



        }

    }

    /// <summary>
    /// コスト回復処理を初期化する
    /// </summary>
    private void Cost_RecoveryInit()
    {
        // 既存Coroutine停止
        if (costRecover != null)
        {
            StopCoroutine(costRecover);
            costRecover = null;
        }

        nowCostCanUse = startCostCanUse;                // 初期コスト設定
        CostUIUpdate();                                 // コストUIを更新する
        recoverTimer = 0;                               // タイマー初期化
        costRecover = StartCoroutine(CostRecover());    // 回復Coroutine開始

    }

    #endregion

    #region Trap
    [Header("Trap")]
    /// <summary> 罠選択用ラジアルUI </summary>
    public TrapRing trapRings;
    /// <summary> 指定した罠のプレハブを取得する </summary>
    private GameObject TarpObject(TrapName trapName) => GameManager.allTrap[trapName].prefab;
    /// <summary> 指定した罠のアイコンSpriteを取得する </summary>
    private Sprite TrapSprite(TrapName trapName) => GameManager.allTrap[trapName].icon;
    /// <summary> 指定した罠の必要コストを取得する </summary>
    private int TrapCost(TrapName trapName) => GameManager.allTrap[trapName].cost;
    /// <summary> 使用可能な罠を初期化する </summary>
    private void CanUseTrapInit(List<TrapName> useTrapName) => trapRings.Init(useTrapName);
    /// <summary> 罠UIを更新する </summary>
    private void TrapRingsUpdate() => trapRings.UIUpdate();
    /// <summary> 罠説明表示用テキスト </summary>
    public TMP_Text trap_Introduce;

    /// <summary>
    /// 選択中の罠情報をUIへ表示する
    /// </summary>
    private void TrapIntroduce(TrapName trap)
    {
        string trapName = $"Trap Name : {trap.ToString()}\n";                               // 罠名
        string cost = $"Need Cost : {GameManager.allTrap[trap].cost}\n";                    // 必要コスト
        string introduce = $"Introduce : {GameManager.allTrap[trap].inGame_information}";   // 罠説明

        trap_Introduce.text = trapName + cost + introduce;                                  // UIへ反映

        Debug.Log(trap_Introduce.text);
    }
    #endregion

    #region Put Trap
    /// <summary> 現在選択中の罠オブジェクト </summary>
    private GameObject choseTrap;
    /// <summary> 罠設置Coroutine </summary>
    private Coroutine createTrap;
    /// <summary> カーソル位置をグリッドへスナップしたワールド座標 </summary>
    private Vector3 cursorPos
    {
        get
        {
            // カーソルのワールド座標取得
            Vector3 world = hunterCursor.worldPos;
            // ワールド座標をグリッド座標へ変換
            Vector2Int grid = StageGridManager.Instance.WorldToGrid(world);
            // グリッド座標をワールド座標へ戻しスナップ
            world = StageGridManager.Instance.GridToWorld(grid);
            // Z座標固定
            world.z = 0;

            return world;
        }
    }

    /// <summary>
    /// 罠設置時にコストを消費する
    /// </summary>
    private void UseCost(TrapName trap)
    {
        nowCostCanUse -= TrapCost(trap);    // コスト減少
        CostUIUpdate();                     // UI更新
        TrapRingsUpdate();                  // 罠UI更新
    }

    private void GetTrapPlacer(TrapName trapName, GameObject target, out TrapPlacer trapPlacer)
    {
        bool targetPlacer = trapName == TrapName.Spikes || trapName == TrapName.IceArea || trapName == TrapName.GlueArea;
        if (target.GetComponent<TrapPlacer>() == null)
        {
            trapPlacer = targetPlacer ? target.AddComponent<WallTrapPlacer>() : target.AddComponent<StandardTrapPlacer>();
        }
        else trapPlacer = target.GetComponent<TrapPlacer>();
        trapPlacer.InitializePreview();
    }

    /// <summary>
    /// 罠の設置処理を行うCoroutine
    /// </summary>
    private IEnumerator PuttingTrap(TrapName trapName)
    {
        // TrapRing未設定チェック
        if (trapRings == null)
        {
            Debug.LogError("TrapRing is Null");
            yield break;
        }
        // 無効な罠チェック
        if (trapName == TrapName.None || TarpObject(trapName) == null) yield break;

        while (true)
        {
            // 罠説明UI更新
            TrapIntroduce(trapName);
            // 罠生成
            choseTrap = Instantiate(TarpObject(trapName), cursorPos, TarpObject(trapName).transform.rotation);
            // TrapPlacer取得
            GetTrapPlacer(trapName, choseTrap, out TrapPlacer placer);

            while (true)
            {
                placer.UpdatePreviewPosition(cursorPos);

                // 設置可能判定
                bool canPlacePreview = placer.ValidatePlacement() && nowCostCanUse >= GameManager.allTrap[trapName].cost;
                // プレビュー色更新
                placer.UpdatePreviewColor(canPlacePreview);
                // 設置・入力検知
                if (inputData.wasPutPressedThisFrame && canPlacePreview)
                {
                    break;
                }
                yield return null;
            }

            placer.RestoreVisuals();
            placer.enabled = false;

            UseCost(trapName);                              // コスト消費
            Trap trap = choseTrap.GetComponent<Trap>();     // Trap初期化
            trap.Init();                                
            trap.SetUp();

            InGame.Instance.AddTrap(choseTrap);             // 生成した罠をリストに登録する

            // 現在の設置情報をリセット
            createTrap = null;
            choseTrap = null;

            yield return null;
        }

    }

    /// <summary>
    /// 現在の罠設置処理をキャンセルする
    /// </summary>
    private void Reject()
    {
        // Coroutine停止
        if (createTrap != null)
        {
            StopCoroutine(createTrap);
            createTrap = null;
        }
        // プレビュー罠削除
        if (choseTrap != null)
        {
            Destroy(choseTrap);
            choseTrap = null;
        }
    }

    /// <summary>
    /// 新しい罠設置処理を開始する
    /// </summary>
    public void CreateTrap(TrapName trapName)
    {
        // 現在の Trap 設置処理をキャンセル
        Reject();
        // Trap 設置 Coroutine を開始
        createTrap = StartCoroutine(PuttingTrap(trapName));
    }


    #endregion

    /// <summary>
    /// ハンター操作対象を切り替える
    /// </summary>
    public void HunterSwitch(Player targetPlayer)
    {
        CanUseTrapInit(targetPlayer.hunter.backpack.trapsPack);         // 使用可能な罠を初期化
        trap_Introduce.text = "";                                       // 罠説明UIリセット
        Cost_RecoveryInit();                                            // コスト回復処理初期化
        TrapRingsUpdate();                                              // 罠UI更新
        TrapIntroduce(trapRings.chooseTrapName);
        commonUI.CommonUIInit();                                        // 共通UI初期化
        Reject();                                                       // 現在の設置処理をキャンセル
    }

    public void HunterInit()
    {
        hunterCursor.Init(this);
    }

    private void Update()
    {
        trapRings.ControllerChoose(inputData.chooseInput);
    }


}


