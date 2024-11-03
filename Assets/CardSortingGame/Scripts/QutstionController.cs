using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
    [SerializeField] private Button spellButton; // スペルボタン
    private bool isAttacking = false;
    
    private CardsManager cardsManager;
    private NetworkSystem networkSystem;
    
    void Start()
    {
        cardsManager = FindObjectOfType<CardsManager>();
        networkSystem = FindObjectOfType<NetworkSystem>();
        
        questionBG = GameObject.Find("QuestioningBG");
        questionBG.SetActive(false);// 非表示に
        
        // クリックイベント設定
        confirmButton.GetComponent<Button>().onClick.AddListener(OnConfirmButtonClicked);
        spellButton.onClick.AddListener(OnSpellButtonClicked);
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
    }

    void OnSpellButtonClicked()
    {
        Debug.Log("スペルボタン クリック");
        isAttacking = !isAttacking;
        networkSystem.ToggleAttacked();
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
    
    void OnConfirmButtonClicked()
    {
        if (isAttacking || selectedCards.Count == 2)
        {
            if (isAttacking) Debug.Log("QuestionController: 攻撃します");
            else
            {
                int ans = -1;
                if (selectedCards.Count == 2)
                {
                    ans = CompareCards(selectedCards[0], selectedCards[1]);
                }
            }
            // 比較後に選択状態をリセット
            foreach (GameObject card in selectedCards)
            {
                card.GetComponent<Image>().color = originalColor; // 色を元に戻す
            }
            selectedCards.Clear();

            // クローンカードのUIを削除
            GameObject[] clonedCards = GameObject.FindGameObjectsWithTag("ClonedCard");
            foreach (GameObject card in clonedCards) Destroy(card);

            // 背景のパネルを非表示に
            questionBG.SetActive(false);
            
            // 攻撃トグルを初期化
            isAttacking = false;

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
        Debug.Log("left:"+leftName + " right:"+rightName);
        if(leftName[leftName.Length - 1] < rightName[rightName.Length - 1]) Debug.Log("right card is greater");
        else Debug.Log("left card is greater");
        
        return 0;
    }
}
