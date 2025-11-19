using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MazeGenerator : MonoBehaviour
{
    [Header("미로 설정")]
    public int width = 15;
    public int height = 15;

    [Header("시각화")]
    public GameObject cellPrefab;
    public Transform mazeParent;
    public float cellSize = 1f;
    public float moveSpeed = 0.2f;  // 이동 속도 (Inspector에서 조절)
    public Camera mainCamera;  // 메인 카메라

    [Header("UI 요소")]
    public Button generateButton;
    public Button showPathButton;
    public Button autoMoveButton;

    private int[,] map;
    private Vector2Int start;
    private Vector2Int goal;
    private List<Vector2Int> shortestPath;
    private GameObject playerObject;
    private Dictionary<Vector2Int, GameObject> cellObjects = new Dictionary<Vector2Int, GameObject>();
    private List<GameObject> pathMarkers = new List<GameObject>();  // 경로 표시 큐브들
    private Coroutine moveCoroutine;

    Vector2Int[] dirs = {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    void Start()
    {
        // 카메라 자동 찾기
        if (mainCamera == null)
            mainCamera = Camera.main;

        generateButton.onClick.AddListener(GenerateNewMaze);
        showPathButton.onClick.AddListener(ShowShortestPath);
        autoMoveButton.onClick.AddListener(AutoMove);

        showPathButton.interactable = false;
        autoMoveButton.interactable = false;
    }

    public void GenerateNewMaze()
    {
        // 이동 중이면 코루틴 중지
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
            autoMoveButton.interactable = true;
            Debug.Log("플레이어 이동 중지!");
        }

        if (width % 2 == 0) width++;
        if (height % 2 == 0) height++;

        int attempts = 0;

        while (attempts < 100)
        {
            attempts++;

            map = new int[height, width];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    map[y, x] = 1;

            CreateComplexMaze(1, 1);
            AddManyRandomPaths();

            start = new Vector2Int(1, 1);
            goal = new Vector2Int(width - 2, height - 2);
            map[start.y, start.x] = 0;
            map[goal.y, goal.x] = 0;

            if (CanEscape())
            {
                Debug.Log($"탈출 가능한 미로 생성 완료! ({attempts}회)");
                shortestPath = FindPathBFS();
                DrawMaze();
                AdjustCameraToMaze();
                showPathButton.interactable = true;
                autoMoveButton.interactable = true;
                return;
            }
        }

        Debug.LogError("미로 생성 실패!");
    }

    void AdjustCameraToMaze()
    {
        if (mainCamera == null) return;

        float centerX = (width - 1) * cellSize * 0.5f;
        float centerZ = (height - 1) * cellSize * 0.5f;
        float maxSize = Mathf.Max(width, height);
        float cameraHeight = maxSize * cellSize * 0.8f;
        float cameraDistance = maxSize * cellSize * 0.6f;

        mainCamera.transform.position = new Vector3(centerX, cameraHeight, centerZ - cameraDistance);
        mainCamera.transform.LookAt(new Vector3(centerX, 0, centerZ));
    }

    void CreateComplexMaze(int x, int y)
    {
        map[y, x] = 0;

        List<Vector2Int> directions = new List<Vector2Int>(dirs);
        for (int i = 0; i < directions.Count; i++)
        {
            int j = Random.Range(i, directions.Count);
            Vector2Int temp = directions[i];
            directions[i] = directions[j];
            directions[j] = temp;
        }

        foreach (var dir in directions)
        {
            int nx = x + dir.x * 2;
            int ny = y + dir.y * 2;

            if (nx > 0 && nx < width - 1 && ny > 0 && ny < height - 1 && map[ny, nx] == 1)
            {
                map[y + dir.y, x + dir.x] = 0;
                CreateComplexMaze(nx, ny);
            }
        }
    }

    void AddManyRandomPaths()
    {
        int extraPaths = (width * height) / 8;

        for (int i = 0; i < extraPaths; i++)
        {
            int x = Random.Range(1, width - 1);
            int y = Random.Range(1, height - 1);

            if (map[y, x] == 1)
            {
                if (Random.value > 0.3f)
                {
                    map[y, x] = 0;

                    if (Random.value > 0.5f)
                    {
                        Vector2Int randomDir = dirs[Random.Range(0, dirs.Length)];
                        int nx = x + randomDir.x;
                        int ny = y + randomDir.y;

                        if (nx > 0 && nx < width - 1 && ny > 0 && ny < height - 1)
                            map[ny, nx] = 0;
                    }
                }
            }
        }

        for (int y = 2; y < height - 2; y++)
        {
            for (int x = 2; x < width - 2; x++)
            {
                if (map[y, x] == 0 && Random.value > 0.85f)
                {
                    Vector2Int randomDir = dirs[Random.Range(0, dirs.Length)];
                    int nx = x + randomDir.x;
                    int ny = y + randomDir.y;

                    if (nx > 0 && nx < width - 1 && ny > 0 && ny < height - 1 && map[ny, nx] == 1)
                        map[ny, nx] = 0;
                }
            }
        }
    }

    bool CanEscape()
    {
        bool[,] visited = new bool[height, width];
        return DFS(start.x, start.y, visited);
    }

    bool DFS(int x, int y, bool[,] visited)
    {
        if (x == goal.x && y == goal.y) return true;
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        if (map[y, x] == 1 || visited[y, x]) return false;

        visited[y, x] = true;

        foreach (var dir in dirs)
            if (DFS(x + dir.x, y + dir.y, visited))
                return true;

        return false;
    }

    List<Vector2Int> FindPathBFS()
    {
        bool[,] visited = new bool[height, width];
        Vector2Int?[,] parent = new Vector2Int?[height, width];
        Queue<Vector2Int> q = new Queue<Vector2Int>();

        q.Enqueue(start);
        visited[start.y, start.x] = true;

        while (q.Count > 0)
        {
            Vector2Int cur = q.Dequeue();

            if (cur == goal)
            {
                Debug.Log("BFS: Goal 도착!");
                return ReconstructPath(parent);
            }

            foreach (var d in dirs)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;

                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                if (map[ny, nx] == 1) continue;
                if (visited[ny, nx]) continue;

                visited[ny, nx] = true;
                parent[ny, nx] = cur;
                q.Enqueue(new Vector2Int(nx, ny));
            }
        }

        Debug.Log("BFS: 경로 없음");
        return null;
    }

    List<Vector2Int> ReconstructPath(Vector2Int?[,] parent)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int? cur = goal;

        while (cur.HasValue)
        {
            path.Add(cur.Value);
            cur = parent[cur.Value.y, cur.Value.x];
        }

        path.Reverse();
        Debug.Log($"경로 길이: {path.Count}");
        return path;
    }

    void DrawMaze()
    {
        foreach (Transform child in mazeParent)
            Destroy(child.gameObject);
        cellObjects.Clear();
        pathMarkers.Clear();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 pos = new Vector3(x * cellSize, 0, y * cellSize);
                GameObject cell = Instantiate(cellPrefab, pos, Quaternion.identity, mazeParent);
                cellObjects[new Vector2Int(x, y)] = cell;

                Renderer renderer = cell.GetComponent<Renderer>();

                if (map[y, x] == 1)
                {
                    renderer.material.color = Color.red;
                    cell.transform.localScale = new Vector3(1, 1, 1);
                }
                else
                {
                    renderer.material.color = Color.white;
                    cell.transform.localScale = new Vector3(1, 0.2f, 1);
                }

                if (x == start.x && y == start.y)
                    renderer.material.color = Color.cyan;

                if (x == goal.x && y == goal.y)
                    renderer.material.color = Color.yellow;
            }
        }

        CreatePlayer();
    }

    void CreatePlayer()
    {
        if (playerObject != null)
            Destroy(playerObject);

        Vector3 pos = new Vector3(start.x * cellSize, 0.6f, start.y * cellSize);
        playerObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        playerObject.transform.position = pos;
        playerObject.transform.localScale = Vector3.one * 0.7f;
        playerObject.transform.SetParent(mazeParent);
        playerObject.GetComponent<Renderer>().material.color = new Color(0.8f, 0.5f, 0.2f);
    }

    public void ShowShortestPath()
    {
        if (shortestPath == null || shortestPath.Count == 0) return;

        // 기존 경로 마커 삭제
        foreach (var marker in pathMarkers)
        {
            if (marker != null)
                Destroy(marker);
        }
        pathMarkers.Clear();

        // 최단 경로를 초록색 큐브로 표시 (사진처럼 입체적으로)
        for (int i = 1; i < shortestPath.Count - 1; i++)
        {
            Vector2Int pos = shortestPath[i];
            Vector3 cubePos = new Vector3(pos.x * cellSize, 0.3f, pos.y * cellSize);
            GameObject pathCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pathCube.transform.position = cubePos;
            pathCube.transform.localScale = new Vector3(0.8f, 0.4f, 0.8f);  // 살짝 작고 높게
            pathCube.transform.SetParent(mazeParent);
            pathCube.GetComponent<Renderer>().material.color = Color.green;
            pathMarkers.Add(pathCube);
        }

        Debug.Log("최단 경로 시각화 완료!");
    }

    public void AutoMove()
    {
        if (shortestPath == null || playerObject == null) return;

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        moveCoroutine = StartCoroutine(MovePlayerAlongPath());
    }

    IEnumerator MovePlayerAlongPath()
    {
        autoMoveButton.interactable = false;

        foreach (var pos in shortestPath)
        {
            Vector3 targetPos = new Vector3(pos.x * cellSize, 0.6f, pos.y * cellSize);
            float elapsed = 0f;
            Vector3 startPos = playerObject.transform.position;

            while (elapsed < moveSpeed)
            {
                playerObject.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / moveSpeed);
                elapsed += Time.deltaTime;
                yield return null;
            }

            playerObject.transform.position = targetPos;
        }

        Debug.Log("목표 지점 도착!");
        moveCoroutine = null;
        autoMoveButton.interactable = true;
    }
}
