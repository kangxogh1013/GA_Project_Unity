using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class AICardSequenceCombo : MonoBehaviour
{
    public Button startButton;
    public Text resultText;
    Coroutine runningRoutine;

    void Start()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
    }

    public void OnStartButtonClicked()
    {
        if (runningRoutine != null) return;
        runningRoutine = StartCoroutine(BruteForceRoutine());
    }

    IEnumerator BruteForceRoutine()
    {
        Debug.Log("시뮬레이션 시작");
        Stopwatch sw = new Stopwatch();
        sw.Start();

        int tryCount = 0;
        int[] damages = { 6, 6, 8, 8, 16, 24 };
        int[] costs = { 2, 2, 3, 3, 5, 7 };

        for (int i = 0; i < 64; i++)
        {
            int totalDamage = 0, totalCost = 0;
            for (int j = 0; j < 6; j++)
            {
                if ((i & (1 << j)) != 0) { totalDamage += damages[j]; totalCost += costs[j]; }
            }
            tryCount++;
            if (totalDamage == 48 && totalCost <= 15)
            {
                sw.Stop();
                string result = $"성공! 시도={tryCount} 데미지={totalDamage} 코스트={totalCost} 소요={sw.Elapsed.TotalSeconds:F3}초";
                Debug.Log(result);
                if (resultText != null)
                    resultText.text = result;
                runningRoutine = null;
                yield break;
            }
        }
        sw.Stop();
        Debug.Log($"실패. 시도={tryCount} 소요={sw.Elapsed.TotalSeconds:F3}초");
        runningRoutine = null;
    }
}
