using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStExample : MonoBehaviour
{
    int[,] map =
    {
        {0,0,0,0,0,0,0,0,0,0},
        {0,1,1,1,1,1,1,1,1,0},
        {0,1,0,0,0,1,0,0,1,0},
        {0,1,0,2,1,1,1,0,1,0},
        {0,1,1,1,1,0,1,0,1,0},
        {0,1,0,0,1,0,1,1,1,0},
        {0,1,0,1,1,1,3,0,1,0},
        {0,1,1,1,0,1,1,1,1,0},
        {0,1,0,1,1,1,0,0,1,0},
        {0,0,0,0,0,0,0,0,0,0}
    };

    Vector2Int start = new Vector2Int(1, 1);
    Vector2Int goal = new Vector2Int(8, 8);

    void Start()
    {
        var path = AStarWallAvoid(map, start, goal);
        if (path == null)
            Debug.Log("경로 없음");
        else
        {
            Debug.Log("=== 회피 A* 경로 ===");
            foreach (var p in path)
                Debug.Log($"({p.x}, {p.y})");
            Debug.Log($"총 경로 길이: {path.Count}칸");
        }
    }

    List<Vector2Int> AStarWallAvoid(int[,] map, Vector2Int start, Vector2Int goal)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);

        int[,] gCost = new int[w, h];
        bool[,] visited = new bool[w, h];
        Vector2Int?[,] parent = new Vector2Int?[w, h];

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                gCost[x, y] = int.MaxValue;

        gCost[start.x, start.y] = 0;

        List<Vector2Int> open = new List<Vector2Int>();
        open.Add(start);

        Vector2Int[] dirs = {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        while (open.Count > 0)
        {
            int bestIndex = 0;
            int bestF = F_WallAvoid(open[0], gCost, goal, map);
            for (int i = 1; i < open.Count; i++)
            {
                int f = F_WallAvoid(open[i], gCost, goal, map);
                if (f < bestF)
                {
                    bestF = f;
                    bestIndex = i;
                }
            }

            Vector2Int cur = open[bestIndex];
            open.RemoveAt(bestIndex);

            if (visited[cur.x, cur.y]) continue;
            visited[cur.x, cur.y] = true;

            if (cur == goal)
                return Reconstruct(parent, start, goal);

            foreach (var d in dirs)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;

                if (!InBounds(map, nx, ny)) continue;
                if (map[nx, ny] == 0) continue;
                if (visited[nx, ny]) continue;

                int moveCost = TileCost(map[nx, ny]);
                int newG = gCost[cur.x, cur.y] + moveCost;

                if (newG < gCost[nx, ny])
                {
                    gCost[nx, ny] = newG;
                    parent[nx, ny] = cur;

                    Vector2Int next = new Vector2Int(nx, ny);
                    if (!open.Contains(next))
                        open.Add(next);
                }
            }
        }
        return null;
    }

    int F_WallAvoid(Vector2Int pos, int[,] gCost, Vector2Int goal, int[,] map)
    {
        return gCost[pos.x, pos.y] + H_WallAvoid(pos, goal, map);
    }
    int H_WallAvoid(Vector2Int a, Vector2Int b, int[,] map)
    {
        int h = Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        int wallPenalty = 0;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = a.x + dx;
                int ny = a.y + dy;

                if (InBounds(map, nx, ny) && map[nx, ny] == 0)
                {
                    wallPenalty += 2;
                }
            }
        }

        return h + wallPenalty;
    }

    int TileCost(int tile)
    {
        switch (tile)
        {
            case 1: return 1;
            case 2: return 3;
            case 3: return 5;
            default: return int.MaxValue;
        }
    }

    bool InBounds(int[,] map, int x, int y)
    {
        return x >= 0 && y >= 0 &&
               x < map.GetLength(0) &&
               y < map.GetLength(1);
    }

    List<Vector2Int> Reconstruct(Vector2Int?[,] parent, Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int? cur = goal;

        while (cur.HasValue)
        {
            path.Add(cur.Value);
            if (cur.Value == start) break;
            cur = parent[cur.Value.x, cur.Value.y];
        }

        path.Reverse();
        return path;
    }
}