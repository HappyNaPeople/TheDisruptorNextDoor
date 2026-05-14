using UnityEngine;

public abstract class TrapPlacer : MonoBehaviour
{
    protected Trap trap;
    protected SpriteRenderer[] renderers;
    protected Color[] originalColors;

    protected GameObject instantiatedPreview;
    protected GameObject instantiatedInvalidPreview;
    protected SpriteRenderer[] originalRenderers;

    public virtual void InitializePreview()
    {
        trap = GetComponent<Trap>();
        originalRenderers = GetComponentsInChildren<SpriteRenderer>();

        if (trap != null)
        {
            if (trap.previewPrefab != null)
            {
                instantiatedPreview = Instantiate(trap.previewPrefab, transform.position, transform.rotation, transform);
                renderers = instantiatedPreview.GetComponentsInChildren<SpriteRenderer>();
            }
            if (trap.invalidPreviewPrefab != null)
            {
                instantiatedInvalidPreview = Instantiate(trap.invalidPreviewPrefab, transform.position, transform.rotation, transform);
                instantiatedInvalidPreview.SetActive(false);
            }
        }

        if (renderers == null || renderers.Length == 0)
        {
            renderers = originalRenderers;
        }
        else
        {
            foreach (var r in originalRenderers)
            {
                r.enabled = false;
            }
        }

        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].color;
        }

        gameObject.layer = UseLayerName.runnerCantSeeLayer;
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            child.gameObject.layer = UseLayerName.runnerCantSeeLayer;
        }
    }

    public abstract void UpdatePreviewPosition(Vector3 mouseWorldPos);
    
    public abstract bool ValidatePlacement();

    public void UpdatePreviewColor(bool isValid)
    {
        if (instantiatedInvalidPreview != null)
        {
            if (isValid)
            {
                instantiatedInvalidPreview.SetActive(false);
                if (instantiatedPreview != null) instantiatedPreview.SetActive(true);
                else 
                {
                    foreach (var r in renderers) if (r != null) r.enabled = true;
                }
            }
            else
            {
                instantiatedInvalidPreview.SetActive(true);
                if (instantiatedPreview != null) instantiatedPreview.SetActive(false);
                else
                {
                    foreach (var r in renderers) if (r != null) r.enabled = false;
                }
            }
        }
        else
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].color = isValid ? originalColors[i] : new Color(1f, 0f, 0f, 0.5f);
                }
            }
        }
    }

    public virtual void RestoreVisuals()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].color = originalColors[i];
            }
        }

        if (instantiatedPreview != null)
        {
            Destroy(instantiatedPreview);
        }
        if (instantiatedInvalidPreview != null)
        {
            Destroy(instantiatedInvalidPreview);
        }

        if (originalRenderers != null)
        {
            foreach (var r in originalRenderers)
            {
                if (r != null) r.enabled = true;
            }
        }
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
        if (StageGridManager.Instance == null) return false;
        return StageGridManager.Instance.CanPlaceTrapDataDriven(transform.position);
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
        if (StageGridManager.Instance == null) return false;
        if (!StageGridManager.Instance.CanPlaceTrapDataDriven(transform.position)) return false;
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
