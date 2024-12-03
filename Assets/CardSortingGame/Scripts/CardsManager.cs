using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Random = UnityEngine.Random;


public class CardsManager : MonoBehaviour
{
    public List<CardClass> myCards = new List<CardClass>(); // 手札のリスト
    private NetworkSystem networkSystem;
    private MainSystemScript mainSystem;
    
    [SerializeField] private GameObject cardPrefab; // カードのプレハブ
    [SerializeField] private GameObject slotPrefab; // スロットのプレハブ

    private float selectionOffset = 30f; // 選択時の移動量
    private Dictionary<GameObject, Vector3> originalCardPositions = new Dictionary<GameObject, Vector3>(); // 選択されたカードとその元の位置を保持するディクショナリ
    public Dictionary<int, int> cardAlphabet = new Dictionary<int, int>(); //カードの番号とアルファベットの対応
    public static string[] intToAlph = {"A", "B", "C", "D", "E"};

    void Start()
    {
        networkSystem = FindObjectOfType<NetworkSystem>();
        mainSystem = FindObjectOfType<MainSystemScript>();
        
        GameObject[] cardObjects = mainSystem.GetMyCards(); // MainSystemScriptで生成されたカードを取得

        int[] idx = GenRandomIdx(1, cardObjects.Length); // シャッフルインデックスを生成
        
        cardAlphabet[-1] = 0;
        // カードをリストに登録
        for (int i = 0; i < cardObjects.Length; i++)
        {
            GameObject cardObject = cardObjects[i];
            CardClass card = new CardClass(cardObject, idx[i]);
            cardAlphabet[idx[i]] = i;
            myCards.Add(card);
        }
    }

    
    // myCardsのカードをクローンしてUI用のオブジェクトを生成
    private GameObject[] CloneMyCardsAsUI()
    {
        List<GameObject> clonedCards = new List<GameObject>();
        
        for(int i = 0; i < myCards.Count; i++)
        {
            CardClass card = myCards[i];
            
            // カードのゲームオブジェクトを複製
            GameObject clonedCard = new GameObject("ClonedCard_" + card.cardNum);
            
            // カードにタグを追加
            clonedCard.tag = "ClonedCard";
            
            // RectTransformを設定してUI要素にする
            clonedCard.AddComponent<RectTransform>();
        
            // Imageコンポーネントを追加してUI画像として表示
            clonedCard.AddComponent<Image>().sprite = card.cardObject.GetComponent<Image>().sprite; // 元のカードのスプライトを取得して設定
        
            clonedCards.Add(clonedCard);
        }
        return clonedCards.ToArray();
    }
    
    
    // カードをパネルに配置するようのメソッド
    public GameObject[] PlaceCardsOnPanel(Transform panel, Action<GameObject> onClickAction, float _cardWidth, float _cardSpacing, float _paddingLeft, float _paddingRight)
    {
        RectTransform panelRect = panel.GetComponent<RectTransform>(); // RectTransformを取得
        AdjustPanelSize(panelRect,_cardWidth,_cardSpacing, _paddingLeft, _paddingRight);
        
        GameObject[] cards = CloneMyCardsAsUI();
        for (int i = 0; i < cards.Length; i++)
        {
            // スロット内にカードを作成
            GameObject card = cards[i];
            card.transform.SetParent(panel,false);
            // card.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            
            // ボタンコンポーネントを追加
            if (onClickAction != null)
            {
                Button button = card.AddComponent<Button>();
                button.onClick.AddListener(() => {
                    onClickAction(card);
                });
            }
        }

        return cards;
    }
    
    // パネルの横幅のサイズを調整するためのメソッド
    public void AdjustPanelSize(RectTransform panelRect, float cardWidth, float cardSpacing, float paddingLeft, float paddingRight)
    {
        // パネルの横幅を計算
        float totalWidth = paddingLeft + paddingRight + (cardWidth * NetworkSystem.cardNum) + (cardSpacing * (NetworkSystem.cardNum - 1));
        panelRect.sizeDelta = new Vector2(totalWidth, panelRect.sizeDelta.y);
    }
    
    public void SelectCardUI(GameObject card)
    {
        // カードの元の位置を記録
        RectTransform rectTransform = card.GetComponent<RectTransform>();
        if (!originalCardPositions.ContainsKey(card))
        {
            originalCardPositions.Add(card, rectTransform.anchoredPosition);
        }

        // 上に移動させるターゲット位置を計算
        Vector2 targetPosition = rectTransform.anchoredPosition + new Vector2(0, selectionOffset);

        // アニメーションを開始
        StartCoroutine(MoveCard(card, targetPosition));

        // 選択状態を示すために色を変更
        card.GetComponent<Image>().color = Color.yellow;
    }

    public void DeselectCardUI(GameObject card)
    {
        // 元の位置を取得
        if (originalCardPositions.ContainsKey(card))
        {
            Vector2 originalPosition = originalCardPositions[card];
            RectTransform rectTransform = card.GetComponent<RectTransform>();

            // アニメーションを開始
            StartCoroutine(MoveCard(card, originalPosition));

            // 元の位置を削除
            originalCardPositions.Remove(card);
        }

        // 色を元に戻す
        card.GetComponent<Image>().color = Color.white;
    }

    // Coroutineでカードを移動させる関数
    private IEnumerator MoveCard(GameObject card, Vector2 targetPosition)
    {
        RectTransform rectTransform = card.GetComponent<RectTransform>();
        Vector2 startPosition = rectTransform.anchoredPosition;
        float currentTime = 0f;
        float animationTime = 0.2f;

        while (currentTime < animationTime)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, currentTime / animationTime);
            currentTime += Time.deltaTime;
            yield return null;
        }
        rectTransform.anchoredPosition = targetPosition;
    }
    
    // ランダムインデックス生成
    public int[] GenRandomIdx(int origin, int len)
    {
        int[] idx = new int[len];
        for  (int i = 0; i < len; i++)
        {
            idx[i] = i + origin;
        }
        for  (int i = 0; i < len; i++)
        {
            int j = Random.Range(0, len);
            (idx[i], idx[j]) = (idx[j], idx[i]);
        }
        return idx;
    }
    
    public void SwapCardsInList(int indexA, int indexB)
    {
        if (indexA < 0 || indexB < 0 || indexA >= myCards.Count || indexB >= myCards.Count) return;

        // myCardsリスト内の位置をスワップ
        (myCards[indexA], myCards[indexB]) = (myCards[indexB], myCards[indexA]);

        // ネットワーク上のカードリストもスワップ
        if (networkSystem.IsHost)networkSystem.SwapHostCardServerRpc(indexA, indexB);
        else networkSystem.SwapClientCardServerRpc(indexA, indexB);
    }

    // printデバッグ用関数
    public void printMyCards()
    {
        Debug.Log("All card numbers:");
        string output = "";
        foreach (var card in myCards)
        {
            output += card.cardNum.ToString() + ", ";
        }
        
        //txt.text = output;
        Debug.Log(output);
    }
}