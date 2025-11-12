using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SearchPerformance : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField searchInput;
    public Button linearButton;
    public Button binaryButton;
    public Transform contentParent;
    public GameObject itemPrefab;

    private List<Item> allItems, sortedItems;
    private List<GameObject> spawned = new List<GameObject>();

    void Start()
    {
        // 아이템 100개 생성 및 정렬
        allItems = new List<Item>();
        for (int i = 0; i < 100; i++)
            allItems.Add(new Item($"Item_{i:D2}"));
        sortedItems = new List<Item>(allItems);
        sortedItems.Sort((a, b) => a.itemName.CompareTo(b.itemName));
        // 버튼 콜백 등록
        linearButton.onClick.AddListener(SearchLinear);
        binaryButton.onClick.AddListener(SearchBinary);

        // 첫 화면에 전체 표시
        DisplayAll();
    }

    void Clear()
    {
        foreach (var go in spawned) Destroy(go);
        spawned.Clear();
    }

    void DisplayList(IEnumerable<Item> list)
    {
        Clear();
        foreach (var it in list)
        {
            var go = Instantiate(itemPrefab, contentParent);
            go.GetComponentInChildren<TMP_Text>().text = it.itemName;
            spawned.Add(go);
        }
    }

    public void DisplayAll() => DisplayList(allItems);

    void SearchLinear()
    {
        string key = searchInput.text.Trim();
        if (string.IsNullOrEmpty(key))
        {
            DisplayAll(); // 검색창이 비어있으면 전체 표시
            return;
        }
        var found = allItems.FindAll(x => x.itemName == key);
        DisplayList(found);
    }

    void SearchBinary()
    {
        string key = searchInput.text.Trim();
        if (string.IsNullOrEmpty(key))
        {
            DisplayAll(); // 검색창이 비어있으면 전체 표시
            return;
        }
        int l = 0, r = sortedItems.Count - 1;
        while (l <= r)
        {
            int m = (l + r) / 2;
            int cmp = sortedItems[m].itemName.CompareTo(key);
            if (cmp == 0) { DisplayList(new[] { sortedItems[m] }); return; }
            if (cmp < 0) l = m + 1; else r = m - 1;
        }
        DisplayList(new Item[0]);
    }
}