using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("미로 크기")]
    public int mapWidth = 31;
    public int mapHeight = 31;

    [Header("시도 횟수")]
    public int tryCount = 10000;

    [Header("벽 생성 확률")]
    [Range(0f, 1f)]
    public float wallRate = 0.3f;

    // 1=벽, 0=길 (아주 작은 예시 맵)
    bool[,] map;

    bool[,] visited;    // 방문기록
    Vector2Int goal = new Vector2Int(3, 3);         // 도착지 (3, 3)
    Vector2Int[] dirs = { new(1, 0), new(0, 1), new(-1, 0), new(0, -1) };        // 탐색 순서

    Coroutine generateRoutine;
    Coroutine showRoutine;
    List<Vector2Int> escapeRoute = new List<Vector2Int>();

    // Start is called before the first frame update
    void Start()
    {
        generateRoutine = StartCoroutine(TryGenerate());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (generateRoutine != null)
            {
                Debug.LogWarning("맵 생성 시도중, 잠시 후 재시도 희망");
                return;
            }

            if (showRoutine != null)
            {
                StopCoroutine(showRoutine);
                showRoutine = null;
            }

            generateRoutine = StartCoroutine(TryGenerate());
        }

        if (Input.GetKeyDown(KeyCode.R) && escapeRoute.Count > 0)        // R키를 누르고 탈출 경로가 존재 할때
        {
            if (showRoutine != null) return;
            showRoutine = StartCoroutine(ShowEscapeRoute());
        }
    }

    IEnumerator TryGenerate()
    {
        goal = new Vector2Int(mapWidth - 2, mapHeight - 2);

        int currentTry = 0;
        while (true)
        {
            currentTry++;
            visited = new bool[mapWidth, mapHeight];            // 방문 기록 초기화

            map = GenerateMaze();                               // 맵 새로 생성

            escapeRoute = new List<Vector2Int>();
            bool ok = SearchMaze(1, 1, escapeRoute);                         // 시작점 (1, 1)
            if (ok)
            {
                Debug.Log($"{currentTry}만큼 시도, 성공");
                ShowMaze(map);
                break;          // 출구 있으면 통과
            }
            else if (currentTry >= tryCount)
            {
                Debug.Log($"{tryCount}만큼 시도, 생성 실패");
                break;          // 출구 있으면 통과
            }

            Debug.Log($"{currentTry}만큼 시도");

            if (currentTry % 1000 == 0)     // 10000 번 마다 잠시 쉬기
                yield return null;
        }

        generateRoutine = null;
    }

    IEnumerator ShowEscapeRoute()
    {
        while (escapeRoute.Count > 0)
        {
            Vector2Int pos = escapeRoute[0];
            GameObject route = GameObject.CreatePrimitive(PrimitiveType.Cube);
            route.transform.SetParent(transform);
            route.transform.position = new Vector3(pos.x, 0.5f, pos.y);
            route.transform.localScale = Vector3.one * 0.6f;
            route.GetComponent<MeshRenderer>().material.color = Color.green;

            escapeRoute.Remove(pos);
            yield return new WaitForSeconds(0.1f);
        }

        showRoutine = null;
    }

    bool[,] GenerateMaze()
    {
        bool[,] m = new bool[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                m[x, y] = IsWall(x, y);
            }
        }

        return m;
    }

    void ClearMaze()
    {
        foreach (Transform c in transform)
        {
            Destroy(c.gameObject);
        }
    }

    void ShowMaze(bool[,] maze)
    {
        ClearMaze();

        for (int x = 0; x < maze.GetLength(0); x++)
        {
            for (int y = 0; y < maze.GetLength(1); y++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = $"Maze ({x}, {y})";
                cube.transform.SetParent(transform);

                if (maze[x, y])     // 벽이면
                {
                    cube.transform.position = new Vector3(x, 0.5f, y);
                    cube.transform.localScale = Vector3.one;
                    cube.GetComponent<MeshRenderer>().material.color = Color.gray;
                }
                else
                {
                    cube.transform.position = new Vector3(x, 0, y);
                    cube.transform.localScale = new Vector3(1, 0.1f, 1);
                    if (x == goal.x && y == goal.y)
                        cube.GetComponent<MeshRenderer>().material.color = Color.red;
                    else if (x == 1 && y == 1)
                        cube.GetComponent<MeshRenderer>().material.color = Color.green;
                    else
                        cube.GetComponent<MeshRenderer>().material.color = Color.white;
                }
            }
        }
    }

    bool IsWall(int x, int y)
    {
        if (x == 0 || y == 0 || x == mapWidth - 1 || y == mapHeight - 1) return true;       // 테두리는 벽
        if (x == 1 && y == 1) return false;                                         // 시작지점은 빈공간
        if (x == goal.x && y == goal.y) return false;                               // 골 지점은 빈공간

        return Random.value < wallRate;
    }

    bool SearchMaze(int x, int y, List<Vector2Int> path)
    {
        // 범위/벽/재방문 체크
        if (x < 0 || y < 0 || x >= map.GetLength(0) || y >= map.GetLength(1)) return false;
        if (map[x, y] || visited[x, y]) return false;

        // 방문 표시
        visited[x, y] = true;
        Debug.Log($"이동: ({x},{y})");
        path.Add(new Vector2Int(x, y));

        // 목표 도달?
        if (x == goal.x && y == goal.y) return true;

        // 4방향 재귀 탐색
        foreach (var d in dirs)
            if (SearchMaze(x + d.x, y + d.y, path)) return true;

        // 막혔으면 되돌아감
        Debug.Log($"되돌아감: ({x},{y})");
        path.RemoveAt(path.Count - 1);
        return false;
    }
}