using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommonUI : MonoBehaviour
{
    [Header("Common Object")]
    public Timer_UI timer;
    public Fillbar_UI fillbar;
    public WalkDistence_UI walkDistence;

    [Header("Path Animator")]
    public Animator[] checkPointAnimator;
    private List<Transform> _checkPoints => InGame.Instance.checkPoints;


    private Coroutine timerWatcher;
    private Coroutine fillbarWatcher;
    private Coroutine walkDistenceWatcher;
    private Coroutine checkPointWatcher;
    private IEnumerator TimerWatcher()
    {
        yield return new WaitUntil(() => InGame.Instance.NowTimeToInt() > 0);

        int timerTime = InGame.Instance.NowTimeToInt();

        while (true) 
        {
            if(timerTime > InGame.Instance.NowTimeToInt())
            {
                timerTime = InGame.Instance.NowTimeToInt();
                timer.SpriteChange(timerTime);
            }
            yield return null;
        }
    }
    private IEnumerator FillbarWatcher()
    {
        float fillup = 0;

        yield return new WaitUntil(() => InGame.Instance.startingPoint != null && InGame.Instance.goal != null);

        while (true)
        {
            if(fillup < InGame.Instance.percentOfPassedDistance)
            {
                fillup = InGame.Instance.percentOfPassedDistance;
                fillbar.Fill(fillup);
            }
            yield return null;

        }
    }
    private IEnumerator WalkDistenceWatcher()
    {
        int distence = 0;
        yield return new WaitUntil(() => InGame.Instance.startingPoint != null && InGame.Instance.goal != null);

        while (true)
        {
            if (distence < InGame.Instance.passedDistance)
            {
                distence = InGame.Instance.passedDistance;
                walkDistence.SpriteChange(distence);
            }
            yield return null;

        }
    }

    private IEnumerator CheckPointWatcher()
    {
        yield return null;

        // 初期状態：全てのチェックポイントを未達成（false）に設定
        for (int i = 0; i < _checkPoints.Count; i++)
        {
            checkPointAnimator[i].SetBool($"Through", false);
        }
        // 1フレーム待機（初期化反映）
        yield return null;

        // チェックポイントを順番に監視
        for (int i = 0; i < _checkPoints.Count; i++)
        {
            Transform point = _checkPoints[i];

            // 条件：
            // ・Dictionaryにキーが存在する
            // ・かつ、そのチェックポイントがtrue（到達済み）
            yield return new WaitUntil(() => InGame.Instance.checkPointsDict.ContainsKey(point) && InGame.Instance.checkPointsDict[point]);

            // 到達したチェックポイントのアニメーションをON
            checkPointAnimator[i].SetBool($"Through", true);

            // 次の処理へ（1フレーム待機）
            yield return null;
        }
    }



    public void CommonUIInit()
    {
        if(timer==null|| fillbar==null|| walkDistence == null)
        {
            Debug.Log("Part Lost");
        }

        fillbar.Init();


        StopAllCoroutines();
        timerWatcher = StartCoroutine(TimerWatcher());
        fillbarWatcher = StartCoroutine(FillbarWatcher());
        walkDistenceWatcher = StartCoroutine(WalkDistenceWatcher());
        checkPointWatcher = StartCoroutine(CheckPointWatcher());

    }


    private void Start()
    {
        //CommonUIInit();
    }

    public bool test = false;
    private void Update()
    {
        if (test)
        {
            test = false;
            checkPointAnimator[1].SetBool($"Through", true);



        }


    }



}
