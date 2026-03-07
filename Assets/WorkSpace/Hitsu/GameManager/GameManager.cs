using UnityEngine;
using UnityEngine.InputSystem;

public static class UseLayerName
{
    //Runner Controller
    public static int runnerLayer;
    public static int platformLayer;
    public static int oneWayPlatformLayer;
    public static int triggersLayer;

    //Map
    public static int mapLayer;

    //Hunter
    public static int trapLayer;
    public static int runnerCantSeeLayer;
    private static int GetLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);

        if (layer == -1)
        {
            Debug.LogWarning($"{layerName} Layer not exist");
        }

        return layer;
    }

    private static bool isLayerSetUp = false;
    public static void UseLayerName_Init()
    {
        if (isLayerSetUp) return;
        runnerLayer = GetLayer("Runner");
        platformLayer = GetLayer("Platform");
        oneWayPlatformLayer = GetLayer("OneWayPlatform");
        triggersLayer = GetLayer("Triggers");
        mapLayer = GetLayer("Default");
        trapLayer = GetLayer("Trap");
        runnerCantSeeLayer = GetLayer("RunnerCantSee");


        isLayerSetUp = true;
    }

}



public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static InputDevice inputDevice;

    public Camera runnerCamera;
    public Camera hunterCamera;

    public HunterConTrollerPad hunterConTrollerPad;

    public Player player01;
    public Player player02;


    private Camera TargetCamera(Player.Job targetJob)
    {
        switch (targetJob)
        {
            case Player.Job.Runner: return runnerCamera;
            case Player.Job.Hunter: return hunterCamera;

        }
        return null;
    }
    public Gamepad TargetGamepad(int targetCode)
    {
        if(targetCode >= inputDevice.gamepad.Count)
        {
            Debug.LogError($"Not this Decives, The Max connenting Device max are : {inputDevice.gamepad.Count}" );
            return null;
        }
        return inputDevice.gamepad[targetCode];
    }

    private void InputInit()
    {
        inputDevice = new InputDevice();
        inputDevice.InputInit();

        Debug.Log(inputDevice.mouse.name);
        Debug.Log(inputDevice.keyboard.name);

        if (inputDevice.gamepad.Count > 0)
        {
            for (int i = 0; i < inputDevice.gamepad.Count; i++)
            {
                Debug.Log(inputDevice.gamepad[i].name);
            }
        }

    }
    private void PlayerInit()
    {
        GameObject playerGroup = new GameObject();
        GameObject player1 = new GameObject();
        GameObject player2 = new GameObject();

        playerGroup.name = "------Player------";
        player1.name = "Player01";
        player2.name = "Player02";
        player1.transform.parent = playerGroup.transform;
        player2.transform.parent = playerGroup.transform;

        player01 = player1.AddComponent<Player>();
        player02 = player2.AddComponent<Player>();

        if (inputDevice.gamepad.Count >= 2)
        {
            player01.PlayerInit(Player.Job.Runner, runnerDisplay,0);
            player02.PlayerInit(Player.Job.Hunter, hunterDisplay,1);
        }
        else
        {
            player01.PlayerInit(Player.Job.Runner, runnerDisplay, 0);
            player02.PlayerInit(Player.Job.Hunter, hunterDisplay, 0);
        }


    }

    private const int runnerDisplay = 0;
    private const int hunterDisplay = 1;
    private void DisPlayInit()
    {
        runnerCamera.targetDisplay = runnerDisplay;
        hunterCamera.targetDisplay = hunterDisplay;
    }

    private void GameManager_Init()
    {
        InputInit();
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }
        DisPlayInit();
        UseLayerName.UseLayerName_Init();

        PlayerInit();





    }

    private void JobSwitch()
    {
        Player.Job job1 = player01.job;
        Player.Job job2 = player02.job;

        player01.SetJop(job2);
        player02.SetJop(job1);

        Camera cam1 = TargetCamera(job2);
        Camera cam2 = TargetCamera(job1);

        cam1.targetDisplay = player01.displayCode;
        cam2.targetDisplay = player02.displayCode;
    }



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else Destroy(this);

    }

    private void Start()
    {
        GameManager_Init();
    }

    public bool test;
    private void Update()
    {
        if (test)
        {
            JobSwitch();
            test = false;
        }


    }

}

