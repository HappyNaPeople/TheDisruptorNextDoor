using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
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
    // Hunter が操作する Gamepad
    private Gamepad hunterUseController;

    /// <summary>
    /// Hunter が使用する Gamepad を設定する
    /// </summary>
    /// <param name="targetGamePad">使用する Gamepad</param>
    public void GamePadInit(Gamepad targetGamePad) => hunterUseController = targetGamePad;


    #region Cost
    [Header("Cost")]
    private float maxCostCanUse => 20.0f + (5.0f * InGame.Instance.passCheckPoint);
    private const float startCostCanUse = 5.0f;

    private float costRecoveryPerSec => (1.0f + InGame.Instance.passCheckPoint) / 3;
    private float nowCostCanUse = 0.0f;

    private Coroutine costRecover;

    public Image costImage;
    private float FillAmount() => nowCostCanUse / maxCostCanUse;

    public TMP_Text costText;
    private string NowCost() => Convert.ToInt32(nowCostCanUse).ToString("D2");

    private void RecoveryInit()
    {
        if (costRecover != null) StopCoroutine(costRecover);
        nowCostCanUse = startCostCanUse;
        costRecover = StartCoroutine(CostRecover());
    }

    private IEnumerator CostRecover()
    {
        while (true)
        {
            if (nowCostCanUse >= maxCostCanUse) nowCostCanUse = maxCostCanUse;
            else if (nowCostCanUse < 0) nowCostCanUse = 0;
            else nowCostCanUse += costRecoveryPerSec * Time.deltaTime;

            costImage.fillAmount = FillAmount();
            costText.text = NowCost();
            yield return null;

        }

    }

    private bool CanUseTrap(TrapName trap) => (Convert.ToInt32(nowCostCanUse) - TrapCost(trap)) > 0;
    private void UseCost(TrapName trap) => nowCostCanUse -= TrapCost(trap);

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

    [Header("Trap Choose Button")]
    // Trap UI ボタンリスト
    public List<TrapButtonUI> trapButtonList;

    private HashSet<Trap> trapList = new HashSet<Trap>();

    private void DestroyTrapFromList(Trap target)
    {
        if (!trapList.Contains(target))
        {
            Debug.LogWarning("Why there have unrecord Trap");
            return;
        }
        trapList.Remove(target);
    }

    public void DestroyTrap(Trap targetTrap)
    {
        DestroyTrapFromList(targetTrap);
        Destroy(targetTrap.gameObject);
    }

    private void ResetRoundTraps()
    {
        foreach (Trap traps in trapList)
        {
            if (traps.gameObject != null) DestroyTrap(traps);
        }
        trapList.Clear();
    }

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
            // 一旦すべて非表示
            trapButtonList[index].gameObject.SetActive(false);
            // 既存のクリックイベントを削除
            trapButtonList[index].button.onClick.RemoveAllListeners();

            // 使用可能 Trap が存在する場合
            if (index < trapTypeMax)
            {
                TrapName trap = useTrapName[index];

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
    Vector3 mouseWorldPos
    {
        get
        {
            // マウスのスクリーン座標取得
            Vector2 mousePos = GameManager.inputDevice.mouse.position.ReadValue();

            // カメラとの距離を考慮してワールド座標へ変換
            float distance = Mathf.Abs(hunterCamera.transform.position.z);
            Vector3 world = hunterCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distance));


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
    private bool IsOnMap() => Physics2D.OverlapPoint(mouseWorldPos, mask) != null;

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
        GameObject targetTrap = Instantiate(TarpObject(trapName), mouseWorldPos, TarpObject(trapName).transform.rotation);
        // Runner に見えない Layer に変更
        targetTrap.layer = UseLayerName.runnerCantSeeLayer;
        if (targetTrap.transform.childCount > 0)
        {
            for (int i = 0; i < targetTrap.transform.childCount; i++)
            {
                targetTrap.transform.GetChild(i).gameObject.layer = UseLayerName.runnerCantSeeLayer;
            }

        }

        // プレビュー用 Trap
        choseTrap = targetTrap;
        // クリックされるまでマウス追従
        while (!GameManager.inputDevice.mouse.leftButton.isPressed)
        {
            targetTrap.transform.position = mouseWorldPos;
            yield return null;
        }
        // 設置可能エリアなら Trap 有効化
        if (IsInPutArea(targetTrap.transform.position) && !IsOnMap())
        {
            Trap trap = targetTrap.GetComponent<Trap>();
            trap.Init();
            trap.SetUp();
            trapList.Add(trap);
            UseCost(trap.trapName);
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


    #endregion

    /// <summary>
    /// Hunter プレイヤーが切り替わった時に呼ばれる
    /// Backpack に登録されている Trap を UI に反映する
    /// </summary>
    public void HunterSwitch(Player targetPlayer)
    {
        CanUseTrapInit(targetPlayer.hunter.backpack.trapsPack);
        ResetRoundTraps();
        RecoveryInit();
    }


    /// <summary>
    /// Singleton 初期化
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }


    public bool test = false;
    private void Update()
    {
        if (test)
        {
            if (CanUseTrap(TrapName.FallRock))
            {
                UseCost(TrapName.FallRock);
            }


            test = false;
        }


    }


}
