using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Random = UnityEngine.Random;

// using Vector2 = System.Numerics.Vector2;

public class CardsManager : MonoBehaviour
{
    public List<CardClass> myCards = new List<CardClass>(); // 手札のリスト
    private NetworkSystem networkSystem;
    
    [SerializeField] private GameObject cardPrefab; // カードのプレハブ
    [SerializeField] private GameObject slotPrefab; // スロットのプレハブ
    
    void Start()
    {
        networkSystem = FindObjectOfType<NetworkSystem>(); 
        MainSystemScript mainSystem = FindObjectOfType<MainSystemScript>();
        GameObject[] cardObjects = mainSystem.GetMyCards(); // MainSystemScriptで生成されたカードを取得
        
        int[] idx = GenRandomIdx(1, cardObjects.Length); // シャッフルインデックスを生成
        
        // カードをリストに登録
        for (int i = 0; i < cardObjects.Length; i++)
        {
            GameObject cardObject = cardObjects[i];
            CardClass card = new CardClass(cardObject, idx[i]);
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
        
            // UI用の適切な位置やサイズを設定
            // clonedCard.transform.localScale = new Vector3(1.2f, 1.3f, 1f);  // スケールを調整
            
            // RectTransformを設定してUI要素にする
            clonedCard.AddComponent<RectTransform>();
            // rectTransform.sizeDelta = new Vector2(100*(Screen.width / 1920f), 150*(Screen.height / 1080f)); // カードのサイズを指定
            // rectTransform.anchoredPosition = new Vector2((570 + 130f * i)*(Screen.width / 1920f), 540*(Screen.height / 1080f)); // カードの位置を指定
        
            // Imageコンポーネントを追加してUI画像として表示
            clonedCard.AddComponent<Image>().sprite = card.cardObject.GetComponent<Image>().sprite; // 元のカードのスプライトを取得して設定
        
            clonedCards.Add(clonedCard);
        }
        return clonedCards.ToArray();
    }
    
    // カードをパネルに配置するようのメソッド
    public GameObject[] PlaceCardsOnPanel(Transform panel, Action<GameObject> onClickAction = null, bool isMySlot = false)
    {
        GameObject[] cards = CloneMyCardsAsUI();
        for (int i = 0; i < cards.Length; i++)
        {
            // スロット生成
            // GameObject slot = Instantiate(slotPrefab, panel);
            // slot.GetComponent<CardSlot>().isMySlot = isMySlot;
        
            // スロット内にカードを作成
            GameObject card = cards[i];
            card.transform.SetParent(panel,false);
            // card.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            
            // ボタンコンポーネントを追加
            if (onClickAction != null)
            {
                Button button = card.AddComponent<Button>();
                button.onClick.AddListener(() => onClickAction(card));
            }
        }

        return cards;
    }
    
    
    // myCardsのカードをクローンしてUI用のオブジェクトを生成
    // public GameObject[] CloneMyCardsAsUI()
    // {
    //     List<GameObject> clonedCards = new List<GameObject>();
    //
    //     for(int i = 0; i < myCards.Count; i++)
    //     {
    //         CardClass card = myCards[i];
    //         
    //         // カードのゲームオブジェクトを複製
    //         GameObject clonedCard = new GameObject("ClonedCard_" + card.cardNum);
    //         
    //         // カードにUI用の設定（例：ボタン、画像など）を追加
    //         clonedCard.AddComponent<Button>();
    //         clonedCard.tag = "ClonedCard";  // タグを設定
    //
    //         // UI用の適切な位置やサイズを設定
    //         clonedCard.transform.localScale = new Vector3(1.2f, 1.3f, 1f);  // スケールを調整
    //         // clonedCard.GetComponent<Image>().color = Color.white;  // 色をリセット
    //         
    //         // RectTransformを設定してUI要素にする
    //         RectTransform rectTransform = clonedCard.AddComponent<RectTransform>();
    //         rectTransform.sizeDelta = new Vector2(100*(Screen.width / 1920f), 150*(Screen.height / 1080f)); // カードのサイズを指定
    //         rectTransform.anchoredPosition = new Vector2((570 + 130f * i)*(Screen.width / 1920f), 540*(Screen.height / 1080f)); // カードの位置を指定
    //
    //         // Imageコンポーネントを追加してUI画像として表示
    //         Image image = clonedCard.AddComponent<Image>();
    //         image.sprite = card.cardObject.GetComponent<Image>().sprite; // 元のカードのスプライトを取得して設定
    //
    //         clonedCards.Add(clonedCard);
    //     }
    //     return clonedCards.ToArray();
    // }

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
        //GameObject txtObj = GameObject.Find("OderOfCards(Debug)");
        //TextMeshProUGUI txt = txtObj.GetComponent<TextMeshProUGUI>();
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