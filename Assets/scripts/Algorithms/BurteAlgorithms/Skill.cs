using UnityEngine;

public class Skill : MonoBehaviour
{
    private int h;

    // Start is called before the first frame update
    void Start()
    {
        int bestDmg = -1;
        (int q, int h, int m, int t, int cost, int dmg) best = (0, 0, 0, 0, 0, 0);
        for (int q = 0; q <= 2; q++)
            for (int m = 0; m <= 1; m++)
                for (int t = 0; t <= 1; t++)
                {
                    int cost = 2 * q + 3 * h + 5 * m + 7 * t;
                    if (cost > 15) continue;

                    int dmg = 6 * q + 8 * h + 16 * m + 24 * t;
                    if (dmg > bestDmg)
                    {
                        bestDmg = dmg;
                        best = (q, h, m, t, cost, dmg);
                    }
                }
        Debug.Log($"BEST: 퀵{best.q} 헤비{best.h} 멀티{best.m} 트리플{best.t}, 코스트 {best.cost}/15 , 데미지 {best.dmg}");
    }
}
