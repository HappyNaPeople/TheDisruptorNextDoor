using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HunterConTrollerPad : MonoBehaviour
{
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
    public Camera hunterCamera;
    public Canvas hunterCanvas;

    [Header("InPut")]
    public HunterCursor hunterCursor;
    public PlayerInputData inputData;

    [Header("Time And ProgressBar")]
    public CommonUI commonUI;

    #region Cost
    [Header("Cost")]
    public Image costImage;
    public SpriteRenderer[] costSpriteRenderers;
    //public TMP_Text costText;
    public int nowCostCanUse = 0;

    private int maxCostCanUse => 20 + (5 * InGame.Instance.passCheckPoint);
    private const int startCostCanUse = 5;
    private int costRecovery => 1 + InGame.Instance.passCheckPoint;
    private Coroutine costRecover;
    private const float recoverCountDown = 3;
    private float recoverTimer = 0;
    //private string NowCost() => Convert.ToInt32(nowCostCanUse).ToString("D2");

    private void CostUIUpdate()
    {

        int tens = (nowCostCanUse / 10) % 10;
        int ones = nowCostCanUse % 10;

        var numbers = GameManager.Instance.numberSprites;

        if (numbers == null || numbers.Count < 10)
        {
            Debug.LogError("numberSprites not set correctly");
            return;
        }

        if (costSpriteRenderers == null || costSpriteRenderers.Length < 2)
        {
            Debug.LogError("costSpriteRenderers not enough");
            return;
        }
        costSpriteRenderers[0].sprite = GameManager.Instance.numberSprites[tens];
        costSpriteRenderers[1].sprite = GameManager.Instance.numberSprites[ones];
    }

    private IEnumerator CostRecover()
    {
        while (InGame.Instance.gameStage != GameStage.Playing)
        {
            yield return null;
        }
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

    public TMP_Text trap_Introduce;



    #endregion

    #region Put Trap

    Vector3 cursorPos
    {
        get
        {
            Vector3 world = hunterCursor.worldPos;
            Vector2Int grid = StageGridManager.Instance.WorldToGrid(world);
            //// グリッド → ワールド（スナップ後の正しい位置）
            world = StageGridManager.Instance.GridToWorld(grid);
            //Vector3 mouse = Mouse.current.position.ReadValue();
            //Vector3 world = Camera.main.ScreenToWorldPoint(mouse);
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

    private void TrapIntroduce(TrapName trap)
    {
        string trapName = $"Trap Name : {trap.ToString()}\n";
        string cost = $"Need Cost : {GameManager.allTrap[trap].cost}\n";
        string introduce = $"Introduce : {GameManager.allTrap[trap].information}";


        trap_Introduce.text = trapName + cost + introduce;

    }

    // プレビュー中の Trap
    private GameObject choseTrap;
    // Trap 設置 Coroutine
    private Coroutine createTrap;

    //private IEnumerator PutTrap(TrapName trapName)
    //{
    //    // Prefab が存在しない場合は終了
    //    if (TarpObject(trapName) == null)
    //    {
    //        Debug.Log("No Trap");
    //        yield break;
    //    }
    //    // Trap を生成
    //    GameObject targetTrap = Instantiate(TarpObject(trapName), cursorPos, TarpObject(trapName).transform.rotation);
    //    // TrapPlacer を取得またはアタッチ
    //    TrapPlacer placer = targetTrap.GetComponent<TrapPlacer>();
    //    TrapIntroduce(trapName);

    //    if (placer == null)
    //    {
    //        // if (trapName == TrapName.BlackHole)
    //        //     placer = targetTrap.AddComponent<WorldTrapPlacer>();
    //        //else
    //        if (trapName == TrapName.Spikes || trapName == TrapName.IceArea || trapName == TrapName.StickyArea)
    //            placer = targetTrap.AddComponent<WallTrapPlacer>();
    //        else
    //            placer = targetTrap.AddComponent<StandardTrapPlacer>();
    //    }

    //    placer.InitializePreview();
    //    choseTrap = targetTrap;

    //    // クリックされるまでマウス追従
    //    while (!inputData.isPutPressed)
    //    {
    //        placer.UpdatePreviewPosition(cursorPos);
    //        bool canPlacePreview = placer.ValidatePlacement();
    //        placer.UpdatePreviewColor(canPlacePreview);

    //        yield return null;
    //    }

    //    bool canPlace = placer.ValidatePlacement();
    //    if (canPlace)
    //    {
    //        placer.RestoreVisuals();
    //        Trap trap = targetTrap.GetComponent<Trap>();
    //        trap.Init();
    //        trap.SetUp();
    //        UseCost(trap.trapName);
    //        Destroy(placer); // 設置後は不要なので削除
    //        createTrap = null;
    //        choseTrap = null;
    //    }

    //}

    private IEnumerator PuttingTrap(TrapName trapName)
    {
        if (trapRings == null)
        {
            Debug.LogError("TrapRing is Null");
            yield break;
        }
        TrapPlacer placer = null;
        Trap trap = null;
        while (true)
        {
            if (trapName == TrapName.None|| TarpObject(trapName) == null) yield break;

            choseTrap = Instantiate(TarpObject(trapName), cursorPos, TarpObject(trapName).transform.rotation);
            placer = choseTrap.GetComponent<TrapPlacer>();

            TrapIntroduce(trapName);

            if (placer == null)
            {
                // if (trapName == TrapName.BlackHole)
                //     placer = targetTrap.AddComponent<WorldTrapPlacer>();
                //else
                if (trapName == TrapName.Spikes || trapName == TrapName.IceArea || trapName == TrapName.StickyArea)
                    placer = choseTrap.AddComponent<WallTrapPlacer>();
                else
                    placer = choseTrap.AddComponent<StandardTrapPlacer>();
            }
            placer.InitializePreview();
            //choseTrap = targetTrap;

            bool canPlace = false;

            while (true)
            {
                placer.UpdatePreviewPosition(cursorPos);
                bool canPlacePreview = placer.ValidatePlacement() && nowCostCanUse >= GameManager.allTrap[trapName].cost;

                placer.UpdatePreviewColor(canPlacePreview);
                canPlace = canPlacePreview;

                if (inputData.isPutPressed && canPlace) 
                {
                    break;
                }
                yield return null;
            }

            placer.RestoreVisuals();

            trap = choseTrap.GetComponent<Trap>();
            trap.Init();
            trap.SetUp();

            UseCost(trap.trapName);
            Destroy(placer); // 設置後は不要なので削除
            createTrap = null;
            choseTrap = null;

            yield return null;
        }
    }

    private void Reject()
    {
        if (createTrap != null)
        {
            StopCoroutine(createTrap);
            createTrap = null;
        }

        if (choseTrap != null)
        {
            Destroy(choseTrap);
            choseTrap = null;
        }


    }

    public void CreateTrap(TrapName trapName)
    {
        // 現在の Trap 設置処理をキャンセル
        Reject();
        // Trap 設置 Coroutine を開始
        createTrap = StartCoroutine(PuttingTrap(trapName));
    }

    #endregion

    public void HunterSwitch(Player targetPlayer)
    {
        CanUseTrapInit(targetPlayer.hunter.backpack.trapsPack);
        trap_Introduce.text = "";
        RecoveryInit();
        TrapRingsUpdate();
        //hunterCursor.Init(this);
        commonUI.CommonUIInit();
    }

    public void HunterInit()
    {
        hunterCursor.Init(this);
    }

    public bool test;
    private void Update()
    {
        //if (inputData != null && InGame.Instance.gameStage == GameStage.Playing) 
        //{
        //    trapRings.ControllerChoose(inputData.chooseInput);
        //}

        trapRings.ControllerChoose(inputData.chooseInput);

        //if (test)
        //{
        //    test = false;
        //    UseCost(TrapName.JumpPad);
        //}

        if (test)
        {
            CreateTrap(TrapName.FallRock);
            test = false;
        }
    }


}


