using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class HunterConTrollerPad : MonoBehaviour
{
    public Camera hunterCamera;

    public Transform putAreaLeftTop;
    public Transform putAreaRightDown;

    enum TrapName
    {
        Spikes,
        FallRock
    }

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

}
