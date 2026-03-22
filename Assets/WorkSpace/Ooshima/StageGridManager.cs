using System.Collections.Generic;
using UnityEngine;

// Ooshima: Newly added StageGridManager class
public enum GridState
{
    Empty,
    Platform,
    NoPutArea,
    Trap,
    OutOfBounds
}

public class StageGridManager : MonoBehaviour
{
    public static StageGridManager Instance;

    [Header("Map Scan Settings")]
    public Transform scanAreaLeftTop;
    public Transform scanAreaRightDown;
    public float gridSize = 1f;
    public Vector2 gridOffset = Vector2.zero;

    [Header("Developer Settings")]
    [Tooltip("手動でトラップ配置不可にしたいグリッド座標のリスト")]
    public List<Vector2Int> customNoPutCoords = new List<Vector2Int>();

    private Dictionary<Vector2Int, GridState> gridMap = new Dictionary<Vector2Int, GridState>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    private void Start()
    {
        BuildGridMap();
    }

    private void BuildGridMap()
    {
        gridMap.Clear();
        int indexedCount = 0;

        if (scanAreaLeftTop == null || scanAreaRightDown == null)
        {
            Debug.LogWarning("StageGridManager: Scan areas are not set.");
            return;
        }

        float startX = scanAreaLeftTop.position.x;
        float endX = scanAreaRightDown.position.x;
        float startY = scanAreaRightDown.position.y;
        float endY = scanAreaLeftTop.position.y;

        int minGridX = Mathf.FloorToInt((Mathf.Min(startX, endX) - gridOffset.x) / gridSize);
        int maxGridX = Mathf.CeilToInt((Mathf.Max(startX, endX) - gridOffset.x) / gridSize);
        int minGridY = Mathf.FloorToInt((Mathf.Min(startY, endY) - gridOffset.y) / gridSize);
        int maxGridY = Mathf.CeilToInt((Mathf.Max(startY, endY) - gridOffset.y) / gridSize);

        // Uses the newly added layers in GameManager.cs
        int layerMask = (1 << UseLayerName.platformLayer) | (1 << UseLayerName.noPutAreaLayer);

        for (int x = minGridX; x <= maxGridX; x++)
        {
            for (int y = minGridY; y <= maxGridY; y++)
            {
                Vector2Int gridCoord = new Vector2Int(x, y);
                Vector2 worldPos = new Vector2(x * gridSize + gridOffset.x, y * gridSize + gridOffset.y);

                if (customNoPutCoords.Contains(gridCoord))
                {
                    gridMap[gridCoord] = GridState.NoPutArea;
                    indexedCount++;
                    continue;
                }

                Collider2D col = Physics2D.OverlapPoint(worldPos, layerMask);
                if (col != null)
                {
                    if (col.gameObject.layer == UseLayerName.platformLayer)
                    {
                        gridMap[gridCoord] = GridState.Platform;
                    }
                    else if (col.gameObject.layer == UseLayerName.noPutAreaLayer)
                    {
                        gridMap[gridCoord] = GridState.NoPutArea;
                    }
                    indexedCount++;
                }
                else
                {
                    gridMap[gridCoord] = GridState.Empty;
                }
            }
        }

        Debug.Log($"StageGridManager: Indexed {indexedCount} grid cells from scene.");
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - gridOffset.x) / gridSize);
        int y = Mathf.RoundToInt((worldPos.y - gridOffset.y) / gridSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(Vector2Int gridCoord)
    {
        return new Vector3(gridCoord.x * gridSize + gridOffset.x, gridCoord.y * gridSize + gridOffset.y, 0f);
    }

    public bool CanPlaceTrapDataDriven(Vector3 worldPos)
    {
        Vector2Int gridCoord = WorldToGrid(worldPos);
        return CanPlaceTrapDataDriven(gridCoord);
    }

    public bool CanPlaceTrapDataDriven(Vector2Int gridCoord)
    {
        if (gridMap.TryGetValue(gridCoord, out GridState state))
        {
            return state == GridState.Empty;
        }
        return false; // Out of bounds or not scanned
    }

    public void RegisterTrap(Vector3 worldPos)
    {
        Vector2Int gridCoord = WorldToGrid(worldPos);
        RegisterTrap(gridCoord);
    }

    public void RegisterTrap(Vector2Int gridCoord)
    {
        if (gridMap.ContainsKey(gridCoord))
        {
            gridMap[gridCoord] = GridState.Trap;
        }
    }

    public void MoveTrap(Vector2Int fromGrid, Vector2Int toGrid)
    {
        if (fromGrid == toGrid) return;
        
        if (gridMap.ContainsKey(fromGrid))
        {
            if (gridMap[fromGrid] == GridState.Trap)
            {
                gridMap[fromGrid] = GridState.Empty;
            }
        }
        
        if (gridMap.ContainsKey(toGrid))
        {
            // Only occupy the destination if it's currently Empty
            // (Don't overwrite Platform or NoPutArea with a moving trap)
            if (gridMap[toGrid] == GridState.Empty)
            {
                gridMap[toGrid] = GridState.Trap;
            }
        }
    }

    public void ChangeGridState(Vector2Int gridCoord, GridState newState)
    {
        if (gridMap.ContainsKey(gridCoord))
        {
            gridMap[gridCoord] = newState;
        }
    }

    // Ooshima: Added Unregister method when trap is destroyed
    public void UnregisterTrap(Vector3 worldPos)
    {
        Vector2Int gridCoord = WorldToGrid(worldPos);
        UnregisterTrap(gridCoord);
    }

    // Ooshima: Added overload to unregister by Grid Coordinate directly
    public void UnregisterTrap(Vector2Int gridCoord)
    {
        if (gridMap.ContainsKey(gridCoord))
        {
            // Only unregister if the grid isn't occupied by something else permanent
            if (gridMap[gridCoord] == GridState.Trap)
            {
                gridMap[gridCoord] = GridState.Empty;
            }
        }
    }
}
