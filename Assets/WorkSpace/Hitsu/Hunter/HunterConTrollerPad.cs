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
    //private Dictionary<TrapName, TrapData> trapDictionary;
    /// <summary>
    /// TrapName から Trap Prefab を取得する
    /// </summary>
    private GameObject TarpObject(TrapName trapName) => CanUseTrap.allTrap[trapName].prefab;
    /// <summary>
    /// TrapName から Trap Sprite を取得する
    /// </summary>
    private Sprite TrapSprite(TrapName trapName) => CanUseTrap.allTrap[trapName].icon;
    // Trap UI ボタン
    public List<Button> trapButtonList;

    /// <summary>
    /// 使用可能な Trap を UI ボタンに設定する
    /// </summary>
    public void CanUseTrapInit(List<TrapName> targetTrap)
    {
        targetTrap.Distinct().ToList();
        int index;
        int trapTypeMax = targetTrap.Count > trapButtonList.Count ? trapButtonList.Count : targetTrap.Count;

        for (index = 0; index < trapButtonList.Count; index++)
        {
            trapButtonList[index].gameObject.SetActive(false);
            trapButtonList[index].onClick.RemoveAllListeners();

            if (index < trapTypeMax)
            {
                switch (targetTrap[index])
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

                trapButtonList[index].image.sprite = TrapSprite(targetTrap[index]);
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
    public void HunterConTrollerPad_Init(Player targetPlyer)
    {
    }

    public void HunterSwitch(Player targetPlayer)
    {
        //GameManager.Instance.TargetGamepad(targetPlayer);
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
