using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RoundTrap
{
    public Trap trap {  get; private set; }
    public int round {  get; private set; }

    public RoundTrap(Trap trap, int round)
    {
        this.trap = trap;
        this.round = round;
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
    public void GamePadInit(Gamepad targetGamePad)=> hunterUseController = targetGamePad;

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

    /// <summary>
    /// 使用可能な Trap を UI ボタンに設定する
    /// </summary>
    /// <param name="targetTrap">使用可能な TrapName のリスト</param>
    private void CanUseTrapInit(List<TrapName> targetTrap)
    {
        // 重複する Trap を除外
        targetTrap.Distinct().ToList();
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
                TrapName trap = targetTrap[index];
                trapButtonList[index].onClick.AddListener(() => CreateTrap(trap));

                // Trap アイコンを設定
                trapButtonList[index].image.sprite = TrapSprite(targetTrap[index]);
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
    public bool isGrid = false;
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
            // スクリーン座標 → ワールド座標変換
            Vector3 world = hunterCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));

            // グリッドスナップ
            // Ooshima: StageGridManager が存在する場合は、isGrid が false でも強制的にスナップする機能を追加
            if (isGrid || StageGridManager.Instance != null)
            {
                // Ooshima: StageGridManager のグリッドに合わせてスナップするように変更
                if (StageGridManager.Instance != null)
                {
                    Vector2Int gridPos = StageGridManager.Instance.WorldToGrid(world);
                    world = StageGridManager.Instance.GridToWorld(gridPos);
                }
                else
                {
                    // ワールド座標をグリッドサイズに合わせて丸める
                    world.x = Mathf.Round(world.x / gridSize) * gridSize;
                    world.y = Mathf.Round(world.y / gridSize) * gridSize;
                    // 2DゲームのためZ座標は0に固定
                    world.z = 0;
                }
            }

            // 計算したワールド座標を返す
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
        if(trap.x > rightDown.x||
            trap.x < leftTop.x||
            trap.y < rightDown.y||
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
        int mask = UseLayerName.platformLayer | UseLayerName.trapLayer;

        return Physics2D.OverlapPoint(mouseWorldPos, mask) != null;
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

        // Ooshima: 設置可能かどうかで色を変えるための準備（子要素の SpriteRenderer を全て取得）
        SpriteRenderer[] renderers = targetTrap.GetComponentsInChildren<SpriteRenderer>();
        Color normalColor = Color.white;
        Color errorColor = new Color(1f, 0.5f, 0.5f, 0.8f); // 設置不可時の赤みのかかった半透明色

        // クリックされるまでマウス追従
        while (!GameManager.inputDevice.mouse.leftButton.isPressed)
        {
            targetTrap.transform.position = mouseWorldPos;

            // Ooshima: リアルタイムで配置可能かチェックし、色を切り替える
            bool canPlaceNow = false;
            if (StageGridManager.Instance != null)
            {
                canPlaceNow = StageGridManager.Instance.CanPlaceTrapDataDriven(targetTrap.transform.position);
            }
            else
            {
                canPlaceNow = IsInArea(targetTrap.transform.position) && !IsOnMap();
            }

            foreach (var sr in renderers)
            {
                sr.color = canPlaceNow ? normalColor : errorColor;
            }

            yield return null;
        }

        // Ooshima: 色を元に戻す
        foreach (var sr in renderers)
        {
            sr.color = normalColor;
        }

        // 設置可能エリアなら Trap 有効化
        // Ooshima: StageGridManager を使用した配置判定に変更
        bool canPlace = false;
        if (StageGridManager.Instance != null)
        {
            canPlace = StageGridManager.Instance.CanPlaceTrapDataDriven(targetTrap.transform.position);
        }
        else
        {
            canPlace = IsInArea(targetTrap.transform.position) && !IsOnMap();
        }

        if (canPlace)
        {
            Trap trap = targetTrap.GetComponent<Trap>();
            trap.Init();
            trap.SetUp();
            AddToRoundTraps(trap);
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
        foreach(RoundTrap roundTrap in roundTraps)
        {
            if(roundTrap.trap!=null) 
            {
                Destroy(roundTrap.trap.gameObject);
            }
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
