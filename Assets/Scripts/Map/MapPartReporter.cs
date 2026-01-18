using UnityEngine;


[RequireComponent(typeof(Collider))]
public class MapPartReporter : MonoBehaviour
{
    private Vector2Int m_MinGrid;
    private Vector2Int m_MaxGrid;

    private void Awake()
    {
        CalculateGridRange();
    }

    private void CalculateGridRange()
    {
        var bounds = gameObject.GetComponent<Collider>().bounds;
        m_MinGrid = new Vector2Int(Mathf.FloorToInt(bounds.min.x / MapUtil.GRID_SIZE), Mathf.FloorToInt(bounds.min.z / MapUtil.GRID_SIZE));
        m_MaxGrid = new Vector2Int(Mathf.CeilToInt(bounds.max.x / MapUtil.GRID_SIZE) - 1, Mathf.CeilToInt(bounds.max.z / MapUtil.GRID_SIZE) - 1);
    }

    // 可用时添加自己的地形数据到MapData中
    private void OnEnable()
    {
        for (int x = m_MinGrid.x; x <= m_MaxGrid.x; x++)
        {
            for (int y = m_MinGrid.y; y <= m_MaxGrid.y; y++)
            {
                MapData.Instance.AddFlag(x, y, (MapFlags)(1 << gameObject.layer));
            }
        }
        if (gameObject.layer == LayerDefine.Layer_Ground)
        {
            MapData.Instance.UpdateMapRange(m_MinGrid, m_MaxGrid);
        }
    }

    // 不可用时移除自己的地形数据
    private void OnDisable()
    {
        for (int x = m_MinGrid.x; x <= m_MaxGrid.x; x++)
        {
            for (int y = m_MinGrid.y; y <= m_MaxGrid.y; y++)
            {
                MapData.Instance.RemoveFlag(x, y, (MapFlags)(1<<gameObject.layer));
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!MapUtil.DEBUG_MAP)
        {
            return;
        }
        if (!Application.isPlaying)
        {
            CalculateGridRange();
        }
        if (gameObject.layer == LayerDefine.Layer_Obstacle)
        {
            Gizmos.color = Color.yellow;
        }
        else
        {
            Gizmos.color = Color.green;
        }
        for (int x = m_MinGrid.x; x <= m_MaxGrid.x; x++)
        {
            for (int y = m_MinGrid.y; y <= m_MaxGrid.y; y++)
            {
                var center = MapUtil.Grid2WorldPos(new Vector2Int(x, y), transform.position.y);
                Gizmos.DrawWireCube(center, Vector3.one * MapUtil.GRID_SIZE);
            }
        }
    }
}
