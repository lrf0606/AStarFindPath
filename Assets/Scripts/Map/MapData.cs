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

    private MapData()
    {
        m_MapFlagsDict = new Dictionary<Vector2Int, MapFlags>();
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
}

