using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HunterCanvas : MonoBehaviour
{
    public static HunterCanvas Instance;
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
    public Camera hunterCamera;
    public Canvas canvas;

    [Header("InPut")]
    public HunterCursor hunterCursor;
    public PlayerInputData inputData;

    [Header("Time And ProgressBar")]
    public TimeAndProgressBar timeAndProgressBar;

    #region Cost
    [Header("Cost")]
    public Image costImage;
    public SpriteRenderer[] costSpriteRenderers;
    public TMP_Text costText;
    public int nowCostCanUse = 0;

    private int maxCostCanUse => 20 + (5 * 1/*InGame.Instance.passCheckPoint*/);
    private const int startCostCanUse = 5;
    private int costRecovery => 1 + 0/*InGame.Instance.passCheckPoint*/;
    private Coroutine costRecover;
    private const float recoverCountDown = 3;
    private float recoverTimer = 0;
    //private string NowCost() => Convert.ToInt32(nowCostCanUse).ToString("D2");

    private void CostUIUpdate()
    {
        int tens = (nowCostCanUse / 10) % 10;
        int ones = nowCostCanUse % 10;

        costSpriteRenderers[0].sprite = GameManager.Instance.numberSprites[tens];
        costSpriteRenderers[1].sprite = GameManager.Instance.numberSprites[ones];
    }

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
                //costText.text = NowCost();
                CostUIUpdate();

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
                    CostUIUpdate();
                    //costText.text = NowCost();


                    recoverTimer -= recoverCountDown;
                    TrapRingsUpdate();
                }

                yield return null;
            }



        }

    }

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
        //costText.text = NowCost();
        CostUIUpdate();
        // 回復タイマーをリセット（最初からカウント開始）
        recoverTimer = 0;

        // 回復処理開始
        costRecover = StartCoroutine(CostRecover());


    }


    #endregion

    #region Trap
    [Header("Trap")]
    public TrapRing trapRings;
    private GameObject TarpObject(TrapName trapName) => GameManager.allTrap[trapName].prefab;
    private Sprite TrapSprite(TrapName trapName) => GameManager.allTrap[trapName].icon;
    private int TrapCost(TrapName trapName) => GameManager.allTrap[trapName].cost;
    private void CanUseTrapInit(List<TrapName> useTrapName) => trapRings.Init(useTrapName);
    private void TrapRingsUpdate() => trapRings.UIUpdate();


    #endregion

    #region Put Trap

    Vector3 cursorPos
    {
        get
        {
            //Vector3 world = hunterCursor.worldPos;
            //Vector2Int grid = StageGridManager.Instance.WorldToGrid(world);
            //// グリッド → ワールド（スナップ後の正しい位置）
            //world = StageGridManager.Instance.GridToWorld(grid);
            Vector3 mouse = Mouse.current.position.ReadValue();
            Vector3 world = Camera.main.ScreenToWorldPoint(mouse);
            world.z = 0;

            return world;
        }
    }

    private void UseCost(TrapName trap)
    {
        // コスト減少
        nowCostCanUse -= TrapCost(trap);

        // UI更新
        //costText.text = NowCost();
        CostUIUpdate();

        TrapRingsUpdate();
    }

    // プレビュー中の Trap
    private GameObject choseTrap;
    // Trap 設置 Coroutine
    private Coroutine createTrap;

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
            // if (trapName == TrapName.BlackHole)
            //     placer = targetTrap.AddComponent<WorldTrapPlacer>();
            //else
            if (trapName == TrapName.Spikes || trapName == TrapName.IceArea || trapName == TrapName.StickyArea)
                placer = targetTrap.AddComponent<WallTrapPlacer>();
            else
                placer = targetTrap.AddComponent<StandardTrapPlacer>();
        }

        placer.InitializePreview();
        choseTrap = targetTrap;

        // クリックされるまでマウス追従
        while (!Mouse.current.rightButton.isPressed)
        {
            placer.UpdatePreviewPosition(cursorPos);
            bool canPlacePreview = placer.ValidatePlacement();
            placer.UpdatePreviewColor(canPlacePreview);

            yield return null;
        }
        bool canPlace = placer.ValidatePlacement();
        if (canPlace)
        {
            placer.RestoreVisuals();
            Trap trap = targetTrap.GetComponent<Trap>();
            trap.Init();
            trap.SetUp();
            UseCost(trap.trapName);
            Destroy(placer); // 設置後は不要なので削除
            createTrap = null;
            choseTrap = null;
        }

    }

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

    public void CreateTrap(TrapName trapName)
    {
        // 現在の Trap 設置処理をキャンセル
        Reject();
        // Trap 設置 Coroutine を開始
        createTrap = StartCoroutine(PutTrap(trapName));
    }

    #endregion

    public void HunterSwitch(Player targetPlayer)
    {
        CanUseTrapInit(targetPlayer.hunter.backpack.trapsPack);
        RecoveryInit();
        TrapRingsUpdate();
        //hunterCursor.Init(this);
        //timeAndProgressBar.ProgressBarInit();
    }

    public void HunterInit()
    {
        List<TrapName> choseTraps = new List<TrapName>() { TrapName.Spikes, TrapName.FallRock, TrapName.Boom, TrapName.JumpPad, TrapName.Spikes };
        CanUseTrapInit(choseTraps);
        RecoveryInit();
        TrapRingsUpdate();
        //timeAndProgressBar.ProgressBarInit();
    }

    private void Start()
    {
        HunterInit();
    }



    public bool test;
    private void Update()
    {
        if (Gamepad.current != null)
        {
            trapRings.ControllerChoose(Gamepad.current.rightStick.value);
        }

        if (test)
        {
            test = false;
            UseCost(TrapName.JumpPad);
        }

    }

}
