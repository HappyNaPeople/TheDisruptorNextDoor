using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections.Generic;

public class PlayerDisplayAssigner : MonoBehaviour
{
    public static List<PlayerDisplayAssigner> instances = new();

    public CinemachineCamera vCam;
    public Canvas canvas;
    public PlayerInput playerInput;
    public EventSystem eventSystem;

    void Start()
    {
        instances.Add(this);

        // プレイヤーのインデックス（0 or 1）を取得
        int index = playerInput.playerIndex + 1;
        gameObject.name = $"Player_{index}";
        transform.position = (index == 1) ? new Vector3(-10, 0) : new Vector3(10, 0);

        string cameraName = $"MainCamera_P{index}";
        GameObject cameraObj = GameObject.Find(cameraName);
        Canvas defaultCanvas = cameraObj.GetComponentInChildren<Canvas>();
        if(defaultCanvas != null)
        {
            defaultCanvas.gameObject.SetActive(false);
        }

        if (cameraObj != null)
        {
            Camera physicalCamera = cameraObj.GetComponent<Camera>();

            // 3. Cinemachineのチャンネルを合わせる
            // これで、このVCamは対応するBrainにだけ映像を送るようになる
            if (vCam != null)
            {
                vCam.OutputChannel = (index == 1) ? OutputChannels.Channel01 : OutputChannels.Channel02;

                // VCamの追従対象を自分自身（プレハブのルート）に設定
                vCam.Follow = this.transform;
            }

            // 4. Canvasにカメラを割り当てる
            if (canvas != null && physicalCamera != null)
            {
                canvas.worldCamera = physicalCamera;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
            }

            Debug.Log($"{gameObject.name} linked to {cameraName}");
        }
        else
        {
            Debug.LogError($"{cameraName} がシーン内に見つかりません！");
        }

        
        if(index == 2)
        {

        }
    }
}