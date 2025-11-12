using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeVisualizer : MonoBehaviour
{
    private MazeGenerator mazeGen;

    public float cellSize = 1f;

    // 색상
    private Material wallMaterial;
    private Material pathMaterial;
    private Material startMaterial;
    private Material goalMaterial;
    private Material solutionMaterial;

    private GameObject mazeContainer;
    private List<GameObject> mazeObjects = new List<GameObject>();
    private List<GameObject> pathObjects = new List<GameObject>();

    void Start()
    {
        mazeGen = GetComponent<MazeGenerator>();

        // 머티리얼 생성
        CreateMaterials();
    }

    void CreateMaterials()
    {
        wallMaterial = new Material(Shader.Find("Standard"));
        wallMaterial.color = Color.black;

        pathMaterial = new Material(Shader.Find("Standard"));
        pathMaterial.color = Color.white;

        startMaterial = new Material(Shader.Find("Standard"));
        startMaterial.color = Color.green;

        goalMaterial = new Material(Shader.Find("Standard"));
        goalMaterial.color = Color.red;

        solutionMaterial = new Material(Shader.Find("Standard"));
        solutionMaterial.color = new Color(1f, 1f, 0f, 1f); // 노란색
    }

    public void UpdateVisualization()
    {
        // 기존 미로 오브젝트 제거
        ClearMaze();

        // 새로운 컨테이너 생성
        mazeContainer = new GameObject("MazeContainer");
        mazeContainer.transform.parent = transform;
        mazeContainer.transform.localPosition = Vector3.zero;

        int width = mazeGen.GetWidth();
        int height = mazeGen.GetHeight();

        // 미로 그리기
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 pos = new Vector3(x * cellSize, 0, y * cellSize);

                GameObject cell = null;
                Material mat = null;

                if (x == mazeGen.GetStart().x && y == mazeGen.GetStart().y)
                {
                    // 시작점
                    cell = CreateCube(pos, "Start");
                    mat = startMaterial;
                }
                else if (x == mazeGen.GetGoal().x && y == mazeGen.GetGoal().y)
                {
                    // 목표점
                    cell = CreateCube(pos, "Goal");
                    mat = goalMaterial;
                }
                else if (mazeGen.GetCell(x, y) == 1)
                {
                    // 벽
                    cell = CreateCube(pos, "Wall");
                    mat = wallMaterial;
                }
                else
                {
                    // 길
                    cell = CreateCube(pos, "Path");
                    mat = pathMaterial;
                }

                if (cell != null && mat != null)
                {
                    cell.GetComponent<MeshRenderer>().material = mat;
                    cell.transform.parent = mazeContainer.transform;
                    mazeObjects.Add(cell);
                }
            }
        }
    }

    public void UpdatePathVisualization()
    {
        // 기존 경로 오브젝트 제거
        foreach (GameObject obj in pathObjects)
        {
            Destroy(obj);
        }
        pathObjects.Clear();

        // 새로운 경로 그리기
        List<Vector2Int> solutionPath = mazeGen.GetSolutionPath();
        foreach (var pos in solutionPath)
        {
            Vector3 pathPos = new Vector3(pos.x * cellSize, 0.1f, pos.y * cellSize);
            GameObject pathCell = CreateCube(pathPos, "SolutionPath");
            pathCell.GetComponent<MeshRenderer>().material = solutionMaterial;
            pathCell.transform.localScale = Vector3.one * cellSize * 0.8f;
            pathCell.transform.parent = mazeContainer.transform;
            pathObjects.Add(pathCell);
        }
    }

    GameObject CreateCube(Vector3 position, string name)
    {
        GameObject cube = new GameObject(name);
        cube.transform.position = position;
        cube.transform.localScale = Vector3.one * cellSize * 0.9f;

        // Mesh 추가
        MeshFilter meshFilter = cube.AddComponent<MeshFilter>();
        meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

        // Renderer 추가
        MeshRenderer renderer = cube.AddComponent<MeshRenderer>();

        // Collider 추가
        BoxCollider collider = cube.AddComponent<BoxCollider>();

        return cube;
    }

    void ClearMaze()
    {
        if (mazeContainer != null)
        {
            Destroy(mazeContainer);
        }
        mazeObjects.Clear();
        pathObjects.Clear();
    }

    void OnDestroy()
    {
        ClearMaze();
    }
}
