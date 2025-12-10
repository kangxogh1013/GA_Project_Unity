using System.Collections.Generic;
using UnityEngine;

public class Assignment : MonoBehaviour
{
    public int width = 21;
    public int height = 21;
    [Range(0, 50)] public int wallPercent = 20;
    public int enemyCount = 5;

    public bool avoidWalls = false;      // 과제 1: 벽 회피
    public bool avoidEnemies = false;    // 과제 2: 적 회피

    int[,] map;
    Vector2Int startPos;
    Vector2Int goalPos;
    List<Vector2Int> enemies = new List<Vector2Int>();

    Transform mapContainer;
    Transform pathContainer;

    void Start()
    {
        do { GenerateMap(); } while (!IsSolvable());
        SpawnEnemies();
        DrawMap();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var path = AStar(map, startPos, goalPos);
            DrawPath(path);
        }
    }

    void GenerateMap()
    {
        map = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    map[x, y] = 0;
                    continue;
                }

                int rand = Random.Range(0, 100);
                if (rand < wallPercent) map[x, y] = 0;           // 벽
                else if (rand < wallPercent + 50) map[x, y] = 1; // 땅
                else if (rand < wallPercent + 80) map[x, y] = 2; // 숲
                else map[x, y] = 3;                              // 진흙
            }
        }
        startPos = new Vector2Int(1, 1);
        goalPos = new Vector2Int(width - 2, height - 2);
        map[startPos.x, startPos.y] = 1;
        map[goalPos.x, goalPos.y] = 1;
    }

    void SpawnEnemies()
    {
        enemies.Clear();
        int spawned = 0;
        while (spawned < enemyCount)
        {
            int x = Random.Range(1, width - 1);
            int y = Random.Range(1, height - 1);

            if (map[x, y] != 0 && new Vector2Int(x, y) != startPos &&
                new Vector2Int(x, y) != goalPos && !enemies.Contains(new Vector2Int(x, y)))
            {
                enemies.Add(new Vector2Int(x, y));
                spawned++;
            }
        }
    }

    // DFS를 사용한 탈출 가능 여부 판정 (과제 요구사항)
    bool IsSolvable()
    {
        bool[,] visited = new bool[width, height];
        return DFS(startPos, visited);
    }

    bool DFS(Vector2Int pos, bool[,] visited)
    {
        if (pos == goalPos) return true;
        if (visited[pos.x, pos.y]) return false;

        visited[pos.x, pos.y] = true;
        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int nx = pos.x + dx[i];
            int ny = pos.y + dy[i];

            if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
            if (map[nx, ny] == 0) continue; // 벽은 통과 불가

            if (DFS(new Vector2Int(nx, ny), visited))
                return true;
        }
        return false;
    }

    List<Vector2Int> AStar(int[,] map, Vector2Int start, Vector2Int goal)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);

        int[,] gCost = new int[w, h];
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                gCost[x, y] = int.MaxValue;

        gCost[start.x, start.y] = 0;

        // F값(gCost + heuristic)으로 정렬된 리스트
        List<Vector2Int> openSet = new List<Vector2Int> { start };
        Vector2Int?[,] parent = new Vector2Int?[w, h];
        bool[,] closedSet = new bool[w, h];

        Vector2Int[] dirs = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        while (openSet.Count > 0)
        {
            // F값이 가장 작은 노드 찾기
            int bestIdx = 0;
            int bestF = GetF(openSet[0], gCost, goal);

            for (int i = 1; i < openSet.Count; i++)
            {
                int f = GetF(openSet[i], gCost, goal);
                if (f < bestF)
                {
                    bestF = f;
                    bestIdx = i;
                }
            }

            Vector2Int current = openSet[bestIdx];
            openSet.RemoveAt(bestIdx);

            if (current == goal)
                return Reconstruct(parent, start, goal);

            closedSet[current.x, current.y] = true;

            // 이웃 노드 탐색
            foreach (var dir in dirs)
            {
                int nx = current.x + dir.x;
                int ny = current.y + dir.y;

                if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                if (map[nx, ny] == 0) continue;           // 벽 통과 불가
                if (closedSet[nx, ny]) continue;

                Vector2Int neighbor = new Vector2Int(nx, ny);
                int moveCost = GetTileCost(map[nx, ny]);
                int newG = gCost[current.x, current.y] + moveCost;

                // 더 좋은 경로 발견
                if (newG < gCost[nx, ny])
                {
                    gCost[nx, ny] = newG;
                    parent[nx, ny] = current;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        Debug.Log("경로를 찾을 수 없습니다!");
        return null;
    }

    // F = G + H 계산
    int GetF(Vector2Int pos, int[,] gCost, Vector2Int goal)
    {
        return gCost[pos.x, pos.y] + GetHeuristic(pos, goal);
    }

    // 커스텀 휴리스틱 함수
    int GetHeuristic(Vector2Int current, Vector2Int goal)
    {
        // 맨하탄 거리
        int h = Mathf.Abs(current.x - goal.x) + Mathf.Abs(current.y - goal.y);

        // 과제 1: 벽 근처 회피 (벽에서 가까우면 비용 증가)
        if (avoidWalls)
        {
            int distToWall = GetDistanceToWall(current);
            if (distToWall < 3)
                h += (3 - distToWall) * 2; // 벽 근처일수록 비용 증가
        }

        // 과제 2: 적 회피
        if (avoidEnemies)
        {
            foreach (var enemy in enemies)
            {
                float distToEnemy = Vector2Int.Distance(current, enemy);

                if (distToEnemy < 5f)
                {
                    // 적과의 거리가 가까울수록 비용 증가
                    // 거리 0 (적 위치): 20, 거리 5: ~4
                    h += (int)(20f / (distToEnemy + 0.5f));
                }
            }
        }

        return h;
    }

    // 현재 위치에서 벽까지의 최단 거리
    int GetDistanceToWall(Vector2Int pos)
    {
        int minDist = int.MaxValue;

        // 4방향으로 벽까지의 거리 계산
        for (int x = pos.x - 3; x <= pos.x + 3; x++)
        {
            for (int y = pos.y - 3; y <= pos.y + 3; y++)
            {
                if (x < 0 || y < 0 || x >= map.GetLength(0) || y >= map.GetLength(1))
                    continue;

                if (map[x, y] == 0)
                {
                    int dist = Mathf.Abs(pos.x - x) + Mathf.Abs(pos.y - y);
                    minDist = Mathf.Min(minDist, dist);
                }
            }
        }

        return minDist == int.MaxValue ? 10 : minDist;
    }

    int GetTileCost(int type)
    {
        return type switch
        {
            1 => 1,     // 땅
            2 => 3,     // 숲
            3 => 5,     // 진흙
            _ => 999    // 벽 (도달 불가)
        };
    }

    List<Vector2Int> Reconstruct(Vector2Int?[,] parent, Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int? current = goal;

        while (current.HasValue)
        {
            path.Add(current.Value);
            if (current.Value == start) break;
            current = parent[current.Value.x, current.Value.y];
        }

        path.Reverse();
        return path;
    }

    void DrawMap()
    {
        if (mapContainer != null) Destroy(mapContainer.gameObject);
        mapContainer = new GameObject("Map Container").transform;
        mapContainer.parent = transform;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = new Vector3(x, 0, y);
                cube.transform.parent = mapContainer;

                Renderer rend = cube.GetComponent<Renderer>();
                rend.material.color = map[x, y] switch
                {
                    0 => Color.black,                          // 벽
                    1 => Color.white,                          // 땅
                    2 => new Color(0.2f, 0.8f, 0.2f),        // 숲
                    3 => new Color(0.6f, 0.4f, 0.2f),        // 진흙
                    _ => Color.gray
                };

                // Start와 Goal 표시
                if (new Vector2Int(x, y) == startPos)
                    rend.material.color = Color.blue;
                else if (new Vector2Int(x, y) == goalPos)
                    rend.material.color = Color.green;
            }
        }

        // 적 표시
        foreach (var enemy in enemies)
        {
            GameObject enemyCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemyCube.transform.position = new Vector3(enemy.x, 1f, enemy.y);
            enemyCube.transform.localScale = Vector3.one * 0.8f;
            enemyCube.transform.parent = mapContainer;
            enemyCube.GetComponent<Renderer>().material.color = Color.red;
            enemyCube.name = "Enemy";
        }
    }

    void DrawPath(List<Vector2Int> path)
    {
        if (path == null || path.Count == 0) return;

        if (pathContainer != null) Destroy(pathContainer.gameObject);
        pathContainer = new GameObject("Path Container").transform;
        pathContainer.parent = transform;

        foreach (var p in path)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = new Vector3(p.x, 0.5f, p.y);
            cube.transform.localScale = Vector3.one * 0.5f;
            cube.transform.parent = pathContainer;
            cube.GetComponent<Renderer>().material.color = Color.cyan;
        }
    }
}
