using System;
using System.Collections;
using System.IO;
using UnityEngine;
using static BgmData;


[System.Serializable]
public class BgmData
{
    /// <summary>
    /// ゲームの各シーン・状態に対応するBGMの種類
    /// </summary>
    public enum StageBgm
    {
        GameTitle, // タイトル画面
        InGame,    // ゲームプレイ中
        Release,   // 解放・クリア時
        Error      // エラー・異常時
    };

    /// <summary>
    /// 現在のBGMが対応するステージ
    /// </summary>
    public StageBgm stage;
    /// <summary>
    /// 再生するBGMのAudioClip
    /// </summary>
    public AudioClip clip;
    /// <summary>
    /// 音量（Inspector上で1～100の範囲で調整可能）
    /// </summary>
    [Range(1f, 100f)] public float volume;

}



/// <summary>
/// BGM（音楽）および SFX（効果音）の再生・停止・音量調整を一括管理するクラス。
/// ・BGM の StreamingAssets からの動的読み込み
/// ・シーンをまたいで破棄されないサウンドシステム
/// ・名前によるサウンド検索
/// ・効果音のワンショット再生
/// など、ゲーム全体の音響を統括するマネージャー。
/// </summary>
public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// AudioManager のシングルトンインスタンス。
    /// </summary>
    public static AudioManager Instance;

    /// <summary>
    /// BGM、SFX 用に登録されたサウンドデータの配列
    /// </summary>
    public BgmData[] stageBgmDates;

    /// <summary>
    /// BGM / SFX をそれぞれ再生するための AudioSource
    /// </summary>
    public AudioSource musicSource;
    public AudioSource[] trapSfxSources;

    /// <summary>
    /// 現在 BGM（musicSource）が再生中かどうかを返す
    /// </summary>
    public bool IsPlaying() {  return musicSource.isPlaying; }

    /// <summary>
    /// シングルトンをセットし、シーンを跨いで破棄されないよう設定する
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    //// ========================================================================
    ////  BGM のクリップ切り替え（StreamingAssets から動的ロードする）
    //// ========================================================================

    ///// <summary>
    ///// 指定した BGM 名に対応する Sound を探し、
    ///// StreamingAssets から BGM ファイル（.mp3）を読み込んで差し替える。
    ///// 
    ///// 例：
    /////   ChangeCrip("MainBGM", "Stage01");
    ///// → StreamingAssets/StageMusic/Stage01.mp3 をロードして MainBGM に設定する。
    ///// 
    ///// ※ BGM の動的切り替えを行いたい場合に使用する。
    ///// </summary>
    ///// <param name="name">musicSounds 配列内の Sound 名（BGM の識別名）</param>
    ///// <param name="cripName">読み込みたい BGM ファイル名（拡張子 .mp3 は不要）</param>
    //public void ChangeCrip(string name, string cripName)
    //{
    //    // musicSounds[] の中から該当の Sound を検索
    //    Sound s = Array.Find(musicSounds, x => x.name == name);

    //    // 指定名が存在しなかった場合
    //    if (s == null)
    //    {
    //        Debug.LogWarning($"[ChangeClip] Sound '{name}' 指定名が存在しなかった");
    //        return;
    //    }

    //    //// .mp3 の拡張子が付いていなければ自動的に付与する
    //    string path;
    //    if (!cripName.EndsWith(".mp3")) cripName += ".mp3";

    //    Debug.Log(cripName);
    //    // StreamingAssets/StageMusic/xxx.mp3 の絶対パスを生成
    //    path = Path.Combine(Application.streamingAssetsPath, "Bgm", cripName);

    //    // BGM の非同期読み込み処理を開始（コルーチン）
    //    StartCoroutine(LoadAudioAndSet(s, path));
    //}

    ///// <summary>
    ///// BGM を読み込んで Sound に設定するコルーチン。
    ///// Editor / Development Build の場合：StreamingAssets から mp3 を読み込む。
    ///// 実機（Android / iOS）の場合：Resources から AudioClip を読み込む。
    ///// 
    ///// 使用例：
    /////     ChangeCrip("MainBGM", "Stage01");
    ///// </summary>
    ///// <param name="s">読み込み後に clip を設定する Sound オブジェクト</param>
    ///// <param name="path">StreamingAssets 用のフルパス（実機では未使用）</param>
    //private IEnumerator LoadAudioAndSet(Sound s, string path)
    //{
    //    // 指定パスの mp3 を取得するための UnityWebRequest を作成
    //    UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG);
    //    // ダウンロードハンドラを Dispose しない（後で AudioClip を使用するため）
    //    req.disposeDownloadHandlerOnDispose = false;

    //    // 非同期で読み込み開始（ここで処理は一時停止）
    //    yield return req.SendWebRequest();

    //    // 読み込み失敗 → エラーを出して終了
    //    if (req.result != UnityWebRequest.Result.Success)
    //    {
    //        Debug.LogError($"[ChangeClip] 読み込み失敗 : {path}\n{req.error}");
    //        yield break;
    //    }

    //    // 読み込み成功 → AudioClip を取得
    //    AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
    //    // 拡張子を除いたファイル名を clip.name に設定
    //    clip.name = System.IO.Path.GetFileNameWithoutExtension(path);
    //    // Resources.UnloadUnusedAssets() で破棄されないよう保護
    //    clip.hideFlags = HideFlags.DontUnloadUnusedAsset;
    //    // 読み込んだクリップを Sound に設定
    //    s.clip = clip;
    //    Debug.Log($"[ChangeClip] (StreamingAssets) mp3 を読み込む成功　: {clip.name}");

    //}

    ///// <summary>
    ///// 指定した BGM 名に対応する AudioClip の長さ（秒）を返す。
    ///// ・Sound が見つからない場合
    ///// ・AudioClip が未設定の場合
    ///// は 0 を返す安全設計になっている。
    ///// </summary>
    ///// <param name="name">musicSounds 内で検索する BGM 名</param>
    ///// <returns>クリップの長さ（秒）。見つからない場合は 0。</returns>
    //public float CripLenght(string name)
    //{
    //    // musicSounds[] の中から指定名の Sound を検索
    //    Sound s = Array.Find(musicSounds, x => x.name == name);

    //    // Sound が存在しない、または clip が未設定なら 0 を返す
    //    // （ロード失敗や clip が破棄された場合にも安全）
    //    if (s == null || s.clip == null)
    //    {
    //        Debug.LogWarning("No Crip");
    //        return 0f;

    //    }
    //    musicLength = s.clip.length;
    //    // 正常 → AudioClip の長さを返す
    //    return musicLength;
    //}

    /// <summary>
    /// 指定されたステージに対応するBGMを再生する
    /// </summary>
    /// <param name="targetStage">再生したいステージBGM</param>
    public void PlayMusic(StageBgm targetStage)
    {
        // 対応するBGMデータを検索
        BgmData bgmData = Array.Find(stageBgmDates, x => x.stage == targetStage);
        // BGMデータまたはAudioSourceが無効な場合は終了
        Debug.Log("musicSource.clip.name");

        // Sound が見つからない場合
        if (bgmData == null || bgmData.clip == null)
        {
            Debug.LogWarning("BgmData が見つかりません");
            return;
        } else if (musicSource == null)
        {
            Debug.LogWarning("AudioSource が設定されていません");
            return;
        }
        // BGM設定

        musicSource.clip = bgmData.clip;          
        musicSource.volume = bgmData.volume / 100;  // 0～1に変換

        // BGM の再生開始
        musicSource.Play();                 
        Debug.Log($"Play BGM: {musicSource.clip.name} / Volume: {musicSource.volume}");


    }

    ///// <summary>
    ///// 指定した BGM を一時停止する。
    ///// 指定名の Sound を検索し、その AudioClip を musicSource に設定して Pause() を実行する。
    ///// すでに再生中の BGM を一時的に止めたい時に使用する。
    ///// </summary>
    ///// <param name="name">一時停止したい BGM 名（musicSounds 内の name）</param>
    //public void PauseMusic(string name)
    //{
    //    // musicSounds[] から指定された BGM 名を検索
    //    Sound s = Array.Find(musicSounds, x => x.name == name);

    //    // Sound が見つからない場合
    //    if (s == null)
    //    {
    //        Debug.Log("Sound が見つからない");
    //    }
    //    else　　// 見つかった場合 → Clip をセットして一時停止
    //    {
    //        musicSource.clip = s.clip;  // 操作対象の BGM を設定
    //        musicSource.Pause();        // 再生中の BGM を一時停止する
    //    }
    //}

    ///// <summary>
    ///// 一時停止中の BGM を再開する。
    ///// PauseMusic() で停止した BGM を続きから再生したい場合に使用する。
    ///// </summary>
    ///// <param name="name">再開したい BGM 名（musicSounds 内の name）</param>
    //public void UnPauseMusic(string name)
    //{
    //    // musicSounds[] から指定された BGM 名を検索
    //    Sound s = Array.Find(musicSounds, x => x.name == name);

    //    // Sound が見つからない場合
    //    if (s == null)
    //    {
    //        Debug.Log("// Sound が見つからない");
    //    }
    //    else　　// 見つかった場合 → AudioClip を設定して再開
    //    {
    //        musicSource.clip = s.clip;  // 再生対象の BGM を設定
    //        musicSource.UnPause();      // 一時停止から再開
    //    }
    //}

    /// <summary>
    /// 指定した BGM を停止する。
    /// 再生位置を巻き戻し、完全に停止させたい場合に使用する。
    /// 一時停止（Pause）ではなく、再生状態をリセットする動作となる。
    /// </summary>
    /// <param name="name">停止したい BGM 名（musicSounds 内の name）</param>
    public void EndMusic(StageBgm targetStage)
    {
        BgmData bgmData = Array.Find(stageBgmDates, x => x.stage == targetStage);
        // Sound が見つからない場合
        if (bgmData == null|| bgmData.clip == null)
        {
            Debug.LogWarning("bgmData が見つからない");
            return;
        }
        else    // 見つかった場合 → Clip をセットして停止
        {
           
            musicSource.clip = bgmData.clip;  // 停止対象となる AudioClip を設定
            musicSource.Stop();         // BGM を完全停止（再生位置は 0 に戻る）
        }
    }

    private int index = 0;
    /// <summary>
    /// Trapの効果音を再生する
    /// ・AudioSourceの配列を使い、順番に再生（簡易Pool）
    /// ・同時再生数を分散して、音切れや競合を防ぐ
    /// </summary>
    /// <param name="targetSfxData">再生するSFXデータ</param>
    public void PlayTrapSfx(TrapSfxData targetSfxData)
    {
        // データまたはClipが存在しない場合は警告を出して終了
        if (targetSfxData == null|| targetSfxData.clip == null)
        {
            Debug.LogWarning("Not TrapSfxDate or Clip");
            return;
        }
        else if (trapSfxSources == null || trapSfxSources.Length == 0)
        {
            Debug.LogWarning("AudioSource が設定されていません");
            return;
        }

        // 現在のAudioSourceで効果音を再生
        trapSfxSources[index].PlayOneShot(targetSfxData.clip, targetSfxData.volume / 100); // 0～1に変換

        // 次のAudioSourceへ（ループ）
        index = (index + 1) % trapSfxSources.Length;
    }



    //[SerializeField] private float musicLength;
    //[SerializeField] private int targetSample;
    //[SerializeField] private int startSample;


    //public void SampleInit(string name,float startTime, float endTime)
    //{


    //    Debug.LogWarning("SampleInit CALLED"); // ← 必须看到这个

    //    Sound s = Array.Find(musicSounds, x => x.name == name);
    //    if (s == null || s.clip == null)
    //    {
    //        Debug.LogWarning("No Crip");
    //        return;

    //    }
    //    int frequency = s.clip.frequency;
    //    startSample = Mathf.RoundToInt(startTime * frequency);
    //    targetSample = Mathf.RoundToInt(Mathf.Min(endTime, musicLength) * frequency);

    //}

    //public void MusicLoop()
    //{
    //    if (!musicSource.isPlaying) return;

    //    if (musicSource.timeSamples >= targetSample)
    //    {
    //        musicSource.timeSamples = startSample;
    //    }
    //}

}
