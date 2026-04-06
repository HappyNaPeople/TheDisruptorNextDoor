using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;

/// <summary>
/// Hunter が使用する Trap 設置コントローラー。
///
/// 主な役割：
/// ・Trap の Sprite / Prefab データ管理
/// ・Trap UI ボタンの初期化
/// ・マウス位置への Trap プレビュー表示
/// ・Trap 設置可能エリアの判定
/// ・Trap の生成と初期化
///
/// Trap の設置は Coroutine を使用し、
/// プレイヤーがクリックするまでマウス位置に追従する。
/// </summary>
public class HunterConTrollerPad : MonoBehaviour
{
    // Singleton インスタンス
    public static HunterConTrollerPad Instance;
    // Hunter 用カメラ（マウス座標 → ワールド座標変換に使用）
    public Camera hunterCamera;
    public Canvas hunterCanvas;
    public TimeAndProgressBar timeAndProgressBar;
    public HunterCursor hunterCursor;
    public PlayerInputData inputData;

    /// <summary>
    /// Hunter が使用する Gamepad を設定する
    /// </summary>
    /// <param name="targetGamePad">使用する Gamepad</param>

    #region Cost
    [Header("Cost")]
    /// <summary>
    /// ・時間経過でコスト回復
    /// ・最大コストはチェックポイント数に応じて増加
    /// ・UI（ゲージ・テキスト）を更新
    /// </summary>

    // 最大コスト（チェックポイントに応じて増加）
    private int maxCostCanUse => 20 + (5 * InGame.Instance.passCheckPoint);
    // 初期コスト
    private const int startCostCanUse = 5;
    // 秒ごとのコスト回復量（チェックポイント依存）
    private int costRecovery => 1 + InGame.Instance.passCheckPoint;
    // 現在のコスト値
    private int nowCostCanUse = 0;

    // コスト回復用コルーチン
    private Coroutine costRecover;
    private const float recoverCountDown = 3;
    private float recoverTimer = 0;


    // ボタン状態更新用コルーチン
    private Coroutine buttonActive;
    // コストゲージUI
    public Image costImage;
    // コスト表示テキスト
    public TMP_Text costText;
    // 現在コストを2桁表示（例：05）
    private string NowCost() => Convert.ToInt32(nowCostCanUse).ToString("D2");


    /// <summary>
    /// コストを時間経過で回復する処理
    /// ・最大値に到達した場合は固定
    /// ・UI（ゲージ・テキスト）を更新
    /// </summary>
    private IEnumerator CostRecover()
    {
        while (true)
        {
            // 最大値に到達した場合
            if (nowCostCanUse >= maxCostCanUse)
            {
                nowCostCanUse = maxCostCanUse;
                // ゲージを満タンに
                costImage.fillAmount = 1;
                // テキスト更新
                costText.text = NowCost();

                recoverTimer = recoverCountDown;

                yield return new WaitUntil(() => nowCostCanUse < maxCostCanUse);
            }
            else
            {
                recoverTimer += Time.deltaTime;
                // ゲージ進行（一定時間で1周）

                costImage.fillAmount = recoverTimer / recoverCountDown;

                // ゲージ1周ごとに数値更新
                if (recoverTimer >= recoverCountDown)
                {
                    costImage.fillAmount -= recoverCountDown;
                    nowCostCanUse += costRecovery;

                    nowCostCanUse = nowCostCanUse > maxCostCanUse ? maxCostCanUse : nowCostCanUse;
                    costText.text = NowCost();
                    ButtonActive();

                    recoverTimer -= recoverCountDown;
                }

                yield return null;
            }



        }

    }


    // 非アクティブ時の色（白）
    private const string activeColorCode = "#FFFFFF";
    private Color activeColor;
    // アクティブ時の色（グレー）
    private const string nonActiveColorCode = "#707070";
    private Color nonActiveColor;

    /// <summary>
    /// ボタンのアクティブ / 非アクティブ時に使用するカラーを初期化
    /// ・HTMLカラーコードを Color に変換
    /// ・一度だけ実行して再利用することでパフォーマンスを向
    private void ColorInit()
    {
        // アクティブ時のカラーを取得
        ColorUtility.TryParseHtmlString(activeColorCode, out Color active);
        // 非アクティブ時のカラーを取得
        ColorUtility.TryParseHtmlString(nonActiveColorCode, out Color nonActive);

        activeColor = active;
        nonActiveColor = nonActive;
    }

    /// <summary>
    /// ボタンの有効 / 無効状態を切り替える
    /// ・色と操作可否を変更
    /// </summary>
    private void Action(TrapButtonUI targetButton, bool isActive)
    {
        // ボタンの操作可否
        targetButton.button.interactable = isActive;

        // 状態に応じた色を設定
        Color targetColor = isActive ? activeColor : nonActiveColor;

        // UIに反映
        targetButton.button.image.color = targetColor;
        targetButton.icon.color = targetColor;

    }

    /// <summary>
    /// 各トラップボタンの状態更新
    /// ・コストに応じて使用可能かを判定
    /// ・使用不可の場合はボタンを無効化
    /// </summary>
    private void ButtonActive()
    {
        foreach (TrapButtonUI targetCost in trapButtonList)
        {
            // 非表示オブジェクトはスキップ
            if (!targetCost.gameObject.activeSelf) continue;

            Action(targetCost, nowCostCanUse >= TrapCost(targetCost.trapName));
        }

    }

    /// <summary>
    /// コスト回復処理の初期化
    /// ・既存の回復コルーチンを停止
    /// ・初期コストとUIを設定
    /// ・回復タイマーをリセット
    /// ・回復処理を再開
    /// </summary>
    private void RecoveryInit()
    {
        // 既に回復処理が動いている場合は停止
        if (costRecover != null)
        {
            StopCoroutine(costRecover);
            costRecover = null;
        }

        // 初期コスト設定
        nowCostCanUse = startCostCanUse;
        // コスト表示テキスト更新
        costText.text = NowCost();
        // 回復タイマーをリセット（最初からカウント開始）
        recoverTimer = 0;
        // ボタン状態更新（初期コストに応じて有効/無効を切り替え）
        ButtonActive();

        // 回復処理開始
        costRecover = StartCoroutine(CostRecover());


    }


    /// <summary>
    /// トラップが使用可能か判定
    /// </summary>
    private bool CanUseTrap(TrapName trap) => (nowCostCanUse - TrapCost(trap)) >= 0;
    
    /// <summary>
    /// トラップ使用時のコスト消費処理
    /// </summary>
    private void UseCost(TrapName trap)
    {
        // コスト減少
        nowCostCanUse -= TrapCost(trap);

        // UI更新
        costText.text = NowCost();

        ButtonActive();
    }


    #endregion

    #region Trap UI

    /// <summary>
    /// TrapName から Trap Prefab を取得する
    /// </summary>
    private GameObject TarpObject(TrapName trapName) => GameManager.allTrap[trapName].prefab;

    /// <summary>
    /// TrapName から Trap Sprite を取得する
    /// </summary>
    private Sprite TrapSprite(TrapName trapName) => GameManager.allTrap[trapName].icon;

    private int TrapCost(TrapName trapName) => GameManager.allTrap[trapName].cost;

    //[Header("Setup Trap Count")]
    //public TMP_Text setupTrapText;
    //public void UpdateSetupTrapText() => setupTrapText.text = $"Traps Setup : {InGame.Instance.allTheTrap.Count.ToString("D2")} / {InGame.trapMax.ToString("D2")}";

    [Header("Trap Choose Button")]
    // Trap UI ボタンリスト
    public List<TrapButtonUI> trapButtonList;

    //private HashSet<Trap> trapList = new HashSet<Trap>();

    //private void DestroyTrapFromList(Trap target)
    //{
    //    if (!trapList.Contains(target))
    //    {
    //        Debug.LogWarning("Why there have unrecord Trap");
    //        return;
    //    }
    //    trapList.Remove(target);
    //}

    //public void DestroyTrap(Trap targetTrap)
    //{
    //    DestroyTrapFromList(targetTrap);
    //    Destroy(targetTrap.gameObject);
    //}

    //private void ResetRoundTraps()
    //{
    //    foreach (Trap traps in trapList)
    //    {
    //        if (traps.gameObject != null) DestroyTrap(traps);
    //    }
    //    trapList.Clear();
    //}

    /// <summary>
    /// 使用可能な Trap を UI ボタンに設定する
    /// </summary>
    /// <param name="targetTrap">使用可能な TrapName のリスト</param>
    private void CanUseTrapInit(List<TrapName> useTrapName)
    {
        // 重複する Trap を除外
        //targetTrap.Distinct().ToList();
        int index;

        // UI ボタン数と Trap 数の少ない方を使用
        int trapTypeMax = useTrapName.Count > trapButtonList.Count ? trapButtonList.Count : useTrapName.Count;

        // UI ボタンを初期化
        for (index = 0; index < trapButtonList.Count; index++)
        {
            if (trapButtonList[index] == null)
            {
                Debug.LogError($"[HunterConTrollerPad] Trap Button List の {index} 番目が空(Missing/None)です。インスペクターを確認してください！");
                continue;
            }
            if (trapButtonList[index].button == null)
            {
                Debug.LogError($"[HunterConTrollerPad] Trap Button List の {index} 番目のボタンの参照が外れています。プレハブの設定を確認してください！");
                continue;
            }

            // 一旦すべて非表示
            trapButtonList[index].gameObject.SetActive(false);
            // 既存のクリックイベントを削除
            trapButtonList[index].button.onClick.RemoveAllListeners();

            // 使用可能 Trap が存在する場合
            if (index < trapTypeMax)
            {
                TrapName trap = useTrapName[index];

                trapButtonList[index].trapName = trap;
                //trapButtonList[index].button.onClick.AddListener(() => CreateTrap(trap));

                trapButtonList[index].button.onClick.AddListener(() => CreateTrap(trap));

                trapButtonList[index].icon.sprite = TrapSprite(trap);
                trapButtonList[index].cost.text = TrapCost(trap).ToString();
                // ボタン表示
                trapButtonList[index].gameObject.SetActive(true);




            }
            else
            {
                // 使用しないボタンは非表示
                trapButtonList[index].gameObject.SetActive(false);
            }
        }

        testing_CanUseTrap = useTrapName;
    }

    private List<TrapName> testing_CanUseTrap = new List<TrapName>();


    private void test_RandomButton(int trapButtonUICode, TrapName usedTrap)
    {
        TrapName newTrap = testing_CanUseTrap[UnityEngine.Random.Range(0, testing_CanUseTrap.Count)];
        while (newTrap == usedTrap) { newTrap = testing_CanUseTrap[UnityEngine.Random.Range(0, testing_CanUseTrap.Count)]; }

        trapButtonList[trapButtonUICode].trapName = newTrap;
        //trapButtonList[index].button.onClick.AddListener(() => CreateTrap(trap));

        trapButtonList[trapButtonUICode].button.onClick.AddListener(() => test_CreateTrap(trapButtonUICode,newTrap));

        trapButtonList[trapButtonUICode].icon.sprite = TrapSprite(newTrap);
        trapButtonList[trapButtonUICode].cost.text = TrapCost(newTrap).ToString();
        // ボタン表示
        trapButtonList[trapButtonUICode].gameObject.SetActive(true);

    }


    #endregion

    #region PuttingTrap

    [Header("PuttingTrap")]
    // Trap を設置できるエリアの左上座標
    public Transform putAreaLeftTop;
    // Trap を設置できるエリアの右下座標
    public Transform putAreaRightDown;

    // グリッドスナップを使用するかどうか
    public bool isGrid = true;

    /// <summary>
    /// マウス位置をワールド座標で取得する
    /// グリッドスナップが有効な場合はグリッドに合わせる
    /// </summary>
    Vector3 cursorPos
    {
        get
        {
            Vector3 world = hunterCursor.worldPos;

            Vector2Int grid = StageGridManager.Instance.WorldToGrid(world);
            // グリッド → ワールド（スナップ後の正しい位置）
            world = StageGridManager.Instance.GridToWorld(grid);

            world.z = 0;

            return world;
        }
    }

    /// <summary>
    /// 指定された座標が設置可能エリア内にあるか判定する
    /// </summary>
    /// <param name="leftTop">設置エリア左上座標</param>
    /// <param name="rightDown">設置エリア右下座標</param>
    /// <param name="trap">判定対象の座標</param>
    /// <returns>エリア内なら true</returns>
    private bool IsInPutArea(Vector3 trap)
    {
        if (trap.x > putAreaRightDown.position.x ||
            trap.x < putAreaLeftTop.position.x ||
            trap.y < putAreaRightDown.position.y ||
            trap.y > putAreaLeftTop.position.y) return false;

        return true;
    }

    private int mask = UseLayerName.platformLayer | UseLayerName.trapLayer | UseLayerName.noPutAreaLayer;

    /// <summary>
    /// マップ上に Trap を設置しようとしているか判定
    /// </summary>
    private bool IsOnMap() => Physics2D.OverlapPoint(cursorPos, mask) != null;

    // プレビュー中の Trap
    private GameObject choseTrap;
    // Trap 設置 Coroutine
    private Coroutine createTrap;

    /// <summary>
    /// Trap 設置 Coroutine
    /// マウスクリックされるまで Trap をマウス位置に追従させる
    /// </summary>
    private IEnumerator PutTrap(TrapName trapName)
    {
        // Prefab が存在しない場合は終了
        if (TarpObject(trapName) == null)
        {
            Debug.Log("No Trap");
            yield break;
        }

        // Trap を生成
        GameObject targetTrap = Instantiate(TarpObject(trapName), cursorPos, TarpObject(trapName).transform.rotation);
        
        // TrapPlacer を取得またはアタッチ
        TrapPlacer placer = targetTrap.GetComponent<TrapPlacer>();
        if (placer == null)
        {
            if (trapName == TrapName.BlackHole)
                placer = targetTrap.AddComponent<WorldTrapPlacer>();
            else if (trapName == TrapName.Spikes)
                placer = targetTrap.AddComponent<WallTrapPlacer>();
            else
                placer = targetTrap.AddComponent<StandardTrapPlacer>();
        }

        placer.InitializePreview();
        choseTrap = targetTrap;

        // クリックされるまでマウス追従
        while (!inputData.isPutPressed)
        {
            placer.UpdatePreviewPosition(cursorPos);
            bool canPlacePreview = placer.ValidatePlacement();
            placer.UpdatePreviewColor(canPlacePreview);

            yield return null;
        }

        bool canPlace = placer.ValidatePlacement();

        // 設置可能エリアなら Trap 有効化
        if (canPlace)
        {
            placer.RestoreVisuals();
            Trap trap = targetTrap.GetComponent<Trap>();
            trap.Init();
            trap.SetUp();
            //trapList.Add(trap);
            UseCost(trap.trapName);
            Destroy(placer); // 設置後は不要なので削除
            InGame.Instance.AddTrap(trap.gameObject);
        }
        else
        {
            // 設置不可なら削除
            Destroy(targetTrap);
        }

        // 状態リセット
        createTrap = null;
        choseTrap = null;
    }

    /// <summary>
    /// 現在の Trap 設置処理をキャンセルする
    /// </summary>
    private void Reject()
    {
        if (choseTrap != null)
        {
            Destroy(choseTrap);
            choseTrap = null;
        }

        if (createTrap != null)
        {
            StopCoroutine(createTrap);
            createTrap = null;
        }
    }

    /// <summary>
    /// 指定された Trap を生成する。
    /// 
    /// 既にプレビュー中の Trap が存在する場合はキャンセルし、
    /// 新しい Trap の設置 Coroutine を開始する。
    /// </summary>
    /// <param name="trapName">
    /// 生成する Trap の種類
    /// </param>
    private void CreateTrap(TrapName trapName)
    {
        // 現在の Trap 設置処理をキャンセル
        Reject();
        // Trap 設置 Coroutine を開始
        createTrap = StartCoroutine(PutTrap(trapName));
    }

    private void test_CreateTrap(int trapButtonUI, TrapName trapName)
    {
        // 現在の Trap 設置処理をキャンセル
        Reject();
        // Trap 設置 Coroutine を開始
        createTrap = StartCoroutine(PutTrap(trapName));
    }




    #endregion



    /// <summary>
    /// Hunter プレイヤーが切り替わった時に呼ばれる
    /// Backpack に登録されている Trap を UI に反映する
    /// </summary>
    public void HunterSwitch(Player targetPlayer)
    {
        //UpdateSetupTrapText();
        CanUseTrapInit(targetPlayer.hunter.backpack.trapsPack);
        //ResetRoundTraps();
        RecoveryInit();
        timeAndProgressBar.ProgressBarInit();
    }

    public void HunterInit()
    {
        hunterCursor.Init(this);
    }

    /// <summary>
    /// Singleton 初期化
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        ColorInit();
    }

    public bool test = false;

    private void Update()
    {
        if (test)
        {
            test = false;
        }


    }


}

