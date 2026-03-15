using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayerDisplayAssigner : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] EventSystem eventSystem;
    void Start()
    {

        // プレイヤーのインデックス（0 or 1）を取得
        int index = playerInput.playerIndex;
        gameObject.name = $"Player_{index + 1}";

        if (cam != null)
        {
            // インデックスに応じてターゲットディスプレイを切り替える
            // index 0 -> Display 1, index 1 -> Display 2
            cam.targetDisplay = index;
            Debug.Log($"Player {index + 1} assigned to Display {index + 1}");
        }
    }
}