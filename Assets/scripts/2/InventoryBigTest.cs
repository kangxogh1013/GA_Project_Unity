using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class InventoryBigTest : MonoBehaviour
{
    List<Item> items = new List<Item>();

    private System.Random rand = new System.Random();

    // Start is called before the first frame update
    void Start()
    {
        //1������ ������ ����
        for (int i = 0; i < 10000; i++)
        {
            string name = $"Item_{i:D5}"; // Item_00001 ����
            int qty = rand.Next(1, 100);  // 1~99�� ���� ����
            items.Add(new Item(name, qty));
        }

        // ���� Ž�� ���
        string target = "Item_45672";
        Stopwatch sw = Stopwatch.StartNew();
        Item foundLinear = FindItemLinear(target);
        sw.Stop();
        UnityEngine.Debug.Log($"[���� Ž��] {target} ����: {foundLinear?.quantity}, �ð�: {sw.ElapsedMilliseconds}ms");

        // ���� Ž�� ���
        sw.Restart();
        Item foundBinary = FindItemBinary(target);
        sw.Stop();
        UnityEngine.Debug.Log($"[���� Ž��] {target} ����: {foundBinary?.quantity}, �ð�: {sw.ElapsedMilliseconds}ms");
    }
    public Item FindItemLinear(string targetName)
    {
        foreach (Item item in items)
        {
            if (item.itemName == targetName)
                return item;
        }
        return null;
    }

    // ���� Ž��
    public Item FindItemBinary(string targetName)
    {
        int left = 0;
        int right = items.Count - 1;

        while (left <= right)
        {
            int mid = (left + right) / 2;
            int cmp = items[mid].itemName.CompareTo(targetName);

            if (cmp == 0) return items[mid];
            else if (cmp < 0) left = mid + 1;
            else right = mid - 1;
        }
        return null;
    }

}
