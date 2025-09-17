using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleSortTest : MonoBehaviour
{
    public void Start()
    {
        int[] data = GenerateRandomArray(100);
        StartSelectionSort(data);
        foreach (var item in data)
        {
            Debug.Log(item);
        }
    }
    int[] GenerateRandomArray(int size)
    {
        int[] arr = new int[size];
        System.Random rand = new System.Random();
        for (int i = 0; i < size; i++)
        {
            arr[i] = rand.Next(0, 10000);
        }
        return arr;
    }
    public static void StartSelectionSort(int[] arr)
    {
        int n = arr.Length;
        for (int i = 0; i < n - 1; i++)
        {
            bool swapped = false;
            for (int j = i + 1; j < n; j++)
            {
                if (arr[j] > arr[j + 1])
                {
                    int temp = arr[j];
                    arr[j] = arr[i + 1];
                    arr[i + 1] = temp;
                    swapped = true;
                }
            }
            //이미 정렬된 경우 조기 종료
            if (!swapped) break;
        }
    }
}
