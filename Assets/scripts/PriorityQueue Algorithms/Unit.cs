using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit
{
    public string name;
    public int speed;
    public float nextTurnTime; 

    public Unit(string name, int speed)
    {
        this.name = name;
        this.speed = speed;
        this.nextTurnTime = 0f;
    }

    public void ExecuteTurn(int turnOrder)
    {
        Debug.Log($"<color=yellow>[턴 실행 {turnOrder}번]</color> {name}이(가) 행동! (스피드: {speed})");
        nextTurnTime = 100f / speed;
    }
}
