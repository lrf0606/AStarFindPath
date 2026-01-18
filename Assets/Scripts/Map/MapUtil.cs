using UnityEngine;


// 和Editor中Layer保持一致
public static class LayerDefine
{
    public const int Layer_Ground = 10;
    public const int Layer_Obstacle = 11;
    public const int Layer_Hole = 12;
    public const int Layer_Player = 20;
    public const int Layer_Monster = 21;
}

public static class DirectionDefine
{
    public static readonly int[,] Four = { { 0, 1 }, { 0, -1 }, { -1, 0 }, { 1, 0 } };
    public static readonly int[,] Eight = { { 1, 1 }, { 1, -1 }, { 1, 0 }, { 0, 1 }, { 0, -1 }, { -1, 1 }, { -1, 0 }, { -1, -1 } };
}


[System.Flags]
public enum MapFlags
{
    None = 0,
    Ground = 1 << LayerDefine.Layer_Ground,
    Obstacle = 1 << LayerDefine.Layer_Obstacle,
    Hole = 1 << LayerDefine.Layer_Hole,
    Player = 1 << LayerDefine.Layer_Player,
    Monster = 1 << LayerDefine.Layer_Monster,
}


public static class MapUtil
{
    public static bool DEBUG_MAP = true;  // Debug显示格子数据
    public const float GRID_SIZE = 1.0f;

    public static Vector3 Grid2WorldPos(Vector2Int grid, float keepY)
    {
        return new Vector3(grid.x * GRID_SIZE + GRID_SIZE * 0.5f, keepY, grid.y * GRID_SIZE + GRID_SIZE * 0.5f);
    }

    public static Vector2Int WorldPos2Grid(Vector3 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x / GRID_SIZE), Mathf.FloorToInt(pos.z / GRID_SIZE));
    }

}
