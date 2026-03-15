using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionMirror : MonoBehaviour
{
    [Header("監視するEventSystem")]
    public EventSystem targetEventSystem;

    [Header("自分自身のRectTransform")]
    private RectTransform myRect;

    void Start()
    {
        myRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (targetEventSystem == null) return;

        // 指定したEventSystemがいま選択しているGameObjectを取得
        GameObject selected = targetEventSystem.currentSelectedGameObject;

        if (selected != null)
        {
            // 選択中のボタンのRectTransformを取得
            RectTransform targetRect = selected.GetComponent<RectTransform>();

            if (targetRect != null)
            {
                // ボタンと同じ位置・サイズに自分を合わせる
                myRect.position = targetRect.position;
                myRect.sizeDelta = targetRect.sizeDelta;
            }
        }
    }
}