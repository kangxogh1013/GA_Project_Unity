using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class SortingAlgorithmsUI : MonoBehaviour
{
    public Text resultText;
    long selectionTime; 
    long bubbleTime; 
    long quickTime;
    void Start()

    {
        OnCompareSorts();
    }

    void OnCompareSorts()
    {
        int[] data1 = GenerateRandomArray(50000);
        int[] data2 = (int[])data1.Clone();
        int[] data3 = (int[])data1.Clone();
        Stopwatch sw = new Stopwatch();

        // 선택정렬
        sw.Start();
        SelectionSort(data1);
        sw.Stop();
         selectionTime = sw.ElapsedMilliseconds;

        sw.Reset();
        sw.Start();
        BubbleSort(data2);
        sw.Stop();
         bubbleTime = sw.ElapsedMilliseconds;

        sw.Reset();
        sw.Start();
        QuickSort(data3, 0, data3.Length - 1);
        sw.Stop();
         quickTime = sw.ElapsedMilliseconds;

        string log = $"Selection Sort: {selectionTime} ms\n"
                   + $"Bubble Sort: {bubbleTime} ms\n"
                   + $"Quick Sort: {quickTime} ms";
        UnityEngine.Debug.Log(log);
    }

    int[] GenerateRandomArray(int size)
    {
        int[] arr = new int[size];
        System.Random rand = new System.Random();
        for (int i = 0; i < size; i++)
            arr[i] = rand.Next(0, 10000);
        return arr;
    }
    void SelectionSort(int[] arr)
    {
        int n = arr.Length;
        for (int i = 0; i < n - 1; i++)
        {
            int minIndex = i;
            for (int j = i + 1; j < n; j++)
                if (arr[j] < arr[minIndex]) minIndex = j;
            int temp = arr[minIndex];
            arr[minIndex] = arr[i];
            arr[i] = temp;
        }
    }
    void BubbleSort(int[] arr)
    {
        int n = arr.Length;
        for (int i = 0; i < n - 1; i++)
        {
            bool swapped = false;
            for (int j = 0; j < n - i - 1; j++)
            {
                if (arr[j] > arr[j + 1])
                {
                    int temp = arr[j];
                    arr[j] = arr[j + 1];
                    arr[j + 1] = temp;
                    swapped = true;
                }
            }
            if (!swapped) break;
        }
    }

    // 퀵정렬
    void QuickSort(int[] arr, int low, int high)
    {
        if (low < high)
        {
            int p = Partition(arr, low, high);
            QuickSort(arr, low, p - 1);
            QuickSort(arr, p + 1, high);
        }
    }
    int Partition(int[] arr, int low, int high)
    {
        int pivot = arr[high];
        int i = low - 1;
        for (int j = low; j < high; j++)
        {
            if (arr[j] <= pivot)
            {
                i++;
                int temp = arr[i];
                arr[i] = arr[j];
                arr[j] = temp;
            }
        }
        int temp2 = arr[i + 1];
        arr[i + 1] = arr[high];
        arr[high] = temp2;
        return i + 1;
    }
    public void ShowSelection()
    {
        resultText.text = selectionTime.ToString();
        resultText.text = string.Format("Selection Sort : {0} ms", selectionTime);
    }
    
    public void ShowBubble()
    {
        resultText.text = bubbleTime.ToString();
        resultText.text = string.Format("Bubble Sort : {0} ms", bubbleTime);
    }

    public void ShowQuick()
    {
        resultText.text = bubbleTime.ToString();
        resultText.text = string.Format("Quick Sort : {0} ms", quickTime);
    }
}
