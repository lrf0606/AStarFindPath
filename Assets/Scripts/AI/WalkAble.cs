using UnityEngine;

public interface IWalkable
{
    public bool IsWalkable(int x, int y);
}

public class CompositeWalkable : IWalkable
{
    private int m_Radius;

    public CompositeWalkable(float radius)
    {
        m_Radius = Mathf.CeilToInt(radius / MapUtil.GRID_SIZE);
    }


    private bool CanStand(int x, int y)
    {
        if (!MapData.Instance.HasFlag(x, y, MapFlags.Ground))  // 必须行走在Ground上
        {
            return false;
        }
        if (MapData.Instance.HasFlag(x, y, MapFlags.Obstacle)) // 有Obstacle无法通过
        {
            return false;
        }
        return true;
    }

    public bool IsWalkable(int x, int y)
    {
        if (m_Radius > 0)
        {
            // 半径扩张，当半径为(0,1]个GRID_SIZE时，占3*3格子，(1,2]时占5*5格子
            for (int dx = -m_Radius; dx <= m_Radius; dx++)
            {
                for (int dy = -m_Radius; dy <= m_Radius; dy++)
                {
                    if (!CanStand(x + dx, y + dy))
                    {
                        return false;
                    }
                }
            }
        }
        else
        {
            // 半径为0时，无法进行半径扩张，取上下左右四个格子
            var directionFour = MapUtil.DIRECTION_FOUR;
            for (int i = 0; i < directionFour.GetLength(0); i++)
            {
                if (!CanStand(x + directionFour[i, 0], y + directionFour[i, 1]))
                {
                    return false;
                }
            }
        }
        return true;
    }
}
