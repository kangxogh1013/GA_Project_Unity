using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int mazeWidth = 21;      // 홀수 권장 (21, 31, 41 등)
    public int mazeHeight = 21;     // 홀수 권장

    private int[,] maze;            // 1=벽, 0=길
    private bool[,] visited;        // 방문 기록
    private List<Vector2Int> solutionPath; // 해답 경로
    private Vector2Int start = Vector2Int.zero;
    private Vector2Int goal = Vector2Int.zero;

    void Start()
    {
        GenerateNewMaze();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateNewMaze();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            FindAndDrawPath();
        }
    }

    void GenerateNewMaze()
    {
        // 맵 크기가 홀수인지 확인
        if (mazeWidth % 2 == 0) mazeWidth++;
        if (mazeHeight % 2 == 0) mazeHeight++;

        maze = new int[mazeHeight, mazeWidth];
        visited = new bool[mazeHeight, mazeWidth];
        solutionPath = new List<Vector2Int>();

        // 모든 셀을 벽으로 초기화
        InitializeMaze();

        // 미로 생성 (Recursive Backtracking 사용)
        GenerateMazeRecursive(1, 1);

        // 시작과 끝 포인트 설정
        start = new Vector2Int(1, 1);
        goal = new Vector2Int(mazeWidth - 2, mazeHeight - 2);

        maze[start.y, start.x] = 0;
        maze[goal.y, goal.x] = 0;

        // 탈출 가능 여부 확인
        visited = new bool[mazeHeight, mazeWidth];
        bool canEscape = SearchMaze(start.x, start.y, false);

        if (!canEscape)
        {
            Debug.Log("탈출 불가능한 미로 - 새로 생성합니다.");
            GenerateNewMaze(); // 재귀적으로 새 미로 생성
        }
        else
        {
            Debug.Log($"미로 생성됨: {mazeWidth}x{mazeHeight}");

            // 시각화 업데이트
            GetComponent<MazeVisualizer>().UpdateVisualization();
        }
    }

    void InitializeMaze()
    {
        for (int y = 0; y < mazeHeight; y++)
        {
            for (int x = 0; x < mazeWidth; x++)
            {
                maze[y, x] = 1; // 모두 벽으로
            }
        }
    }

    // Recursive Backtracking을 이용한 미로 생성
    void GenerateMazeRecursive(int x, int y)
    {
        maze[y, x] = 0; // 현재 셀을 길로 설정

        // 4가지 방향을 랜덤하게 섞기
        int[] directions = { 0, 1, 2, 3 }; // 0=상, 1=우, 2=하, 3=좌
        ShuffleArray(directions);

        foreach (int dir in directions)
        {
            int nx = x;
            int ny = y;

            // 2칸 앞의 좌표 계산
            switch (dir)
            {
                case 0: ny -= 2; break; // 위
                case 1: nx += 2; break; // 오른쪽
                case 2: ny += 2; break; // 아래
                case 3: nx -= 2; break; // 왼쪽
            }

            // 범위 체크 및 미방문 체크
            if (nx > 0 && ny > 0 && nx < mazeWidth - 1 && ny < mazeHeight - 1 && maze[ny, nx] == 1)
            {
                // 사이의 벽을 부수기
                maze[y + (ny - y) / 2, x + (nx - x) / 2] = 0;
                // 재귀 호출
                GenerateMazeRecursive(nx, ny);
            }
        }
    }

    // 배열 셔플 (Fisher-Yates)
    void ShuffleArray(int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randomIdx = Random.Range(0, i + 1);
            // Swap
            int temp = array[i];
            array[i] = array[randomIdx];
            array[randomIdx] = temp;
        }
    }

    // 백트래킹을 이용한 미로 탐색
    bool SearchMaze(int x, int y, bool drawPath)
    {
        // 범위/벽/재방문 체크
        if (x < 0 || y < 0 || x >= mazeWidth || y >= mazeHeight) return false;
        if (maze[y, x] == 1 || visited[y, x]) return false;

        // 방문 표시
        visited[y, x] = true;

        if (drawPath)
        {
            solutionPath.Add(new Vector2Int(x, y));
        }

        // 목표 도달?
        if (x == goal.x && y == goal.y) return true;

        // 4방향 탐색 (상, 우, 하, 좌)
        if (SearchMaze(x, y - 1, drawPath)) return true;  // 위
        if (SearchMaze(x + 1, y, drawPath)) return true;  // 오른쪽
        if (SearchMaze(x, y + 1, drawPath)) return true;  // 아래
        if (SearchMaze(x - 1, y, drawPath)) return true;  // 왼쪽

        // 막혔으면 경로에서 제거
        if (drawPath && solutionPath.Count > 0)
        {
            solutionPath.RemoveAt(solutionPath.Count - 1);
        }

        return false;
    }

    void FindAndDrawPath()
    {
        solutionPath.Clear();
        visited = new bool[mazeHeight, mazeWidth];
        bool found = SearchMaze(start.x, start.y, true);

        if (found)
        {
            Debug.Log($"경로 찾음! 길이: {solutionPath.Count}");
            GetComponent<MazeVisualizer>().UpdatePathVisualization();
        }
        else
        {
            Debug.Log("경로를 찾을 수 없습니다.");
        }
    }

    // 외부에서 미로 데이터 접근용
    public int GetCell(int x, int y)
    {
        if (x < 0 || y < 0 || x >= mazeWidth || y >= mazeHeight)
            return 1;
        return maze[y, x];
    }

    public List<Vector2Int> GetSolutionPath()
    {
        return solutionPath;
    }

    public Vector2Int GetStart() => start;
    public Vector2Int GetGoal() => goal;
    public int GetWidth() => mazeWidth;
    public int GetHeight() => mazeHeight;
}
