using System;
using System.Collections.Generic;
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

    public AStarMinHeap()
    {
        m_HeapList = new List<AStarNode>();
        m_NodeIndexDict = new Dictionary<Vector2Int, int>();
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

    public AStarFindPath()
    {
        m_OpenList = new AStarMinHeap();
        m_CloseList = new HashSet<Vector2Int>();
        m_Walkable = null;
    }

    public AStarNode GetMinFNode(List<AStarNode> openList)
    {
        int minIndex = 0;
        for(int i=1;i<openList.Count;i++)
        {
            if (openList[i].F < openList[minIndex].F)
            {
                minIndex = i;
            }
        }
        var node = openList[minIndex];
        openList.RemoveAt(minIndex);
        return node;
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

        float startTime1 = Time.realtimeSinceStartup;

        m_Walkable = walkableInterface;
        if (startGrid == targetGrid)
        {
            return new List<Vector2Int>() { startGrid };
        }
        var startNode = new AStarNode(startGrid);
        var targetNode = new AStarNode(targetGrid);

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
                var path = GenerateResultPath(curNode);
                Debug.Log($"A*核心 生成路径耗时 {(Time.realtimeSinceStartup - startTime1) * 1000:F3}ms");
                float startTime2 = Time.realtimeSinceStartup;
                path = PathOptimization(path);
                Debug.Log($"A* 路径优化 {(Time.realtimeSinceStartup - startTime2) * 1000:F3}ms");
                return path;
            }

            // 八方向邻居
            var directionEight = MapUtil.DIRECTION_EIGHT;
            var directionCount = directionEight.GetLength(0);
            for (int i = 0; i < directionCount; i++)
            {
                // walkable判断
                var neighborNode = new AStarNode(new Vector2Int(curNode.X + directionEight[i, 0], curNode.Y + directionEight[i, 1]));
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
        return null;
    }

    /// <summary>
    /// 倒序得到路径
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public List<Vector2Int> GenerateResultPath(AStarNode node)
    {
        var path = new List<Vector2Int>();
        while (node != null)
        {
            path.Add(node.Pos);
            node = node.Parent;
        }
        path.Reverse();
        return path;
    }

    public List<Vector2Int> PathOptimization(List<Vector2Int> path)
    {
        path = ReduceCollinearPoints(path);
        path = ThickLOSSmoothing(path);
        path = TrimStartPoint(path);
        return path;
    }

    /// <summary>
    /// step1.路径简化，共线点删除：斜率的多个点相同只保留两个端点，删掉多余点，大幅度减少后续计算量
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public List<Vector2Int> ReduceCollinearPoints(List<Vector2Int> path)
    {
        if (path == null || path.Count < 3)
        {
            return path;
        }
        var result = new List<Vector2Int>()
        {
            path[0]
        };

        var lastDir = path[1] - path[0];
        for (int i = 2; i < path.Count; i++)
        {
            var curDir = path[i] - path[i - 1];
            if (curDir != lastDir) // 因为连续点都是VectorInt，所以向量不等则方向不同
            {
                result.Add(path[i - 1]);
                lastDir = curDir;
            }
        }

        result.Add(path[path.Count - 1]);
        return result;
    }


    /// <summary>
    /// step2.贪心式LOS路径平滑：AC两点可以直达则可去掉中间点B，用Bresenham生成两点间的路径点同时考虑IsWalkable
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>

    public List<Vector2Int> ThickLOSSmoothing(List<Vector2Int> path)
    {
        if (path.Count <= 2)
        {
            return path;
        }

        var result = new List<Vector2Int>();
        int start = 0;
        var last = path.Count - 1;
        while (start < last)
        {
            result.Add(path[start]);
            int end = start + 1; // 至少+1避免死循环
            for (int target = last; target > start + 1; target--)
            {
                if (BresenhamLineCheck(path[start], path[target]))
                {
                    end = target;
                    break;
                }
            }
            start = end;
        }
        result.Add(path[last]);

        return result;
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
    public List<Vector2Int> TrimStartPoint(List<Vector2Int> path)
    {
        if (path.Count > 1)
        {
            path.RemoveAt(0);
        }
        return path;
    }
}
