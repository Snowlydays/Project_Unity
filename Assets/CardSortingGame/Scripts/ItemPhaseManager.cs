using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemPhaseManager : MonoBehaviour
{
    const int ITEM_NUM = 6; // 「spell of algo」に登場するアイテム数
    public bool[] myItems = new bool[ITEM_NUM]; // 自分が所有しているアイテムをboolで管理
    private List<int> selectedItems = new List<int>(); // 選択されたアイテムのリスト

    [SerializeField] private Transform itemDisplayPanel; // アイテム使用ボタンを表示するパネル
    [SerializeField] private GameObject itemTogglePrefab; // アイテム選択用トグルのプレハブ
    [SerializeField] private Button confirmButton; // 決定ボタン

    void Start()
    {
        // 非表示
        itemDisplayPanel.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);

        // 自分の所有アイテムを初期化
        for (int i = 0; i < ITEM_NUM; i++)
        {
            myItems[i] = false;
        }
        // 決定ボタンにリスナーを追加
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
    }

    public void StartItemPhase()
    {
        Debug.Log("アイテムフェーズ開始");
        DistributeItem();
        DistributeItem();
        itemDisplayPanel.gameObject.SetActive(true); // アイテムディスプレイを表示
        confirmButton.gameObject.SetActive(true); // 決定ボタンを表示
    }

    private void OnConfirmButtonClicked()
    {
        Debug.Log("決定ボタンがクリックされた");
        foreach (int itemIdx in selectedItems)
        {
            Debug.Log($"アイテム{itemIdx} を使用");
            ApplyItemEffect(itemIdx);
            myItems[itemIdx] = false;
        }

        selectedItems.Clear(); // 選択していたアイテムをクリア
        itemDisplayPanel.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);

        NetworkSystem.phase = 2; // 質問・詠唱フェーズへ進める
    }

    // プレイヤーにアイテムを配布するメソッド
    private void DistributeItem()
    {
        // 未所有のアイテムをリストへ追加
        List<int> unownedItems = new List<int>();
        for (int i = 0; i < ITEM_NUM; i++)
        {
            if (!myItems[i])
            {
                unownedItems.Add(i);
            }
        }

        if (unownedItems.Count > 0)
        {
            // 未所有のアイテムリストからランダムに選び、プレイヤーへ配る
            int randomIdx = UnityEngine.Random.Range(0, unownedItems.Count);
            int distributedItemIdx = unownedItems[randomIdx];
            myItems[distributedItemIdx] = true;
            Debug.Log($"アイテム{distributedItemIdx}:を配布");

            // アイテム選択用トグルを作成
            CreateItemToggle(distributedItemIdx);
        }
        else
        {
            Debug.Log("全てのアイテムを所有中");
        }
    }

    // アイテムの使用・不使用を選択できるトグルを作成するメソッド
    private void CreateItemToggle(int itemIdx)
    {
        GameObject toggleObj = Instantiate(itemTogglePrefab, itemDisplayPanel);
        Toggle toggle = toggleObj.GetComponent<Toggle>();
        TextMeshProUGUI toggleText = toggleObj.GetComponentInChildren<TextMeshProUGUI>();
        toggleText.text = $"{itemIdx}";
        toggle.onValueChanged.AddListener((isOn) => OnToggleValueChanged(isOn, itemIdx));
    }

    private void OnToggleValueChanged(bool isOn, int itemIdx)
    {
        if (isOn)
        {
            if (!selectedItems.Contains(itemIdx))
            {
                Debug.Log($"アイテム{itemIdx} を選択");
                selectedItems.Add(itemIdx);
            }
        }
        else
        {
            if (selectedItems.Contains(itemIdx))
            {
                Debug.Log($"アイテム{itemIdx} の選択を解除");
                selectedItems.Remove(itemIdx);
            }
        }
    }

    private void ApplyItemEffect(int itemIdx)
    {
        // ここにアイテムの効果を実装
    }
}