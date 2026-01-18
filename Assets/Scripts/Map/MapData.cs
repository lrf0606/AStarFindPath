using System;
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

    private Dictionary<Vector2Int, MapFlags> m_MapFlagsDict;
    private (int minX, int minY, int maxX, int maxY) m_MapRange;
    public (int minX, int minY, int maxX, int maxY) MapRange => m_MapRange;

    private MapData()
    {
        m_MapFlagsDict = new Dictionary<Vector2Int, MapFlags>();
        m_MapRange = new(0, 0, 0, 0);
    }

    public MapFlags GetFlags(int x, int y)
    {
        if (m_MapFlagsDict.TryGetValue(new Vector2Int(x, y), out var flags))
        {
            return flags;
        }
        return MapFlags.None;
    }

    public bool HasFlag(int x, int y, MapFlags flag)
    {
        return (GetFlags(x, y) & flag) != 0;
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
        m_MapRange.minX = Mathf.Min(minGrid.x, m_MapRange.Item1);
        m_MapRange.minY = Mathf.Min(minGrid.y, m_MapRange.Item2);
        m_MapRange.maxX = Mathf.Max(maxGrid.x, m_MapRange.Item3);
        m_MapRange.maxY = Mathf.Max(maxGrid.y, m_MapRange.Item4);
    }
}

