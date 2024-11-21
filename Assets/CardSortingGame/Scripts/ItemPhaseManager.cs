using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class ItemPhaseManager : MonoBehaviour
{
    private const int ITEM_NUM = 6; // 「spell of algo」に登場するアイテム数
    public bool[] myItems = new bool[ITEM_NUM]; // 自分が所有しているアイテムをboolで管理
    public bool[] otherItems = new bool[ITEM_NUM]; // 相手が所有しているアイテムをboolで管理
    private List<int> selectedItems = new List<int>(); // 選択されたアイテムのリスト

    [SerializeField] private Transform itemDisplayPanel; // アイテム使用ボタンを表示するパネル
    [SerializeField] private GameObject itemTogglePrefab; // アイテム選択用トグルのプレハブ
    [SerializeField] private Button confirmButton; // 決定ボタン
    [SerializeField] private GameObject itemDescriptionPanel; // アイテム説明を表示するパネル

    // 所有アイテム表示用の変数
    [SerializeField] private Transform myInventoryPanel; // 自分の所有アイテムを表示するパネル
    [SerializeField] private Transform otherInventoryPanel; // 相手の所有アイテムを表示するパネル
    [SerializeField] private GameObject inventoryItemPrefab; // 所有アイテム表示用のプレハブ
    [SerializeField] private Sprite[] itemIcons; // 各アイテムのアイコン
    [SerializeField] private Sprite[] itemDescriptions; // 各アイテムの説明
    
    // Toggleオブジェクトを管理するリスト
    private List<GameObject> toggleList = new List<GameObject>();
    [SerializeField] private Color toggleOnColor = Color.green; // トグルがオンの時の色
    [SerializeField] private Color toggleOffColor = Color.white; // トグルがオフの時の色

    private NetworkSystem networkSystem;
    
    void Awake()
    {
        networkSystem = FindObjectOfType<NetworkSystem>();
        myInventoryPanel = GameObject.Find("MyInventoryPanel").transform;
        otherInventoryPanel = GameObject.Find("OtherInventoryPanel").transform;
        itemDescriptionPanel = GameObject.Find("ItemDescription");
    }
    void Start()
    {
        // 非表示
        itemDisplayPanel.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        itemDescriptionPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

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
        // DistributeItem();
        TestGetAllItem();//アイテムテスト用
        SortToggles(); // トグルのボタンをソート
        itemDisplayPanel.gameObject.SetActive(true); // アイテムディスプレイを表示
        confirmButton.gameObject.SetActive(true); // 決定ボタンを表示
    }

    public void UpdateInventoryUI()
    {
        UpdateInventory(myItems,myInventoryPanel); // 自分の所有アイテムの表示を更新
        UpdateInventory(otherItems, otherInventoryPanel); // 相手の所有アイテムの表示を更新
    }
    
    // 表示するアイテムを更新するメソッド
    private void UpdateInventory(bool [] items, Transform panel)
    {
        // 既存のアイコンをクリア
        foreach (Transform child in panel)
        {
            Destroy(child.gameObject);
        }
        
        // 所有しているアイテムのアイコンを表示
        for(int i = 0;i < ITEM_NUM;i++)
        {
            if (items[i])
            {
                CreateInventoryItem(i,panel);
            }
        }
    }

    private void CreateInventoryItem(int itemIdx, Transform panel)
    {
        // プレハブをインスタンス化して、inventoryPanelの子として配置
        GameObject inventoryItem = Instantiate(inventoryItemPrefab, panel);

        // RectTransformを設定
        RectTransform rectTransform = inventoryItem.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        // Imageコンポーネントにアイコンを設定
        Image itemImage = inventoryItem.GetComponent<Image>();
        itemImage.sprite = itemIcons[itemIdx];

        inventoryItem.AddComponent<Button>();
        Button itemButton = inventoryItem.GetComponent<Button>();
        itemButton.onClick.AddListener(() => ShowItemDescription(inventoryItem, itemIdx)); 
    }

    private void ShowItemDescription(GameObject item, int itemIdx)
    {
        Image itemDescription = itemDescriptionPanel.GetComponent<Image>();
        if(itemDescription.sprite == itemDescriptions[itemIdx])
        {
            itemDescription.sprite = null;
            itemDescription.color = new Color(0f, 0f, 0f, 0f);
        }
        else
        {
            itemDescription.sprite = itemDescriptions[itemIdx];
            itemDescription.color = new Color(1f, 1f, 1f, 1f);
        }
    }

    private void OnConfirmButtonClicked()
    {
        Debug.Log("決定ボタンがクリックされた");
        foreach (int itemIdx in selectedItems)
        {
            networkSystem.ChangeItems(itemIdx,false);
        }

        networkSystem.ChangeItemSelects(selectedItems.ToArray());

        // 使用したアイテムのtoggleを削除
        foreach (GameObject toggleObj in toggleList.ToList())
        {
            Toggle toggle = toggleObj.GetComponent<Toggle>();
            if (toggle.isOn)
            {
                toggleList.Remove(toggleObj);
                Destroy(toggleObj);
            }
        }
        
        selectedItems.Clear(); // 選択していたアイテムをクリア
        itemDisplayPanel.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        //networkSystem.ToggleReady();
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
            networkSystem.ChangeItems(distributedItemIdx,true);
            Debug.Log($"アイテム{distributedItemIdx+1}:を配布");

            // アイテム選択用トグルを作成
            CreateToggle(distributedItemIdx);
        }
        else
        {
            Debug.Log("全てのアイテムを所有中");
        }
    }

    // アイテムの使用・不使用を選択できるトグルを作成するメソッド
    private void CreateToggle(int itemIdx)
    {
        // プレハブをインスタンス化して、itemDisplayPanelの子として配置
        GameObject toggleObj = Instantiate(itemTogglePrefab, itemDisplayPanel.Find("LayoutGroup"));

        // Layout Groupを機能させるために、RectTransformをリセット
        RectTransform rectTransform = toggleObj.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;
        rectTransform.localPosition = Vector3.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        // Toggleコンポーネントとテキストの設定
        Toggle toggle = toggleObj.GetComponent<Toggle>();
        TextMeshProUGUI toggleText = toggleObj.GetComponentInChildren<TextMeshProUGUI>();
        toggleText.text = $"{itemIdx+1}";

        // Toggleの背景Imageを取得
        Image backgroundImage = toggleObj.GetComponentInChildren<Image>();
        // 初期色を設定
        backgroundImage.color = toggle.isOn ? toggleOnColor : toggleOffColor;

        // Toggleのリスナーを追加し、色を変更
        toggle.onValueChanged.AddListener((isOn) =>
        {
            backgroundImage.color = isOn ? toggleOnColor : toggleOffColor;
            OnToggleValueChanged(isOn, itemIdx);
        });

        // Toggleをリストに追加
        toggleList.Add(toggleObj);
    }

    // Toggleのボタンを昇順に並び変えるメソッド(これがないとボタンが作成された順に並ぶ)
    private void SortToggles()
    {
        // ソート(Toggleのテキストの昇順)
        toggleList.Sort((a, b) =>
        {
            string textA = a.GetComponentInChildren<TextMeshProUGUI>().text;
            string textB = b.GetComponentInChildren<TextMeshProUGUI>().text;
            return String.CompareOrdinal(textA, textB);
        });

        // ソートされた順にUI上の順序を再設定
        for (int i = 0; i < toggleList.Count; i++)
        {
            toggleList[i].transform.SetSiblingIndex(i);
        }
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
        //networkSystem.ChangeItemSelects(selectedItems.ToArray());
    }

    //テスト用
    private void TestGetAllItem()
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
        
        foreach(int id in unownedItems)
        {
            networkSystem.ChangeItems(id,true);
            Debug.Log($"アイテム{id+1}:を配布");
            
            // アイテム選択用トグルを作成
            CreateToggle(id);
        }
    }

    private void ApplyItemEffect(int itemIdx)
    {
        // ここにアイテムの効果を実装
        switch(itemIdx)
        {
            case 0:

            break;

            case 1:
                networkSystem.questionController.isGetDiff=true;
            break;

            case 2:

            break;

            case 3:

            break;

            case 4:

            break;

            case 5:
                networkSystem.questionController.isThreeSelect=true;
            break;
        }
    }
}