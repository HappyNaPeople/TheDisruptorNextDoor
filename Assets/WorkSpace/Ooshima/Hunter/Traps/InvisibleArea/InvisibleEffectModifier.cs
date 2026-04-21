using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InvisibleEffectModifier : MonoBehaviour
{
    private class SpriteState
    {
        public SpriteRenderer renderer;
        public int originalLayer;
        public Color originalColor;
    }

    private List<SpriteState> spriteStates = new List<SpriteState>();
    private Runner runner;

    private Coroutine durationCoroutine;

    private RunnerEffectController effectController;
    private bool isInArea = false;

    private void Awake()
    {
        runner = GetComponent<Runner>();
        effectController = GetComponentInChildren<RunnerEffectController>();
    }

    public void EnterArea()
    {
        isInArea = true;
        
        if (effectController != null)
        {
            effectController.forceShowFootstep = true;
        }

        // もしカウントダウン中なら止める
        if (durationCoroutine != null)
        {
            StopCoroutine(durationCoroutine);
            durationCoroutine = null;
        }

        // 初回の場合に元の状態を保存して不可視化
        if (spriteStates.Count == 0)
        {
            ApplyInvisible();
        }
    }

    public void ExitArea(float exitDuration = 5f)
    {
        isInArea = false;
        
        // 5秒の持続時間開始
        if (durationCoroutine != null)
        {
            StopCoroutine(durationCoroutine);
        }
        durationCoroutine = StartCoroutine(CountDownAndRevert(exitDuration));
    }

    private void ApplyInvisible()
    {
        spriteStates.Clear();
        // Runnerとその子要素すべてのSpriteRendererを取得
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (var sr in renderers)
        {
            // 元の情報を保存
            SpriteState state = new SpriteState();
            state.renderer = sr;
            state.originalLayer = sr.gameObject.layer;
            state.originalColor = sr.color;
            spriteStates.Add(state);

            // 「1P(Runner)カメには見えず、2P(Hunter)カメには見える」レイヤーに設定
            sr.gameObject.layer = UseLayerName.runnerCantSeeLayer;
            
            // Hunterからは「うっすら」見えるようにする
            Color newColor = sr.color;
            newColor.a = 0.3f;
            sr.color = newColor;
        }
    }

    private void RevertInvisible()
    {
        foreach (var state in spriteStates)
        {
            if (state.renderer != null)
            {
                state.renderer.gameObject.layer = state.originalLayer;
                state.renderer.color = state.originalColor;
            }
        }
        spriteStates.Clear();
        
        if (effectController != null)
        {
            effectController.forceShowFootstep = false;
        }
    }

    private IEnumerator CountDownAndRevert(float duration)
    {
        float t = duration;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            yield return null;
        }

        RevertInvisible();
        // 用済みになったら自分自身を削除
        Destroy(this);
    }

    private void OnDestroy()
    {
        // 途中で削除された場合は安全のため元に戻す
        RevertInvisible();
    }
}
