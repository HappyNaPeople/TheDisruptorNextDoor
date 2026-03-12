using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.LightTransport;


/// <summary>
/// Trap の表示情報と生成用 Prefab をまとめたデータクラス。
/// Trap UI で使用する Sprite と、
/// 実際に生成する Trap オブジェクトを保持する。
/// </summary>
class TrapData
{
    // Trap ボタンに表示する Sprite
    public Sprite trapSprite;

    // 生成する Trap Prefab
    public GameObject trapObject;
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
    // Hunter 用カメラ
    public Camera hunterCamera;
    // 使用する Gamepad
    private Gamepad hunterUseController;
    /// <summary>
    /// Hunter が使用する Gamepad を設定する
    /// </summary>
    /// <param name="targetGamePad"></param>
    public void GamePadInit(Gamepad targetGamePad)=> hunterUseController = targetGamePad;
    // Trap 設置可能エリア
    public Transform putAreaLeftTop;
    public Transform putAreaRightDown;
    // TrapName から TrapData を取得する Dictionary
    private Dictionary<TrapName, TrapData> trapDictionary;
    /// <summary>
    /// TrapName から Trap Prefab を取得する
    /// </summary>
    private GameObject TarpObject(TrapName trapName) => trapDictionary[trapName].trapObject;
    /// <summary>
    /// TrapName から Trap Sprite を取得する
    /// </summary>
    private Sprite TrapSprite(TrapName useTrap) => trapDictionary[useTrap].trapSprite;
    // Trap UI ボタン
    public List<Button> trapButtonList;
    /// <summary>
    /// Trap データを初期化する
    /// Sprite と Prefab を Resources から読み込む
    /// </summary>
    private void TrapDataListInit()
    {
        trapDictionary = new Dictionary<TrapName, TrapData>();
        trapDictionary[TrapName.Spikes]=new TrapData(){trapSprite = Resources.Load<Sprite>("Texture/Traps/Spike"),trapObject = Resources.Load<GameObject>("Prefabs/Traps/Spikes")};
        trapDictionary[TrapName.FallRock] = new TrapData() { trapSprite = Resources.Load<Sprite>("Texture/Traps/FallRock"), trapObject = Resources.Load<GameObject>("Prefabs/Traps/FallRock") };
        trapDictionary[TrapName.Boom] = new TrapData() { trapSprite = Resources.Load<Sprite>("Texture/Traps/Boom"), trapObject = Resources.Load<GameObject>("Prefabs/Traps/Boom") };
        trapDictionary[TrapName.JumpPad] = new TrapData() { trapSprite = Resources.Load<Sprite>("Texture/Traps/JumpPad"), trapObject = Resources.Load<GameObject>("Prefabs/Traps/JumpPad") };

    }

    /// <summary>
    /// 使用可能な Trap を UI ボタンに設定する
    /// </summary>
    public void CanUseTrapInit(List<Trap> targetTrap)
    {
        List<TrapName> useTrap = new List<TrapName>();
        foreach (Trap trap in targetTrap)
        {
            useTrap.Add(trap.trapName);
        }
        useTrap.Distinct().ToList();
        int index;
        int trapTypeMax = useTrap.Count > trapButtonList.Count ? trapButtonList.Count : useTrap.Count;

        for (index = 0; index < trapButtonList.Count; index++)
        {
            trapButtonList[index].gameObject.SetActive(false);
            trapButtonList[index].onClick.RemoveAllListeners();

            if (index < trapTypeMax)
            {
                switch (useTrap[index])
                {
                    case TrapName.Spikes:
                        trapButtonList[index].onClick.AddListener(Button_CreateSpikes);
                        break;

                    case TrapName.FallRock:
                        trapButtonList[index].onClick.AddListener(Button_FallRock);
                        break;

                    case TrapName.Boom:
                        trapButtonList[index].onClick.AddListener(Button_Boom);
                        break;
                }

                trapButtonList[index].image.sprite = TrapSprite(useTrap[index]);
                trapButtonList[index].gameObject.SetActive(true);

            }
            else
            {
                trapButtonList[index].gameObject.SetActive(false);
            }
        }


    }

    /// <summary>
    /// 使用可能な Trap を UI ボタンに設定する
    /// </summary>
    public void TestCanUseTrapInit(List<TrapName> targetTrap)
    {
        List<TrapName> useTrap = targetTrap.Distinct().ToList();
        int index;
        int trapTypeMax = useTrap.Count > trapButtonList.Count ? trapButtonList.Count : useTrap.Count;

        for (index = 0; index < trapButtonList.Count; index++)
        {
            trapButtonList[index].gameObject.SetActive(false);
            trapButtonList[index].onClick.RemoveAllListeners();

            if (index < trapTypeMax)
            {
                switch (useTrap[index])
                {
                    case TrapName.Spikes:
                        trapButtonList[index].onClick.AddListener(Button_CreateSpikes);
                        break;

                    case TrapName.FallRock:
                        trapButtonList[index].onClick.AddListener(Button_FallRock);
                        break;

                    case TrapName.Boom:
                        trapButtonList[index].onClick.AddListener(Button_Boom);
                        break;
                    case TrapName.JumpPad:
                        trapButtonList[index].onClick.AddListener(Button_JumpPad);
                        break;
                }

                trapButtonList[index].image.sprite = TrapSprite(useTrap[index]);
                trapButtonList[index].gameObject.SetActive(true);

            }
            else
            {
                trapButtonList[index].gameObject.SetActive(false);
            }
        }


    }

    /// <summary>
    /// HunterController の初期化処理
    /// 使用可能な Trap を設定する
    /// </summary>
    public void HunterConTrollerPad_init()
    {
        TrapDataListInit();

        List<TrapName> useTrap = new List<TrapName>();
        useTrap.Add(TrapName.FallRock);
        useTrap.Add(TrapName.Spikes);
        useTrap.Add(TrapName.Boom);
        useTrap.Add(TrapName.JumpPad);

        TestCanUseTrapInit(useTrap);
    }

    public void HunterSwitch(Player targetPlayer)
    {
        GameManager.Instance.TargetGamepad(targetPlayer.controllerCode);
        CanUseTrapInit(targetPlayer.hunter.backpack.trapsPack);

    }



    /// <summary>
    /// Singleton 初期化
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }
    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Start()
    {
        HunterConTrollerPad_init();
    }

    // グリッドスナップを使用するかどうか
    public bool isGrid = false;
    // グリッドの1マスのサイズ
    public float gridSize = 1f;

    /// <summary>
    /// マウス位置をワールド座標で取得する。
    /// isGrid が true の場合、グリッドにスナップした座標を返す。
    /// </summary>
    Vector3 mouseWorldPos
    {
        get
        {
            // マウスのスクリーン座標を取得（Input System）
            Vector2 mousePos = GameManager.inputDevice.mouse.position.ReadValue();
            // スクリーン座標 → ワールド座標に変換
            Vector3 world = hunterCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));

            // グリッドスナップが有効な場合
            if (isGrid)
            {
                // ワールド座標をグリッドサイズに合わせて丸める
                world.x = Mathf.Round(world.x / gridSize) * gridSize;
                world.y = Mathf.Round(world.y / gridSize) * gridSize;
                // 2DゲームのためZ座標は0に固定
                world.z = 0;
            }

            // 計算したワールド座標を返す
            return world;
        }
    }

    /// <summary>
    /// Trap 設置エリア内かどうか判定する
    /// </summary>
    private bool IsInPutArea(Vector3 trap)
    {
        if(trap.x > putAreaRightDown.position.x||
            trap.x < putAreaLeftTop.position.x||
            trap.y < putAreaRightDown.position.y||
            trap.y > putAreaLeftTop.position.y) return false;

        return true;
    }
    /// <summary>
    /// マップ上に Trap を設置しようとしているか判定する
    /// </summary>
    private bool IsOnMap()
    {
        Collider2D col = Physics2D.OverlapPoint(mouseWorldPos, UseLayerName.platformLayer);
        return col != null;
    }
    // プレビュー中の Trap
    private GameObject choseTrap;
    // Trap 作成 Coroutine
    private Coroutine createTrap;
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
    /// Trap を設置する Coroutine
    /// マウスクリックまで Trap をプレビュー表示する
    /// </summary>
    private IEnumerator PutTrap(TrapName trapName)
    {
        if (TarpObject(trapName) == null)
        {
            Debug.Log("No Trap");
            yield break;
        }

        GameObject targetTrap = Instantiate(TarpObject(trapName), mouseWorldPos, TarpObject(trapName).transform.rotation);
        targetTrap.layer = UseLayerName.runnerCantSeeLayer;

        choseTrap = targetTrap;
        //while (!GameManager.inputDevice.mouse.leftButton.isPressed)
        //    yield return null;

        while (!GameManager.inputDevice.mouse.leftButton.isPressed)
        {
            targetTrap.transform.position = mouseWorldPos;
            yield return null;
        }

        if (IsInPutArea(targetTrap.transform.position) && !IsOnMap())
        {
            Trap trap = targetTrap.GetComponent<Trap>();
            trap.Init();
            trap.SetUp();

        }
        else
        {
            Destroy(targetTrap);
        }
        createTrap = null;
        choseTrap = null;
    }
    /// <summary>
    /// Spikes Trap を生成する
    /// </summary>
    public void Button_CreateSpikes()
    {
        Reject();
        createTrap = StartCoroutine(PutTrap(TrapName.Spikes));
    }
    /// <summary>
    /// FallRock Trap を生成する
    /// </summary>
    public void Button_FallRock()
    {
        Reject();
        createTrap = StartCoroutine(PutTrap(TrapName.FallRock));
    }
    /// <summary>
    /// Boom Trap を生成する
    /// </summary>
    public void Button_Boom()
    {
        Reject();
        createTrap = StartCoroutine(PutTrap(TrapName.Boom));
    }

    public void Button_JumpPad()
    {
        Reject();
        createTrap = StartCoroutine(PutTrap(TrapName.JumpPad));
    }




}
