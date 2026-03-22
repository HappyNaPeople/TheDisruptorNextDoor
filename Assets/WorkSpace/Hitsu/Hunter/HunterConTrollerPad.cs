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
    public Canvas hunterCanvas;

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
    private float maxCostCanUse => 20.0f + (5.0f * InGame.Instance.passCheckPoint);
    // 初期コスト
    private const float startCostCanUse = 5.0f;
    // 秒ごとのコスト回復量（チェックポイント依存）
    private float costRecoveryPerSec => (1.0f + InGame.Instance.passCheckPoint) / 3;
    // 現在のコスト値
    private float nowCostCanUse = 0.0f;
    // コスト回復用コルーチン
    private Coroutine costRecover;
    // ボタン状態更新用コルーチン
    private Coroutine buttionActicve;
    // コストゲージUI
    public Image costImage;
    // コスト表示テキスト
    public TMP_Text costText;
    // 現在コストを2桁表示（例：05）
    private string NowCost() => Convert.ToInt32(nowCostCanUse).ToString("D2");

    /// <summary>
    /// コスト回復処理の初期化
    /// ・既存コルーチン停止
    /// ・初期コスト設定
    /// ・回復処理開始
    /// </summary>
    private void RecoveryInit()
    {
        // 既に回復処理が動いている場合は停止
        if (costRecover != null) StopCoroutine(costRecover);
        costRecover = null;
        // ボタン更新処理が動いている場合は停止
        if (buttionActicve != null) StopCoroutine(buttionActicve);
        buttionActicve = null;

        // 初期コスト設定
        nowCostCanUse = startCostCanUse;
        // 回復処理開始
        costRecover = StartCoroutine(CostRecover());
        // ボタン状態更新処理開始
        buttionActicve = StartCoroutine(ButtionActicve());
    }

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
            }
            else
            {
                // コスト回復
                nowCostCanUse += costRecoveryPerSec * Time.deltaTime;
                // ゲージ進行（一定時間で1周）
                costImage.fillAmount += Time.deltaTime / 3;
                // ゲージ1周ごとに数値更新
                if (costImage.fillAmount >= 1)
                {
                    costImage.fillAmount = 0;
                    costText.text = NowCost();
                }
            }

            yield return null;

        }

    }

    // 非アクティブ時の色
    private const string nonActiveColor = "FFFFFF";
    // アクティブ時の色
    private const string activeColor = "707070";

    /// <summary>
    /// ボタンの有効 / 無効状態を切り替える
    /// ・色と操作可否を変更
    /// </summary>
    private void Action(TrapButtonUI targetButton, bool isActive)
    {
        // ボタンの操作可否
        targetButton.button.enabled = isActive;

        // 状態に応じた色を設定
        string targetColor = isActive ? activeColor : nonActiveColor;

        // HTMLカラー → Color変換
        ColorUtility.TryParseHtmlString(targetColor, out Color fromHex);

        // UIに反映
        targetButton.button.image.color = fromHex;
        targetButton.icon.color = fromHex;


    }

    /// <summary>
    /// 各トラップボタンの状態更新
    /// ・コストに応じて使用可能かを判定
    /// ・使用不可の場合はボタンを無効化
    /// </summary>
    private IEnumerator ButtionActicve()
    {
        while (true)
        {
            foreach(TrapButtonUI targetCost in trapButtonList)
            {
                // 非表示オブジェクトはスキップ
                if (targetCost.gameObject.activeSelf == false) continue;

                // コスト不足なら無効化
                if (TrapCost(targetCost.trapName) < nowCostCanUse) Action(targetCost, false);
                else Action(targetCost, true);
            }

            yield return null;
        }
    }

    /// <summary>
    /// トラップが使用可能か判定
    /// </summary>
    private bool CanUseTrap(TrapName trap) => (Convert.ToInt32(nowCostCanUse) - TrapCost(trap)) > 0;
    
    /// <summary>
    /// トラップ使用時のコスト消費処理
    /// </summary>
    private void UseCost(TrapName trap)
    {
        // コスト減少
        nowCostCanUse -= TrapCost(trap);
        // UI更新
        costText.text = NowCost();
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

                trapButtonList[index].trapName = trap;
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
    private int gridSize = 1;
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


    /// <summary>
    /// 指定位置の周囲に壁や床があるか判定し、スパイクの回転を決定する
    /// </summary>
    private bool CheckSpikePlacement(Vector3 pos, out Quaternion rotation)
    {
        rotation = Quaternion.identity;
        float checkDist = gridSize; 
        int layerMask = 1 << UseLayerName.platformLayer;
        
        // 下に床があれば上向き
        if (Physics2D.OverlapPoint(pos + Vector3.down * checkDist, layerMask))
        {
            rotation = Quaternion.Euler(0, 0, 0);
            return true;
        }
        // 上に天井があれば下向き
        if (Physics2D.OverlapPoint(pos + Vector3.up * checkDist, layerMask))
        {
            rotation = Quaternion.Euler(0, 0, 180);
            return true;
        }
        // 右に壁があれば左向き
        if (Physics2D.OverlapPoint(pos + Vector3.right * checkDist, layerMask))
        {
            rotation = Quaternion.Euler(0, 0, 90);
            return true;
        }
        // 左に壁があれば右向き
        if (Physics2D.OverlapPoint(pos + Vector3.left * checkDist, layerMask))
        {
            rotation = Quaternion.Euler(0, 0, -90);
            return true;
        }
        return false;
    }

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

        SpriteRenderer[] renderers = targetTrap.GetComponentsInChildren<SpriteRenderer>();
        Color[] originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) originalColors[i] = renderers[i].color;

        // クリックされるまでマウス追従
        while (!GameManager.inputDevice.mouse.leftButton.isPressed)
        {
            Vector3 mPos = mouseWorldPos;
            targetTrap.transform.position = mPos;
            
            bool canPlacePreview = IsInPutArea(mPos) && !IsOnMap();
            
            if (trapName == TrapName.Spikes)
            {
                if (CheckSpikePlacement(mPos, out Quaternion rot))
                {
                    targetTrap.transform.rotation = rot;
                }
                else
                {
                    canPlacePreview = false;
                }
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].color = canPlacePreview ? originalColors[i] : new Color(1f, 0f, 0f, 0.5f);
            }

            yield return null;
        }
        
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].color = originalColors[i];
        }

        bool canPlace = IsInPutArea(targetTrap.transform.position) && !IsOnMap();
        if (trapName == TrapName.Spikes)
        {
            canPlace = canPlace && CheckSpikePlacement(targetTrap.transform.position, out _);
        }

        // 設置可能エリアなら Trap 有効化

        if (canPlace)
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
