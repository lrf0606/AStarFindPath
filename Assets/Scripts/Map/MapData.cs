using System.Collections.Generic;
using UnityEngine;


public class MapData
{
    private static MapData m_Instance;
    public static MapData Instance 
    { 
        get 
        {
            m_Instance ??= new MapData();
            return m_Instance; 
        } 
    }


    private (int minX, int minY, int maxX, int maxY) m_MapRange;
    public (int minX, int minY, int maxX, int maxY) MapRange => m_MapRange;


    private Dictionary<Vector2Int, MapFlags> m_MapFlagsDict;

    private MapData()
    {
        m_MapRange = new(0, 0, 0, 0);
        m_MapFlagsDict = new Dictionary<Vector2Int, MapFlags>();
    }

    public MapFlags GetFlags(int x, int y)
    {
        var key = new Vector2Int(x, y);
        if (m_MapFlagsDict.TryGetValue(key, out var flags))
        {
            return flags;
        }
        return MapFlags.None;
    }


    public void AddFlag(int x, int y, MapFlags flag)
    {
        var key = new Vector2Int(x, y);
        if (m_MapFlagsDict.TryGetValue(key, out var flags))
        {
            m_MapFlagsDict[key] = flags | flag;
        }
        else
        {
            m_MapFlagsDict[key] = flag;
        }
    }

    public void RemoveFlag(int x, int y, MapFlags flag)
    {
        var key = new Vector2Int(x, y);
        if (m_MapFlagsDict.TryGetValue(key, out var flags))
        {
            flags &= ~flag;
            if (flag == MapFlags.None)
            {
                m_MapFlagsDict.Remove(key);
            }
            else
            {
                m_MapFlagsDict[key] = flags;
            }
        }
    }

    public void UpdateMapRange(Vector2Int minGrid, Vector2Int maxGrid)
    {
        m_MapRange.minX = Mathf.Min(minGrid.x, m_MapRange.minX);
        m_MapRange.minY = Mathf.Min(minGrid.y, m_MapRange.minY);
        m_MapRange.maxX = Mathf.Max(maxGrid.x, m_MapRange.maxX);
        m_MapRange.maxY = Mathf.Max(maxGrid.y, m_MapRange.maxY);
    }
}

