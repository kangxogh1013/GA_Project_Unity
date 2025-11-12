using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnBasedCombat : MonoBehaviour
{
    private List<Unit> units = new List<Unit>();
    private SimplePriorityQueue<Unit> turnQueue;
    private float currentTime = 0f;
    private int turnCount = 0;

    void Start()
    {
        units.Add(new Unit("전사", 5));
        units.Add(new Unit("마법사", 7));
        units.Add(new Unit("궁수", 10));
        units.Add(new Unit("도적", 12));

        Debug.Log("=== 턴제 전투 시작 ===");
        Debug.Log("스페이스바를 누르면 다음 턴이 실행됩니다.");

        InitializeTurnQueue();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ExecuteNextTurn();
        }
    }

    void InitializeTurnQueue()
    {
        turnQueue = new SimplePriorityQueue<Unit>();

        foreach (var unit in units)
        {
            float priority = unit.nextTurnTime + (-unit.speed * 0.0001f);
            turnQueue.Enqueue(unit, priority);
        }

        Debug.Log("<color=cyan>[초기화]</color> 모든 유닛이 전투 준비 완료!");
    }

    void ExecuteNextTurn()
    {
        if (turnQueue.Count == 0) return;
        turnCount++;

        Unit currentUnit = turnQueue.Dequeue();
        currentTime = currentUnit.nextTurnTime;

        currentUnit.ExecuteTurn(turnCount);

        currentUnit.nextTurnTime = currentTime + (100f / currentUnit.speed);
        float priority = currentUnit.nextTurnTime + (-currentUnit.speed * 0.0001f);
        turnQueue.Enqueue(currentUnit, priority);
    }
}
