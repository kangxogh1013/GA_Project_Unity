using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class Inventory : MonoBehaviour
{
    public List<Item> items = new List<Item>();

    void Start()
    {
        //예시 아이템 추가
        items.Add(new Item("Sword"));
        items.Add(new Item("Shield"));
        items.Add(new Item("Potion"));

        //아이템 찾기 테스트
        Item found = FindItem("Potion");

        if (found != null)
            Debug.Log("찾은 아이템: " + found.itemName);
        else
            Debug.Log("아이템을 찾을 수 없습니다.");
    }
    public Item FindItem(string _itemName)
    {
        foreach (var item in items)
        {
            if (item.itemName == _itemName)
                return item;                                //발견시 반환
        }
        return null;                                        //못찾으면 null
    }
}
