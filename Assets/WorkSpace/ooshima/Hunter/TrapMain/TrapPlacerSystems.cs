using UnityEngine;

public abstract class TrapPlacer : MonoBehaviour
{
    protected Trap trap;
    protected SpriteRenderer[] renderers;
    protected Color[] originalColors;

    public virtual void InitializePreview()
    {
        trap = GetComponent<Trap>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].color;
        }

        gameObject.layer = UseLayerName.runnerCantSeeLayer;
        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.layer = UseLayerName.runnerCantSeeLayer;
            }
        }
    }

    public abstract void UpdatePreviewPosition(Vector3 mouseWorldPos);
    
    public abstract bool ValidatePlacement();

    public void UpdatePreviewColor(bool isValid)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].color = isValid ? originalColors[i] : new Color(1f, 0f, 0f, 0.5f);
        }
    }

    public virtual void RestoreVisuals()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].color = originalColors[i];
        }
    }

    protected bool IsOnMap(Vector3 pos)
    {
        int mask = (1 << UseLayerName.platformLayer) | (1 << UseLayerName.trapLayer);
        return Physics2D.OverlapPoint(pos, mask) != null;
    }

    protected bool IsInPutArea(Vector3 pos)
    {
        if (HunterConTrollerPad.Instance == null) return false;
        var leftTop = HunterConTrollerPad.Instance.putAreaLeftTop;
        var rightDown = HunterConTrollerPad.Instance.putAreaRightDown;
        
        if (pos.x > rightDown.position.x ||
            pos.x < leftTop.position.x ||
            pos.y < rightDown.position.y ||
            pos.y > leftTop.position.y) return false;

        return true;
    }
}

public class StandardTrapPlacer : TrapPlacer
{
    public override void UpdatePreviewPosition(Vector3 mouseWorldPos)
    {
        transform.position = mouseWorldPos;
    }

    public override bool ValidatePlacement()
    {
        return IsInPutArea(transform.position) && !IsOnMap(transform.position);
    }
}

public class WallTrapPlacer : TrapPlacer
{
    public override void UpdatePreviewPosition(Vector3 mouseWorldPos)
    {
        transform.position = mouseWorldPos;

        if (CheckSpikePlacement(mouseWorldPos, out Quaternion rot))
        {
            transform.rotation = rot;
        }
    }

    public override bool ValidatePlacement()
    {
        if (!IsInPutArea(transform.position) || IsOnMap(transform.position)) return false;
        return CheckSpikePlacement(transform.position, out _);
    }

    private bool CheckSpikePlacement(Vector3 pos, out Quaternion rotation)
    {
        rotation = Quaternion.identity;
        float checkDist = StageGridManager.Instance != null ? StageGridManager.Instance.gridSize : 1f; 
        int layerMask = 1 << UseLayerName.platformLayer;
        
        if (Physics2D.OverlapPoint(pos + Vector3.down * checkDist, layerMask))
        {
            rotation = Quaternion.Euler(0, 0, 0);
            return true;
        }
        if (Physics2D.OverlapPoint(pos + Vector3.up * checkDist, layerMask))
        {
            rotation = Quaternion.Euler(0, 0, 180);
            return true;
        }
        if (Physics2D.OverlapPoint(pos + Vector3.right * checkDist, layerMask))
        {
            rotation = Quaternion.Euler(0, 0, 90);
            return true;
        }
        if (Physics2D.OverlapPoint(pos + Vector3.left * checkDist, layerMask))
        {
            rotation = Quaternion.Euler(0, 0, -90);
            return true;
        }
        return false;
    }
}

public class WorldTrapPlacer : TrapPlacer
{
    private Vector3 targetPos;

    public override void UpdatePreviewPosition(Vector3 mouseWorldPos)
    {
        float playerX = 0f;
        if (InGame.Instance != null && InGame.Instance.runner != null)
            playerX = InGame.Instance.runner.transform.position.x;
            
        float stageY = 0f;
        if (StageGridManager.Instance != null && StageGridManager.Instance.scanAreaLeftTop != null && StageGridManager.Instance.scanAreaRightDown != null)
            stageY = (StageGridManager.Instance.scanAreaLeftTop.position.y + StageGridManager.Instance.scanAreaRightDown.position.y) / 2f;

        targetPos = new Vector3(playerX - 20f, stageY, 0f);
        transform.position = targetPos;
    }

    public override bool ValidatePlacement()
    {
        return true; 
    }
}
