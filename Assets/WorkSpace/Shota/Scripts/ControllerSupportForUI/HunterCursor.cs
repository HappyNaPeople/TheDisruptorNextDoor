using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public class HunterCursor : MonoBehaviour
{
    public float cursorSpeed = 10f;

    private HunterConTrollerPad hunterCon;
    public string scheme { get
        {
            if (hunterCon.inputData == null) return "";

            return hunterCon.inputData.playerInput.currentControlScheme;
        }
    }
    public bool isUsingController { get => scheme == "Gamepad"; }

    public Transform cursor;
    public Vector2 localPos = Vector2.zero;
    public Vector2 worldPos { get => cursor.position; }

    float camHeight;
    float camWidth;

    public void Init(HunterConTrollerPad con)
    {
        hunterCon = con;
        
        Camera cam = hunterCon.hunterCamera;

        camHeight = cam.orthographicSize * 2f;
        camWidth = camHeight * cam.aspect;

        Debug.Log("Cursor Init");
    }

    void Update()
    {
        MoveCursor(Time.deltaTime);
    }

    void MoveCursor(float dt)
    {
        var newPos = worldPos;
        if (isUsingController)
        {
            var deltaPos = cursorSpeed * hunterCon.inputData.moveInput * dt;
            localPos += deltaPos;
            localPos = ClampCursor(localPos);

            newPos = (Vector3)localPos + hunterCon.hunterCamera.transform.position;
        }
        else
        {
            // マウスのスクリーン座標取得
            Vector2 mousePos = GameManager.inputDevice.mouse.position.ReadValue();

            // カメラとの距離を考慮してワールド座標へ変換
            float distance = Mathf.Abs(hunterCon.hunterCamera.transform.position.z);
            newPos = hunterCon.hunterCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distance));
        }

        cursor.position = newPos;
    }

    Vector2 ClampCursor(Vector2 pos)
    {
        float left = -camWidth / 2f;
        float right = camWidth / 2f;
        float bottom = -camHeight / 2f;
        float top = camHeight / 2f;

        pos.x = Mathf.Clamp(pos.x, left, right);
        pos.y = Mathf.Clamp(pos.y, bottom, top);

        return pos;
    }
}
