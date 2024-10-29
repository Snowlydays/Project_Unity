using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QutstionController : MonoBehaviour
{
    GameObject questionBG;
    private List<GameObject> selectedCards = new List<GameObject>();  // 選択されたカードのリスト
    private Color originalColor = Color.white;  // デフォルトのカードの色
    private Color selectedColor = Color.yellow; // 選択されたカードの色

    [SerializeField] private Button confirmButton; // 決定ボタン
    
    private CardsManager cardsManager;
    private NetworkSystem networkSystem;
    
    private GameObject selectedCard1 = null;
    private GameObject selectedCard2 = null;
    
    void Start()
    {
        cardsManager = FindObjectOfType<CardsManager>();
        networkSystem = FindObjectOfType<NetworkSystem>();
        
        questionBG = GameObject.Find("QuestioningBG");
        questionBG.SetActive(false);// 非表示に
    }

    public void StartQuestionPhase()
    {
        questionBG.SetActive(true);
            
        // CardsManagerからクローンカードを取得
        GameObject[] clonedCards = cardsManager.CloneMyCardsAsUI();
        if(clonedCards == null)Debug.LogError("clonedCards are null!");

        // キャンバスを探す
        Canvas canvas = GameObject.Find("QuestionCanvas").GetComponent<Canvas>();
            
        foreach(GameObject card in clonedCards)
        {
            // キャンバスにカードを追加
            card.transform.SetParent(canvas.transform);

            // カードをボタンとして設定
            Button cardButton = card.GetComponent<Button>();
            cardButton.onClick.AddListener(() => ToggleCardSelection(card));
        }
            
        // 決定ボタンのクリックイベント設定
        confirmButton.GetComponent<Button>().onClick.AddListener(CompareSelectedCards);
    }
    
    // カード選択状態の切り替え関数
    void ToggleCardSelection(GameObject card)
    {
        // カードが既に選択されている場合、選択を解除する
        if(selectedCards.Contains(card))
        {
            DeselectCard(card);
        }
        else
        {
            // 2枚を超えた選択は許可しない
            if(selectedCards.Count < 2)SelectCard(card);
        }
    }

    void SelectCard(GameObject card)
    {
        // カードを選択状態にし、色を変更
        selectedCards.Add(card);
        card.GetComponent<Image>().color = selectedColor;  // 色を変更
        Debug.Log("選択したカード枚数: "+selectedCards.Count);
    }
    
    void DeselectCard(GameObject card)
    {
        // カードの選択を解除し、色を元に戻す
        selectedCards.Remove(card);
        card.GetComponent<Image>().color = originalColor;  // 元の色に戻す
    }

    void CheckSelection()
    {
        if (selectedCard1 != null && selectedCard2 != null)
        {
            Debug.Log("Selected cards: " + selectedCard1.name + " and " + selectedCard2.name);

            // 比較結果を表示する処理
            CompareCards(selectedCard1, selectedCard2);
        }
    }

    // void OnCardClicked(GameObject card)
    // {
    //     SelectCard(card);
    //     CheckSelection();
    // }

    // confirmButtonを押したときに起動するメソッド
    void CompareSelectedCards()
    {
        int ans = -1;
        if (selectedCards.Count == 2)
        {
            ans = CompareCards(selectedCards[0], selectedCards[1]);
            
            // 比較後に選択状態をリセット
            foreach (GameObject card in selectedCards)
            {
                card.GetComponent<Image>().color = originalColor;  // 色を元に戻す
            }
            selectedCards.Clear();
            
            // クローンカードのUIを削除
            GameObject[] clonedCards = GameObject.FindGameObjectsWithTag("ClonedCard");
            foreach (GameObject card in clonedCards)Destroy(card);
            
            // 背景のパネルを非表示に
            questionBG.SetActive(false); 
            
            // 通常フェーズへ戻るためにreadyをトグルする
            networkSystem.ToggleReady();
        }
        else
        {
            Debug.Log("Please select exactly two cards to compare.");
        }
    }

    int CompareCards(GameObject leftCard, GameObject rightCard)
    {
        if(leftCard.transform.position.x > rightCard.transform.position.x)
        {
            (leftCard, rightCard) = (rightCard, leftCard);
        }

        string leftName = leftCard.name, rightName = rightCard.name;
        if(leftName[leftName.Length - 1] < rightName[rightName.Length - 1]) Debug.Log("right card is greater");
        else Debug.Log("left card is greater");
        
        return 0;
    }
    
    void Update()
    {
        // クリックして選択する
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if(hit.collider != null && hit.collider.gameObject.CompareTag("ClonedCard"))
            {
                SelectCard(hit.collider.gameObject);
                CheckSelection();
            }
        }
    }
}
