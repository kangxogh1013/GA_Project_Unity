using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewBehaviourScript : MonoBehaviour
{
    [Header("미로 설정")]
    public int width = 15;
    public int height = 15;

    [Header("시각화")]
    public GameObject cellPrefab;
    public Transform mazeParent;
    public float cellSize = 1f;
    public float moveSpeed = 0.2f;
    public Camera mainCamera;

    [Header("UI 요소")]
    public Button generateButton;
    public Button showPathButton;
    public Button autoMoveButton;

    // 셀 타입 정의
    private const int CELL_WALL = 1;
    private const int CELL_GROUND = 0;
    private const int CELL_FOREST = 3;
    private const int CELL_MUD = 5;

    private int[,] map;
    private Vector2Int start;
    private Vector2Int goal;
    private List<Vector2Int> shortestPath;
    private GameObject playerObject;
    private Dictionary<Vector2Int, GameObject> cellObjects = new Dictionary<Vector2Int, GameObject>();
    private List<GameObject> pathMarkers = new List<GameObject>();
    private Coroutine moveCoroutine;

    Vector2Int[] dirs = {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    void Start()
    {
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
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
            autoMoveButton.interactable = true;
        }

        if (width % 2 == 0) width++;
        if (height % 2 == 0) height++;

        int attempts = 0;

        while (attempts < 100)
        {
            attempts++;

            // 1. 맵 초기화 (모두 벽으로)
            map = new int[height, width];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    map[y, x] = CELL_WALL;

            // 2. 재귀적 미로 생성
            CreateMazeRecursive(1, 1);

            // 3. 랜덤 코스트 추가 (숲, 진흙)
            AddRandomCosts();

            // 4. 추가 경로 생성
            AddExtraPaths();

            // 5. 시작/목표 설정
            start = new Vector2Int(1, 1);
            goal = new Vector2Int(width - 2, height - 2);
            map[start.y, start.x] = CELL_GROUND;
            map[goal.y, goal.x] = CELL_GROUND;

            // 6. DFS로 탈출 가능 여부 확인
            if (CanEscapeDFS())
            {
                Debug.Log($"탈출 가능한 미로 생성 완료! ({attempts}회)");
                DrawMaze();
                AdjustCameraToMaze();
                showPathButton.interactable = true;
                autoMoveButton.interactable = true;
                return;
            }
        }

        Debug.LogError("미로 생성 실패!");
    }

    void CreateMazeRecursive(int x, int y)
    {
        map[y, x] = CELL_GROUND;

        // 방향 섞기
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

            if (nx > 0 && nx < width - 1 && ny > 0 && ny < height - 1 && map[ny, nx] == CELL_WALL)
            {
                map[y + dir.y, x + dir.x] = CELL_GROUND;
                CreateMazeRecursive(nx, ny);
            }
        }
    }

    void AddRandomCosts()
    {
        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                if (map[y, x] == CELL_GROUND)
                {
                    float r = Random.value;
                    if (r > 0.91f)
                        map[y, x] = CELL_MUD;
                    else if (r > 0.80f)
                        map[y, x] = CELL_FOREST;
                }
            }
        }
    }

    void AddExtraPaths()
    {
        int extraPaths = (width * height) / 7;

        for (int i = 0; i < extraPaths; i++)
        {
            int x = Random.Range(1, width - 1);
            int y = Random.Range(1, height - 1);

            if (map[y, x] == CELL_WALL && Random.value > 0.40f)
            {
                map[y, x] = CELL_GROUND;
            }
        }
    }

    bool CanEscapeDFS()
    {
        bool[,] visited = new bool[height, width];
        return DFS(start.x, start.y, visited);
    }

    bool DFS(int x, int y, bool[,] visited)
    {
        if (x == goal.x && y == goal.y) return true;
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        if (map[y, x] == CELL_WALL || visited[y, x]) return false;

        visited[y, x] = true;

        foreach (var dir in dirs)
            if (DFS(x + dir.x, y + dir.y, visited))
                return true;

        return false;
    }

    // Dijkstra 알고리즘 (우선순위 큐 기반)
    List<Vector2Int> FindPathDijkstra()
    {
        // 거리 배열 초기화
        float[,] dist = new float[height, width];
        Vector2Int?[,] parent = new Vector2Int?[height, width];

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                dist[y, x] = float.MaxValue;

        dist[start.y, start.x] = 0;

        // 우선순위 큐 (간단한 리스트로 구현)
        List<Node> openList = new List<Node>();
        openList.Add(new Node(start, 0));

        while (openList.Count > 0)
        {
            // 최소 코스트 노드 찾기
            openList.Sort((a, b) => a.cost.CompareTo(b.cost));
            Node current = openList[0];
            openList.RemoveAt(0);

            if (current.pos == goal)
            {
                return ReconstructPath(parent);
            }

            foreach (var d in dirs)
            {
                int nx = current.pos.x + d.x;
                int ny = current.pos.y + d.y;

                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

                int cellType = map[ny, nx];
                if (cellType == CELL_WALL) continue;

                // 코스트 계산
                float stepCost = (cellType == CELL_GROUND) ? 1f : cellType;
                float newDist = dist[current.pos.y, current.pos.x] + stepCost;

                if (newDist < dist[ny, nx])
                {
                    dist[ny, nx] = newDist;
                    parent[ny, nx] = current.pos;
                    openList.Add(new Node(new Vector2Int(nx, ny), newDist));
                }
            }
        }

        Debug.Log("Dijkstra: 경로 없음");
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
        Debug.Log($"Dijkstra 경로 길이: {path.Count}");
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
                int cellType = map[y, x];

                if (cellType == CELL_WALL)
                {
                    renderer.material.color = new Color(0.75f, 0.09f, 0.18f); // 빨강 (벽)
                    cell.transform.localScale = new Vector3(1, 1, 1);
                }
                else if (cellType == CELL_GROUND)
                {
                    renderer.material.color = Color.white; // 땅
                    cell.transform.localScale = new Vector3(1, 0.2f, 1);
                }
                else if (cellType == CELL_FOREST)
                {
                    renderer.material.color = new Color(0.13f, 0.50f, 0.55f); // 청록 (숲)
                    cell.transform.localScale = new Vector3(1, 0.4f, 1);
                }
                else if (cellType == CELL_MUD)
                {
                    renderer.material.color = new Color(0.66f, 0.29f, 0.18f); // 갈색 (진흙)
                    cell.transform.localScale = new Vector3(1, 0.3f, 1);
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
        shortestPath = FindPathDijkstra();

        if (shortestPath == null || shortestPath.Count == 0)
        {
            Debug.LogWarning("경로를 찾을 수 없습니다!");
            return;
        }

        // 기존 경로 마커 삭제
        foreach (var marker in pathMarkers)
        {
            if (marker != null)
                Destroy(marker);
        }
        pathMarkers.Clear();

        // 최단 경로 시각화
        for (int i = 1; i < shortestPath.Count - 1; i++)
        {
            Vector2Int pos = shortestPath[i];
            Vector3 cubePos = new Vector3(pos.x * cellSize, 0.3f, pos.y * cellSize);
            GameObject pathCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pathCube.transform.position = cubePos;
            pathCube.transform.localScale = new Vector3(0.8f, 0.4f, 0.8f);
            pathCube.transform.SetParent(mazeParent);
            pathCube.GetComponent<Renderer>().material.color = Color.green;
            pathMarkers.Add(pathCube);
        }

        Debug.Log("Dijkstra 최단 경로 시각화 완료!");
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

    // Dijkstra용 노드 클래스
    private class Node
    {
        public Vector2Int pos;
        public float cost;

        public Node(Vector2Int p, float c)
        {
            pos = p;
            cost = c;
        }
    }
}