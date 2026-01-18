using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum MapFlags
{
    None = 0,
    Ground = 1 << 0,
    Wall = 1 << 1,
    Obstacle = 1 << 2,
    Hole = 1 << 3,
    Player = 1 << 4,
    Monster = 1 << 5,
}

public static class MapUtil
{
    public static bool DEBUG_MAP = false;  // 鏄剧ず鍚勭layer瀵瑰簲鏍煎瓙鑼冨洿
    public static readonly int[,] DIRECTION_FOUR = { { 0, 1 }, { 0, -1 }, { -1, 0 }, { 1, 0 } };
    public static readonly int[,] DIRECTION_EIGHT = { { 1, 1 }, { 1, -1 }, { 1, 0 }, { 0, 1 }, { 0, -1 }, { -1, 1 }, { -1, 0 }, { -1, -1 } };

    private static readonly Dictionary<int, MapFlags> m_Layer2MapFlags = new()
    {
        {LayerMask.NameToLayer("Ground"), MapFlags.Ground },
        {LayerMask.NameToLayer("Wall"), MapFlags.Wall },
        {LayerMask.NameToLayer("Obstacle"), MapFlags.Obstacle },
        {LayerMask.NameToLayer("Hole"), MapFlags.Hole },
        {LayerMask.NameToLayer("Player"), MapFlags.Player },
        {LayerMask.NameToLayer("Monster"), MapFlags.Monster },
    };

    public const float GRID_SIZE = 1.0f;

    public static Vector3 Grid2WorldPos(Vector2Int grid, float keepY)
    {
        return new Vector3(grid.x * GRID_SIZE + GRID_SIZE * 0.5f, keepY, grid.y * GRID_SIZE + GRID_SIZE * 0.5f);
    }

    public static Vector2Int WorldPos2Grid(Vector3 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x / GRID_SIZE), Mathf.FloorToInt(pos.z / GRID_SIZE));
    }

    public static MapFlags Layer2MapFlags(int layer)
    {
        if (m_Layer2MapFlags.TryGetValue(layer, out var mapFlags))
        {
            return mapFlags;
        }
        return MapFlags.None;
    }
}