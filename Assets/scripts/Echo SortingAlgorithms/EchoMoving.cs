using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;

public class EchoMoving : MonoBehaviour
{

    [Header("�̵� ����")]
    public float speed = 5f;
    [Tooltip("�ǰ��� �ð� (��)")]
    public float rewindSeconds = 3f;

    [Header("��Ƽ����")]
    public Material normalMaterial;
    public Material echoMaterial;

    [Header("UI ���")]
    public Button recordButton;
    public Button playButton;
    public TextMeshProUGUI queueCount;

    // ���� �ʵ�
    private Queue<(Vector3 pos, bool echo)> commandQueue = new Queue<(Vector3, bool)>();
    private Stack<Vector3> moveHistory;
    private bool isMoving = false;
    private bool isEchoing = false;
    private bool canRecode = false;
    private Vector3 targetPos;

    // �߰� �ʵ�: Ÿ�ӽ����� ��� ���
    private List<(Vector3 pos, float time)> timedHistory = new List<(Vector3, float)>();



    // Start is called before the first frame update
    void Start()
    {
        commandQueue = new Queue<(Vector3, bool)>();
        moveHistory = new Stack<Vector3>();
        timedHistory.Clear();
        targetPos = transform.position;
        recordButton.interactable = true;
        playButton.interactable = false;
        canRecode = false;
    }

    // Update is called once per frame
    void Update()
    {
        queueCount.text = "Queue Count : " + commandQueue.Count.ToString();
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if (canRecode)
        {
            if (x != 0 || y != 0)
            {
                Vector3 move = new Vector3(x, y, 0).normalized * speed * Time.deltaTime;
                targetPos += move;
                commandQueue.Enqueue((targetPos, false));
                moveHistory.Push(targetPos);
                timedHistory.Add((targetPos, Time.time));
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                for (int i = 0; i < 100; i++)
                {
                    if (moveHistory.Count > 0)
                    {
                        Vector3 lastPos = moveHistory.Pop();
                        commandQueue.Enqueue((lastPos, true));
                        targetPos = lastPos;
                    }
                }
                moveHistory = new Stack<Vector3>();
            }
        }
        if (isMoving)
        {
            if (commandQueue.Count > 0)
            {
                var cmd = commandQueue.Dequeue();

                transform.position = cmd.Item1;
                isEchoing = cmd.Item2;
                if (isEchoing)
                {
                    if (commandQueue.Count > 3)
                    {
                        commandQueue.Dequeue();
                        commandQueue.Dequeue();
                        commandQueue.Dequeue();
                    }
                }
                GetComponent<Renderer>().material =
                    isEchoing ? echoMaterial : normalMaterial;
                isMoving = true;
            }
            else
            {
                recordButton.interactable = true;
                isMoving = false;
                commandQueue = new Queue<(Vector3, bool)>();
                moveHistory = new Stack<Vector3>();
            }
        }
    }
    public void Recode()
    {
        timedHistory.Clear();
        moveHistory.Clear();
        commandQueue.Clear();

        canRecode = true; 
        isMoving = false;
        recordButton.interactable = false;
        playButton.interactable = true;
    }

    public void Play()
    {
        canRecode = false; 
        isMoving = true;
        recordButton.interactable = false;
        playButton.interactable = false;
    }
}