using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;

// Webカメラ
public class WebCam : MonoBehaviour
{
    private static int INPUT_SIZE = 256;
    private static int FPS = 30;

    // UI
    RawImage rawImage;
    WebCamTexture webCamTexture;

    // スタート時に呼ばれる
    void Start()
    {
        CameraInit();
    }

    public void CameraInit()
    {
        // 接続されているウェブカメラのデバイス一覧を取得
        WebCamDevice[] devices = WebCamTexture.devices;

        // デバイスが1つも見つからない場合の処理
        if (devices.Length == 0)
        {
            Debug.Log("Webカメラが見つかりません");
            return;
        }

        // Webカメラの開始
        this.rawImage = GetComponent<RawImage>();
        rawImage.color = Color.white;

        // 最初のカメラデバイス(devices[0])を使用するように指定
        this.webCamTexture = new WebCamTexture(devices[0].name, INPUT_SIZE, INPUT_SIZE, FPS);
        this.rawImage.texture = this.webCamTexture;
        this.webCamTexture.Play();
    }
}