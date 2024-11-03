using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class CardsManager : MonoBehaviour
{
    public List<CardClass> myCards = new List<CardClass>(); // 手札のリスト
    private NetworkSystem networkSystem;
    
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
    public GameObject[] CloneMyCardsAsUI()
    {
        List<GameObject> clonedCards = new List<GameObject>();
    
        for(int i = 0; i < myCards.Count; i++)
        {
            CardClass card = myCards[i];
            
            // カードのゲームオブジェクトを複製
            GameObject clonedCard = new GameObject("ClonedCard_" + card.cardNum);
            
            // カードにUI用の設定（例：ボタン、画像など）を追加
            clonedCard.AddComponent<Button>();
            clonedCard.tag = "ClonedCard";  // タグを設定
    
            // UI用の適切な位置やサイズを設定
            clonedCard.transform.localScale = new Vector3(1.2f, 1.3f, 1f);  // スケールを調整
            // clonedCard.GetComponent<Image>().color = Color.white;  // 色をリセット
            
            // RectTransformを設定してUI要素にする
            RectTransform rectTransform = clonedCard.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100, 150); // カードのサイズを指定
            rectTransform.anchoredPosition = new Vector2(570 + 130f * i, 540); // カードの位置を指定
    
            // Imageコンポーネントを追加してUI画像として表示
            Image image = clonedCard.AddComponent<Image>();
            image.sprite = card.cardObject.GetComponent<Image>().sprite; // 元のカードのスプライトを取得して設定
    
            clonedCards.Add(clonedCard);
        }
        return clonedCards.ToArray();
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
        GameObject txtObj = GameObject.Find("OderOfCards(Debug)");
        TextMeshProUGUI txt = txtObj.GetComponent<TextMeshProUGUI>();
        Debug.Log("All card numbers:");
        string output = "";
        foreach (var card in myCards)
        {
            output += card.cardNum.ToString() + ", ";
        }
        
        txt.text = output;
        Debug.Log(output);
    }
}
