using System;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;


public class AStarNode
{
    private Vector2Int m_Pos;
    public Vector2Int Pos => m_Pos;
    public int X => m_Pos.x;
    public int Y => m_Pos.y;
    public float G;
    public float H;
    public float F => G + H;

    public AStarNode Parent;


    public AStarNode(Vector2Int pos)
    {
        m_Pos = pos;
        Reset();
    }

    public void Reset()
    {
        G = float.PositiveInfinity;  // 初始化为无限大，避免首次比较错误
        H = 0;
        Parent = null;
    }
}

/// <summary>
/// 最小堆AStarNode定制版，堆OpenList进行优化，取最小F O(1)，添加O(logn)，更新O(logn)，判断是否存在O(1)
/// </summary>
public class AStarMinHeap
{
    private List<AStarNode> m_HeapList;
    private Dictionary<Vector2Int, int> m_NodeIndexDict;

    public int Count => m_HeapList.Count;

    public AStarMinHeap(int capacity)
    {
        m_HeapList = new List<AStarNode>(capacity);
        m_NodeIndexDict = new Dictionary<Vector2Int, int>(capacity);
    }

    public void Clear()
    {
        m_HeapList.Clear();
        m_NodeIndexDict.Clear();
    }

    public void Add(AStarNode node)
    {
        m_HeapList.Add(node);
        var index = m_HeapList.Count - 1;
        m_NodeIndexDict[node.Pos] = index;
        HeapifyUp(index);
    }

    public AStarNode PopMin()
    {
        if (m_HeapList.Count == 0)
        {
            return null;
        }
        var min = m_HeapList[0];
        var last = m_HeapList[m_HeapList.Count - 1];
        m_HeapList[0] = last;
        m_NodeIndexDict[last.Pos] = 0;

        m_HeapList.RemoveAt(m_HeapList.Count - 1); // RemoveAt最后一个元素O(1)
        m_NodeIndexDict.Remove(min.Pos);

        if (m_HeapList.Count > 0)
        {
            HeapifyDown(0);
        }

        return min;
    }

    public void UpdateNode(AStarNode node)
    {
        if (!m_NodeIndexDict.TryGetValue(node.Pos, out var index))
        {
            return;
        }
        // 如果后续不确定G变大还是变小，上浮和下沉都判断；一般情况下G只会变小，只需上浮；
        HeapifyUp(index);
        HeapifyDown(index);
    }

    public bool Contains(AStarNode node)
    {
        return m_NodeIndexDict.ContainsKey(node.Pos);
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            if (m_HeapList[index].F >= m_HeapList[parentIndex].F)
            {
                break;
            }
            else
            {
                Swap(index, parentIndex);
                index = parentIndex;
            }
        }
    }

    private void HeapifyDown(int index)
    {
        int count = m_HeapList.Count;
        while(true)
        {
            int leftChildIndex = index * 2 + 1;
            int rightChildIndex = index * 2 + 2;
            int smallestIndex = index;
            if (leftChildIndex < count && m_HeapList[leftChildIndex].F < m_HeapList[smallestIndex].F)
            {
                smallestIndex = leftChildIndex;
            }
            if (rightChildIndex < count && m_HeapList[rightChildIndex].F < m_HeapList[smallestIndex].F)
            {
                smallestIndex = rightChildIndex;
            }
            if (smallestIndex == index)
            {
                break;
            }
            Swap(smallestIndex, index);
            index = smallestIndex;
        }
    }

    private void Swap(int i, int j)
    {
        var temp = m_HeapList[i];
        m_HeapList[i] = m_HeapList[j];
        m_HeapList[j] = temp;

        m_NodeIndexDict[m_HeapList[i].Pos] = i;
        m_NodeIndexDict[m_HeapList[j].Pos] = j;
    }
}


public class AStarFindPath
{
    private AStarMinHeap m_OpenList;  // OpenList 最小堆
    private HashSet<Vector2Int> m_CloseList; // CloseList 哈希表，用坐标即可
    private IWalkable m_Walkable;

    // GC优化
    private AStarNode[] m_AllAStarNodes; // 根据地图大小全部初始化
    private (int minX, int minY, int width) m_GridInfo;
    private List<Vector2Int> m_ResultPath; // 重复使用的最终结果路径列表
    private List<Vector2Int> m_TempPath; // 重复使用的临时路径列表


    public AStarFindPath()
    {
        m_Walkable = null;
    }

    public void Init((int minX, int minY, int maxX, int maxY) mapRange)
    {
        (int minX, int minY, int maxX, int maxY) = mapRange;
        int width = maxX - minX + 1;
        int height = maxY - minY + 1;
        int count = width * height;

        m_AllAStarNodes = new AStarNode[count];
        for (int i = 0; i < count; i++)
        {
            int gridX = minX + i % width;
            int gridY = minY + i / width;
            m_AllAStarNodes[i] = new AStarNode(new Vector2Int(gridX, gridY));
        }
        m_GridInfo = new(minX, minY, width);

        int capacity = Mathf.CeilToInt(count * 1.4f); // dict和hashset负载因子0.72  1/0.72=1.39
        m_OpenList = new AStarMinHeap(capacity);
        m_CloseList = new HashSet<Vector2Int>(capacity);

        m_ResultPath = new List<Vector2Int>(width); // 不用很大
        m_TempPath = new List<Vector2Int>(width);
    }

    public AStarNode FetchAStarNode(int gridX, int gridY)
    {
        int index = (gridY - m_GridInfo.minY) * m_GridInfo.width + (gridX - m_GridInfo.minX);
        return m_AllAStarNodes[index];
    }

    public float CalculateG(AStarNode cur, AStarNode neighbor)
    {
        int dx = Math.Abs(neighbor.X - cur.X);
        int dy = Math.Abs(neighbor.Y - cur.Y);
        return cur.G + ((dx == 0 || dy == 0) ? 1f : 1.41421356f);
        // return cur.G + (neighbor.Pos - cur.Pos).magnitude;  // 避免使用平方根运算耗性能
    }

    public float CalculateH(AStarNode cur, AStarNode target)
    {
        // 斜角距离
        float dx = Mathf.Abs(target.X - cur.X);
        float dy = Mathf.Abs(target.Y - cur.Y);
        return Mathf.Min(dx, dy) * 1.41421356f + Mathf.Abs(dx - dy);
        //   return Mathf.Max(Mathf.Abs(target.X - cur.X), Mathf.Abs(target.Y - cur.Y)); 
    }

    /// <summary>
    /// A*核心实现
    /// </summary>
    /// <param name="startGrid"></param>
    /// <param name="targetGrid"></param>
    /// <param name="walkableInterface">判断可达接口类，单一职责解耦</param>
    /// <returns></returns>
    public List<Vector2Int> FindPath(Vector2Int startGrid, Vector2Int targetGrid, IWalkable walkableInterface = null)
    {
        m_ResultPath.Clear();
        for(int i=0;i<m_AllAStarNodes.Length;i++)
        {
            m_AllAStarNodes[i].Reset();
        }

        float startTime1 = Time.realtimeSinceStartup;

        m_Walkable = walkableInterface;
        if (startGrid == targetGrid)
        {
            m_ResultPath.Add(startGrid);
            return m_ResultPath;
        }
        var startNode = FetchAStarNode(startGrid.x, startGrid.y);
        var targetNode = FetchAStarNode(targetGrid.x, targetGrid.y);

        var openList = m_OpenList;
        openList.Clear();
        var closeList = m_CloseList;
        closeList.Clear();
        startNode.G = 0;
        startNode.H = CalculateH(startNode, targetNode);
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // OpenList取F值最小的
            var curNode = openList.PopMin();
            closeList.Add(curNode.Pos);

            // 到达目标点
            if (curNode.X == targetNode.X && curNode.Y == targetNode.Y)
            {
                GenerateResultPath(curNode);
                //Debug.Log($"A*核心 生成路径耗时 {(Time.realtimeSinceStartup - startTime1) * 1000:F3}ms");
                float startTime2 = Time.realtimeSinceStartup;
                PathOptimization();
                //Debug.Log($"A* 路径优化耗时 {(Time.realtimeSinceStartup - startTime2) * 1000:F3}ms");
                return m_ResultPath;
            }

            // 八方向邻居
            var directionEight = DirectionDefine.Eight;
            var directionCount = directionEight.GetLength(0);
            for (int i = 0; i < directionCount; i++)
            {
                // walkable判断
                var neighborNode = FetchAStarNode(curNode.X + directionEight[i, 0], curNode.Y + directionEight[i, 1]);
                if (walkableInterface != null && !walkableInterface.IsWalkable(neighborNode.X, neighborNode.Y))
                {
                    continue;
                }
                // CloseList判断
                if (closeList.Contains(neighborNode.Pos))
                {
                    continue;
                }
                // 更小的G或者不在OpenList则加入
                var newG = CalculateG(curNode, neighborNode);
                var isInOpenList = openList.Contains(neighborNode);
                if (newG < neighborNode.G || !isInOpenList)
                {
                    neighborNode.G = newG;
                    neighborNode.Parent = curNode;
                    if (!isInOpenList)
                    {
                        neighborNode.H = CalculateH(neighborNode, targetNode); // 被加入到OpenList计算一个H即可，避免放到外面重复计算
                        openList.Add(neighborNode);
                    }
                }
            }
        }
        return m_ResultPath;
    }

    /// <summary>
    /// 倒序得到路径
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public void GenerateResultPath(AStarNode node)
    {
        while (node != null)
        {
            m_ResultPath.Add(node.Pos);
            node = node.Parent;
        }
        m_ResultPath.Reverse();
    }

    public void PathOptimization()
    {
        ReduceCollinearPoints();
        ThickLOSSmoothing();
        TrimStartPoint();
    }

    /// <summary>
    /// step1.路径简化，共线点删除：斜率的多个点相同只保留两个端点，删掉多余点，大幅度减少后续计算量
    /// </summary>
    public void ReduceCollinearPoints()
    {
        if (m_ResultPath == null || m_ResultPath.Count < 3)
        {
            return;
        }

        m_TempPath.Clear();

        var lastDir = m_ResultPath[1] - m_ResultPath[0];
        for (int i = 2; i < m_ResultPath.Count; i++)
        {
            var curDir = m_ResultPath[i] - m_ResultPath[i - 1];
            if (curDir != lastDir) // 因为连续点都是VectorInt，所以向量不等则方向不同
            {
                m_TempPath.Add(m_ResultPath[i - 1]);
                lastDir = curDir;
            }
        }

        m_TempPath.Add(m_ResultPath[m_ResultPath.Count - 1]);

        m_ResultPath.Clear();
        m_ResultPath.AddRange(m_TempPath);
    }


    /// <summary>
    /// step2.贪心式LOS路径平滑：AC两点可以直达则可去掉中间点B，用Bresenham生成两点间的路径点同时考虑IsWalkable
    /// </summary>
    public void ThickLOSSmoothing()
    {
        if (m_ResultPath.Count <= 2)
        {
            return;
        }

        m_TempPath.Clear();
        int start = 0;
        var last = m_ResultPath.Count - 1;
        while (start < last)
        {
            m_TempPath.Add(m_ResultPath[start]);
            int end = start + 1; // 至少+1避免死循环
            for (int target = last; target > start + 1; target--)
            {
                if (BresenhamLineCheck(m_ResultPath[start], m_ResultPath[target]))
                {
                    end = target;
                    break;
                }
            }
            start = end;
        }
        m_TempPath.Add(m_ResultPath[last]);

        m_ResultPath.Clear();
        m_ResultPath.AddRange(m_TempPath);
    }

    /// <summary>
    /// 两点是否直接可达检测，检查两点之间的离散点是否都可达，和A*的walkable使用相同的判断条件
    /// </summary>
    /// <param name="start"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool BresenhamLineCheck(Vector2Int start, Vector2Int target)
    {
        if (m_Walkable == null)
        {
            return true;
        }
        int x0 = start.x;
        int y0 = start.y;
        int x1 = target.x;
        int y1 = target.y;
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int err = dx - dy;
        int sx = (x0 < x1) ? 1 : -1;
        int sy = (y0 < y1) ? 1 : -1;

        while (true)
        {
            if (!m_Walkable.IsWalkable(x0, y0))
            {
                return false;
            }
            if (x0 == x1 && y0 == y1)  // 到达重点
            {
                break;
            }
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
        return true;
    }

    /// <summary>
    /// step3.路径优化，去掉第一个路径点，避免走向第一个路径点时，与整体寻路整体方向不符合反方向走的情况
    /// 为什么先LOS再Trim Start？ 因为先Trim Start会影响LOS结果，比如世界坐标a对应格子坐标A，路径A->B->C，LOS优化结果为a->A->C，如果先Trim Start结果就会变为a->B->C
    /// 为什么可以直接去掉path[0]呢？ 因为世界坐标a对应的格子坐标A就是path[0]，同时A*和LOS都进行了相同的严格walkable检测（半径扩张或者斜线4方向检查），A->C可达，则所有映射为A的世界坐标a->C可达
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public void TrimStartPoint()
    {
        if (m_ResultPath.Count > 1)
        {
            m_ResultPath.RemoveAt(0);
        }
    }
}
