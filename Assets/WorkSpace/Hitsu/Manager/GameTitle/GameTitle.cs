using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class GameTitle : MonoBehaviour
{
    public static GameTitle Instance;

    public Player _player01 => GameManager.Instance.player01;
    public Camera player01Camera;
    public TitleCanvas player01Canvas;

    public Player _player02 => GameManager.Instance.player02;
    public Camera player02Camera;
    public TitleCanvas player02Canvas;

    private void PlayerCanvas_Init()
    {
        player01Canvas.targetPlayer = _player01;
        player01Camera.targetDisplay = (int)_player01.displayCode;

        player02Canvas.targetPlayer = _player02;
        player02Camera.targetDisplay = (int)_player02.displayCode;

        player01Canvas.TitleCanvas_Init();
        player02Canvas.TitleCanvas_Init();
    }

    private bool IsEndChooseTrap() => player01Canvas.isPlayerReady && player02Canvas.isPlayerReady;
    private IEnumerator EndProcess()
    {
        while(!IsEndChooseTrap())
        {
            yield return null;
        }
        bool player01Done, player02Done;
        player01Canvas.ChoseTrapToBackpack(out player01Done);
        player02Canvas.ChoseTrapToBackpack(out player02Done);

        if(!player01Done|| !player02Done)
        {
            Debug.LogWarning($"Player01 id chose Trap : {player01Done} , Player01 id chose Trap : {player02Done}");
        }

        SceneManager.LoadScene("Test_InGame");

    }

    private void GameTitle_Init()
    {
        PlayerCanvas_Init();

        StartCoroutine(EndProcess());
    }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(this);
    }

    private void Start()
    {
        GameTitle_Init();
    }


}
