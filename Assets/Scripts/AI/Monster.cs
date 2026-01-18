using System.Collections.Generic;
using UnityEngine;


public class Monster : MonoBehaviour
{
    // 移动
    public float MoveSpeed = 3.0f;
    private List<Vector2Int> m_PathList;
    private int m_PathIndex;
    private IWalkable m_Walkalbe;
    public IWalkable Walkable => m_Walkalbe;
    // 旋转
    public float RotateSpeed = 360.0f;
    // 半径
    public float Radius = 0f;
    

    private void Awake()
    {
        m_PathList = new List<Vector2Int>();
        m_PathIndex = -1;
        m_Walkalbe = new CompositeWalkable(Radius);
    }

    private void Start()
    {
         
    }

    private void Update()
    {
        if (m_PathList == null || m_PathList.Count == 0 || m_PathIndex >= m_PathList.Count)
        {
            return;
        }

        var targetPos = MapUtil.Grid2WorldPos(m_PathList[m_PathIndex], transform.position.y);

        // 旋转
        var direction = targetPos - transform.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            var targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, RotateSpeed * Time.deltaTime);
        }

        // 移动
        transform.position = Vector3.MoveTowards(transform.position, targetPos, MoveSpeed * Time.deltaTime);
        if ((transform.position - targetPos).sqrMagnitude < 0.001f)
        {
            m_PathIndex++;
        }
    }

    public void SetPath(List<Vector2Int> pathList)
    {
        m_PathList = pathList;
        m_PathIndex = 0;
    }

    private void OnDrawGizmos()
    {
        if (!MapUtil.DEBUG_MAP)
        {
            return;
        }
        // 画占格大小
        int inflate = Mathf.CeilToInt(Radius / MapUtil.GRID_SIZE);
        Vector2Int centerGrid = MapUtil.WorldPos2Grid(transform.position);
        Gizmos.color = Color.red;
        for (int x = centerGrid.x - inflate; x <= centerGrid.x + inflate; x++)
        {
            for (int y = centerGrid.y - inflate; y <= centerGrid.y + inflate; y++)
            {
                Vector3 pos = MapUtil.Grid2WorldPos(new Vector2Int(x, y), transform.position.y);
                Gizmos.DrawWireCube(pos, Vector3.one * MapUtil.GRID_SIZE);
            }
        }
        // 画路径点和路线
        if (m_PathList != null && m_PathList.Count > 0)
        {
            Gizmos.color = Color.black;
            int count = m_PathList.Count;
            Gizmos.DrawSphere(MapUtil.Grid2WorldPos(m_PathList[0], transform.position.y), 0.2f);
            if (count > 1)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    var cur = MapUtil.Grid2WorldPos(m_PathList[i], transform.position.y);
                    var next = MapUtil.Grid2WorldPos(m_PathList[i + 1], transform.position.y);
                    Gizmos.DrawLine(cur, next);
                    Gizmos.DrawSphere(next, 0.2f);
                }
            }
        }
    }

}
