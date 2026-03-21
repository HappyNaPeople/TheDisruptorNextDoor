using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using System;

public class RoundTrap
{
    public Trap trap { get; private set; }
    public int round { get; private set; }

    public RoundTrap(Trap trap, int round)
    {
        this.trap = trap;
        this.round = round;
    }


}

public class TrapCount
{
    public TrapName trap { get; private set; }
    public int max { get; private set; }
    public int now { get; private set; }

    public TrapCount(TrapName target, int maxCount)
    {
        trap = target;
        max = maxCount;
        now = max;
    }

    public void Recovery() => now = now + 1 > max ? max : now + 1;
    public void Reduce() => now = now - 1 < 0 ? 0 : now - 1;



    public override bool Equals(object obj)
    {
        if (obj is TrapCount other)
        {
            return trap == other.trap;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(trap);
    }



}

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

    // Trap を設置できるエリアの左上座標
    public Transform putAreaLeftTop;
    // Trap を設置できるエリアの右下座標
    public Transform putAreaRightDown;

    /// <summary>
    /// TrapName から Trap Prefab を取得する
    /// </summary>
    private GameObject TarpObject(TrapName trapName) => GameManager.allTrap[trapName].prefab;

    /// <summary>
    /// TrapName から Trap Sprite を取得する
    /// </summary>
    private Sprite TrapSprite(TrapName trapName) => GameManager.allTrap[trapName].icon;

    // Trap UI ボタンリスト
    public List<Button> trapButtonList;

    private HashSet<TrapCount> trapCounts = new HashSet<TrapCount>();

    private TrapCount TargetTrap(TrapName trapName)
    {
        foreach (TrapCount target in trapCounts)
        {
            if (target.trap == trapName) return target;
        }

        Debug.LogError("Why nonChose Trap in the button which is can put");
        return null;
    }

    private bool CanUseTrap(TrapName trapName)
    {
        TrapCount target = TargetTrap(trapName);

        if (target != null) return target.now < 0;

        Debug.LogWarning("Why nonChose Trap in the button which is can put");

        return false;
    }

    /// <summary>
    /// 使用可能な Trap を UI ボタンに設定する
    /// </summary>
    /// <param name="targetTrap">使用可能な TrapName のリスト</param>
    private void CanUseTrapInit(List<TrapCanUse> targetTrap)
    {
        // 重複する Trap を除外
        //targetTrap.Distinct().ToList();
        int index;

        // UI ボタン数と Trap 数の少ない方を使用
        int trapTypeMax = targetTrap.Count > trapButtonList.Count ? trapButtonList.Count : targetTrap.Count;

        // UI ボタンを初期化
        for (index = 0; index < trapButtonList.Count; index++)
        {
            // 一旦すべて非表示
            trapButtonList[index].gameObject.SetActive(false);
            // 既存のクリックイベントを削除
            trapButtonList[index].onClick.RemoveAllListeners();

            // 使用可能 Trap が存在する場合
            if (index < trapTypeMax)
            {
                TrapName trap = targetTrap[index].trap;
                trapCounts.Add(new TrapCount(trap, targetTrap[index].trapCount));

                trapButtonList[index].onClick.AddListener(() => CreateTrap(trap));

                // Trap アイコンを設定
                trapButtonList[index].image.sprite = TrapSprite(targetTrap[index].trap);
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

    /// <summary>
    /// Hunter プレイヤーが切り替わった時に呼ばれる
    /// Backpack に登録されている Trap を UI に反映する
    /// </summary>
    public void HunterSwitch(Player targetPlayer)
    {
        CanUseTrapInit(targetPlayer.hunter.backpack.trapsPack);
        ResetRoundTraps();
    }

    /// <summary>
    /// Singleton 初期化
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    // グリッドスナップを使用するかどうか
    public bool isGrid = true;
    // グリッドの1マスのサイズ
    public float gridSize = 1f;

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


            //if (isGrid)
            //{
            //    // ワールド → グリッド
            //    Vector2Int grid = StageGridManager.Instance.WorldToGrid(world);

            //    // グリッド → ワールド（スナップ後の正しい位置）
            //    world = StageGridManager.Instance.GridToWorld(grid);
            //}

            // 2DなのでZは固定
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
    private bool IsInPutArea(Vector3 leftTop, Vector3 rightDown, Vector3 trap)
    {
        if (trap.x > rightDown.x ||
            trap.x < leftTop.x ||
            trap.y < rightDown.y ||
            trap.y > leftTop.y) return false;

        return true;
    }
    /// <summary>
    /// 指定された座標が Trap 設置可能エリア内か判定する。
    /// putAreaLeftTop と putAreaRightDown を使用して判定する。
    /// </summary>
    /// <param name="trap">判定するワールド座標</param>
    /// <returns>設置可能エリア内なら true</returns>
    private bool IsInArea(Vector3 trap) => IsInPutArea(putAreaLeftTop.position, putAreaRightDown.position, trap);

    /// <summary>
    /// マップ上に Trap を設置しようとしているか判定
    /// </summary>
    private bool IsOnMap()
    {
        int mask = (1 << UseLayerName.platformLayer) | (1 << UseLayerName.trapLayer);

        return Physics2D.OverlapPoint(mouseWorldPos, mask) != null;
    }

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
            
            bool canPlacePreview = IsInArea(mPos) && !IsOnMap();
            
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

        bool canPlace = IsInArea(targetTrap.transform.position) && !IsOnMap();
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
            AddToRoundTraps(trap);
            TargetTrap(trap.trapName).Reduce();

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
        //if (!CanUseTrap(trapName)) return;

        // 現在の Trap 設置処理をキャンセル
        Reject();
        // Trap 設置 Coroutine を開始
        createTrap = StartCoroutine(PutTrap(trapName));
    }



    // 同時に設置できる Trap の最大数
    private const int theMaxNumberTrapCanPut = 10;
    // 現在のラウンド番号
    public int nowRound = 1;
    // 設置された Trap をラウンド情報付きで管理する Queue
    private List<RoundTrap> roundTraps = new List<RoundTrap>();
    /// <summary>
    /// Trap をラウンド管理キューに追加する。
    /// 
    /// Trap を設置すると現在のラウンド番号と一緒に記録される。
    /// 最大数を超えた場合、最も古い Trap を削除する。
    /// </summary>
    /// <param name="targetTrap">追加する Trap</param>
    public void AddToRoundTraps(Trap targetTrap)
    {
        // 現在ラウンドと一緒に Trap を登録
        roundTraps.Add(new RoundTrap(targetTrap, nowRound));
        // 最大数を超えた場合、最も古い Trap を削除
        if (roundTraps.Count > theMaxNumberTrapCanPut) DestroyTrap(roundTraps[0].trap);
    }


    private void ResetRoundTraps()
    {
        foreach (RoundTrap roundTrap in roundTraps)
        {
            if (roundTrap.trap != null) Destroy(roundTrap.trap.gameObject);
        }
        roundTraps.Clear();
    }

    public void ResetRound()
    {
        // 次のラウンドに残す Trap を保存する Queue
        Queue<RoundTrap> stayRoundTraps = new Queue<RoundTrap>();

        for (int i = 0; i < roundTraps.Count; i++)
        {
            if (roundTraps[i].round == nowRound && roundTraps[i].trap != null)
            {
                Destroy(roundTraps[i].trap.gameObject);
            }
            else stayRoundTraps.Enqueue(roundTraps[i]);
        }

        roundTraps.Clear();
        while (stayRoundTraps.Count > 0)
        {
            roundTraps.Add(stayRoundTraps.Dequeue());
        }


    }
    /// <summary>
    /// 次のラウンドへ進む。
    /// </summary>
    public void NextTurn() => nowRound++;

    public void DestroyTrap(Trap targetTrap)
    {
        RoundTrap roundTrap = roundTraps.Find(rt => rt.trap == targetTrap);
        if (roundTrap == null) return;
        Destroy(roundTrap.trap.gameObject);
        TargetTrap(roundTrap.trap.trapName).Recovery();
        roundTraps.Remove(roundTrap);
    }

    public bool test = false;
    private void Update()
    {
        if (test)
        {
            Debug.Log(roundTraps.Count);
            foreach (RoundTrap t in roundTraps)
            {
                Debug.Log($"{t.trap.gameObject.name} , {t.round}");
            }
            test = false;
        }


    }


}
