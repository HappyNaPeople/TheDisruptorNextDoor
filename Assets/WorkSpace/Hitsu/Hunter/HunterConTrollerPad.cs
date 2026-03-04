using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


struct TrapData
{
    public TrapName trapName;
    public Sprite trapSprite;
}

public class HunterConTrollerPad : MonoBehaviour
{
    public Camera hunterCamera;

    public Transform putAreaLeftTop;
    public Transform putAreaRightDown;

    private HashSet<TrapData> trapDataList;
    public List<Button> trapButtonList;

    private GameObject TargetTrap(TrapName trapName)
    {
        switch (trapName)
        {
            case TrapName.Spikes: return spikesPrefab;
            case TrapName.FallRock: return fallRockPrefab;
        }
        return null;
    }

    public GameObject spikesPrefab;
    public GameObject fallRockPrefab;

    public LayerMask mapLayer;

    

    public void HunterConTrollerPad_init()
    {
        trapDataList = new HashSet<TrapData>();
        trapDataList.Add(new TrapData()
        {
            trapName = TrapName.Spikes,
            trapSprite = Resources.Load<Sprite>("Texture/Traps/Spike")
        });
        trapDataList.Add(new TrapData()
        {
            trapName = TrapName.FallRock,
            trapSprite = Resources.Load<Sprite>("Texture/Traps/FallRock")
        });

        List<TrapName> useTrap = new List<TrapName>();
        useTrap.Add(TrapName.FallRock);
        useTrap.Add(TrapName.Spikes);
        useTrap.Add(TrapName.Spikes);
        useTrap.Add(TrapName.Spikes);
        useTrap.Add(TrapName.Spikes);
        useTrap.Add(TrapName.FallRock);
        useTrap.Add(TrapName.Spikes);
        useTrap.Add(TrapName.Spikes);
        useTrap.Add(TrapName.Spikes);
        useTrap.Add(TrapName.Spikes);

        //useTrap.Add(new FallRock());
        CanUseTrapInit(useTrap);
    }

    private void Start()
    {
        HunterConTrollerPad_init();
    }
    Vector3 mouseWorldPos => hunterCamera.ScreenToWorldPoint(new Vector3(GameManager.inputDevice.mouse.position.ReadValue().x, GameManager.inputDevice.mouse.position.ReadValue().y, 10));
    private bool IsInPutArea(Vector3 trap)
    {
        if(trap.x > putAreaRightDown.position.x||
            trap.x < putAreaLeftTop.position.x||
            trap.y < putAreaRightDown.position.y||
            trap.y > putAreaLeftTop.position.y) return false;

        return true;
    }
    private bool IsOnMap()
    {
        Collider2D col = Physics2D.OverlapPoint(mouseWorldPos, mapLayer);
        return col != null;
    }

    private GameObject choseTrap;
    private Coroutine createTrap;
    private void Reject() 
    {
        if (choseTrap != null)
        {
            Destroy(choseTrap);
            choseTrap = null;
        }

        if (createTrap != null)
        {
            StopCoroutine(createTrap);
            createTrap = null;
        }
    }
    private IEnumerator PutTrap(TrapName trapName)
    {

        if (TargetTrap(trapName) == null)
        {
            Debug.Log("No Trap");
            yield break;
        }
        Debug.Log("Have Trap");

        GameObject targetTrap = Instantiate(TargetTrap(trapName), mouseWorldPos, Quaternion.identity);
        choseTrap = targetTrap;
        //while (!GameManager.inputDevice.mouse.leftButton.isPressed)
        //    yield return null;

        while (!GameManager.inputDevice.mouse.leftButton.isPressed)
        {
            targetTrap.transform.position = mouseWorldPos;
            yield return null;
        }

        if (IsInPutArea(targetTrap.transform.position) && !IsOnMap())
        {
            Trap trap = targetTrap.GetComponent<Trap>();
            trap.Init();
            trap.SetUp();

        }
        else
        {
            Destroy(targetTrap);
        }
        createTrap = null;
        choseTrap = null;
    }

    public void Button_CreateSpikes()
    {
        Reject();
        createTrap = StartCoroutine(PutTrap(TrapName.Spikes));
    }
    public void Button_FallRock()
    {
        Reject();
        createTrap = StartCoroutine(PutTrap(TrapName.FallRock));
    }

    private Sprite TrapSprite(TrapName useTrap)
    {
        foreach(TrapData trapData in trapDataList)
        {
            if(trapData.trapName == useTrap) return trapData.trapSprite;
        }
        return null;
    }

    public void CanUseTrapInit(List<TrapName> targetTrap)
    {
        List<TrapName> useTrap = targetTrap.Distinct().ToList(); ;
        int index;
        int trapTypeMax = useTrap.Count > trapButtonList.Count ? trapButtonList.Count : useTrap.Count;

        for (index = 0; index < trapButtonList.Count; index++)
        {
            trapButtonList[index].gameObject.SetActive(false);
            trapButtonList[index].onClick.RemoveAllListeners();

            if (index < trapTypeMax)
            {
                switch (useTrap[index])
                {
                    case TrapName.Spikes:
                        trapButtonList[index].onClick.AddListener(Button_CreateSpikes);
                        break;

                    case TrapName.FallRock:
                        trapButtonList[index].onClick.AddListener(Button_FallRock);
                        break;
                }

                trapButtonList[index].image.sprite = TrapSprite(useTrap[index]);
                trapButtonList[index].gameObject.SetActive(true);

            }
            else
            {
                trapButtonList[index].gameObject.SetActive(false);
            }
        }


    }





}
