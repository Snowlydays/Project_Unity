using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

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

    public bool isGetDiff = false;
    public bool isThreeSelect = false;
    
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
            //2枚選択か3枚選択かで分ける
            if(!isThreeSelect)
            {
                // 2枚を超えた選択は許可しない
                if(selectedCards.Count < 2)SelectCard(card);
            }else{
                // 3枚を超えた選択は許可しない
                if(selectedCards.Count < 3)SelectCard(card);
            }
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
        if (isAttacking || selectedCards.Count >= 2)
        {
            if (isAttacking) Debug.Log("QuestionController: 攻撃します");
            else if(!isThreeSelect)
            {
                //2枚選択の場合
                int ans = -1;
                if (selectedCards.Count == 2)
                {
                    ans = CompareCards(selectedCards[0], selectedCards[1]);
                }
            }else{
                //三枚選択の場合
                int ans = -1;
                if (selectedCards.Count == 3)
                {
                    ans = CompareThreeCards(selectedCards[0], selectedCards[1], selectedCards[2]);
                }else{
                    Debug.Log("Please select exactly three cards to compare.");
                    return;
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

            // 3枚選択トグルを初期化
            isThreeSelect=false;

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
        networkSystem.Log("Compared "+leftName + " and " + rightName + ".");
        if(leftName[leftName.Length - 1] < rightName[rightName.Length - 1]) Debug.Log("right card is greater");
        else Debug.Log("left card is greater");
        
        if(isGetDiff){
            //アイテム2の処理
            int Diff=0;
            Diff=Mathf.Abs(leftName[leftName.Length - 1] - rightName[rightName.Length - 1]);

            Debug.Log("カードの差は"+Diff.ToString()+"です");

            isGetDiff=false;
        }
        return 0;
    }

    int CompareThreeCards(GameObject leftCard, GameObject middleCard, GameObject rightCard)
    {
        GameObject[] Cards ={leftCard,middleCard,rightCard};

        Cards.OrderBy(e => e.transform.position.x);//x座標の昇順に並び替え 仮
        
        string leftName = Cards[0].name, middleName = Cards[1].name, rightName = Cards[2].name;
        Debug.Log("left:"+leftName + " middle:"+middleName + " right:"+rightName);
        
        int[] array={leftName[leftName.Length - 1],middleName[middleName.Length - 1],rightName[rightName.Length - 1]};
        
        string left="",middle="",right="";

        int maxindex=Array.IndexOf(array,array.Max());

        switch(maxindex)
        {
            case 0:
                left="big";
            break;
            case 1:
                middle="big";
            break;
            case 2:
                right="big";
            break;
        }

        int minindex=Array.IndexOf(array,array.Min());

        switch(minindex)
        {
            case 0:
                left="small";
            break;
            case 1:
                middle="small";
            break;
            case 2:
                right="small";
            break;
        }

        if(left==""){
            left="middle";
        }else if(middle==""){
            middle="middle";
        }else{
            right="middle";
        }

        Debug.Log("Left:"+left+" middle:"+middle+" right:"+right);
        return 0;
    }
}
