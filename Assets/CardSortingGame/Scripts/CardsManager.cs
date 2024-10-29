using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class CardsManager : MonoBehaviour
{
    public List<CardClass> myCards = new List<CardClass>(); // 手札のリスト
    // private Vector3 originalPosition; // 元の位置を記憶
    // private GameObject draggingCard = null; // 現在ドラッグ中のカード
    // private int draggingCardIndex = -1; // ドラッグ中のカードのインデックス
    // private int hoveringIndex = -1; // マウスが重なっているカードのインデックス
    //
    // public static float cardSpacing = 130f;
    // public static float cardYPosition = -75f;


    void Start()
    {
        MainSystemScript mainSystem = FindObjectOfType<MainSystemScript>();
        GameObject[] cardObjects = mainSystem.GetMyCards(); // MainSystemScriptで生成されたカードを取得
        
        int[] idx = GenRandomIdx(1, cardObjects.Length); // シャッフルインデックスを生成
        
        // カードにドラッグ機能を追加してリストに登録
        for (int i = 0; i < cardObjects.Length; i++)
        {
            GameObject cardObject = cardObjects[i];
            // AddDragFunctionality(cardObject, i); // ドラッグ機能を追加
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
            rectTransform.anchoredPosition = new Vector2(130f*(float)(3-i) + 960, 540); // カードの位置を指定

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
    }

    

    // ドラッグ機能をカードに追加
    // private void AddDragFunctionality(GameObject card, int cardIndex)
    // {
    //     var image = card.GetComponent<Image>();
    //     if (image != null)
    //     {
    //         image.raycastTarget = true; // RaycastTargetを有効にする
    //     }
    //
    //     // マウスダウンでドラッグ開始
    //     card.AddComponent<Draggable>().Initialize(
    //         onBeginDrag: () => OnBeginDrag(card, cardIndex),
    //         onDrag: () => OnDrag(card),
    //         onEndDrag: () => OnEndDrag(card)
    //     );
    // }

    // private void OnBeginDrag(GameObject card, int index)
    // {
    //     originalPosition = card.transform.localPosition; // 元の位置を保存
    //     draggingCard = card; // ドラッグ中のカードを記録
    //     draggingCardIndex = index;
    // }
    //
    // private void OnDrag(GameObject card)
    // {
    //     // 一時的な並べ替えを仮表示
    //     int closestIndex = GetClosestCardIndex(card.transform.localPosition.x);
    //
    //     hoveringIndex = closestIndex;
    //     // 一時的な配置を視覚化
    //     UpdateTemporaryArrangement(draggingCardIndex, closestIndex);
    // }
    //
    // private void OnEndDrag(GameObject card)
    // {
    //     // ドロップ時の処理
    //     RearrangeCards(draggingCardIndex, hoveringIndex);
    //     hoveringIndex = -1;
    //     draggingCard = null;
    //     draggingCardIndex = -1;
    // }
    //
    //
    // // 元の配置に戻す関数
    // private void ResetTemporaryRearrangement()
    // {
    //     // カードの元の位置を復元
    //     for (int i = 0; i < myCards.Count; i++)
    //     {
    //         myCards[i].cardObject.transform.localPosition = new Vector3(cardSpacing * (3 - i), cardYPosition, 0.0f);
    //     }
    // }

    // 最も近いカードのインデックスを取得
    // private int GetClosestCardIndex(float xPosition)
    // {
    //     float minDistance = Mathf.Infinity;
    //     int closestIndex = -1;
    //     // 各カードのx座標を基準に、最も近いカードを判定
    //     for (int i = 0; i < myCards.Count; i++)
    //     {
    //         float cardX = cardSpacing * (3 - i); // 配置座標に基づくx位置
    //         float distance = Mathf.Abs(cardX - xPosition);
    //         if (distance < minDistance)
    //         {
    //             minDistance = distance;
    //             closestIndex = i;
    //         }
    //     }
    //     return closestIndex;
    // }

    // 一時的にカードを並べ替える
    // private void UpdateTemporaryArrangement(int oldIndex, int newIndex)
    // {
    //     for (int i = 0; i < myCards.Count; i++)
    //     {
    //         if (i == oldIndex)
    //         {
    //             // ドラッグ中のカードの元の位置はスキップ
    //             continue;
    //         }
    //
    //         if (oldIndex < newIndex && i > oldIndex && i <= newIndex)
    //         {
    //             // ドラッグ中のカードが右に移動、他のカードを左にずらす
    //             myCards[i].cardObject.transform.localPosition = new Vector3(cardSpacing * (3 - (i - 1)), cardYPosition, 0);
    //         }
    //         else if (oldIndex > newIndex && i >= newIndex && i < oldIndex)
    //         {
    //             // ドラッグ中のカードが左に移動、他のカードを右にずらす
    //             myCards[i].cardObject.transform.localPosition = new Vector3(cardSpacing * (3 - (i + 1)), cardYPosition, 0);
    //         }
    //         else
    //         {
    //             // 通常の配置
    //             myCards[i].cardObject.transform.localPosition = new Vector3(cardSpacing * (3 - i), cardYPosition, 0);
    //         }
    //     }
    // }
    //
    // // カードを再配置するロジック
    // private void RearrangeCards(int oldIndex, int newIndex)
    // {
    //     Debug.Log("Rearranged: " + oldIndex + " and " + newIndex);
    //     var card = myCards[oldIndex];
    //
    //     // カードの再配置
    //     KeepMyCardsFollowingThePositions();
    //     for (int i = 0; i < myCards.Count; i++)
    //     {
    //         myCards[i].cardObject.transform.localPosition = new Vector3(cardSpacing * (3 - i), cardYPosition, 0.0f);
    //     }
    // }

    // myCardsでのインデックスをx座標の関係と同期
    // void KeepMyCardsFollowingThePositions()
    // {
    //     // バブルソートで実装
    //     int n = myCards.Count;
    //     for(int i = 0; i < n - 1; i++)
    //     {
    //         for(int j = i + 1; j < n; j++)
    //         {
    //             if(myCards[i].cardObject.transform.localPosition.x > myCards[j].cardObject.transform.localPosition.x)continue;
    //             (myCards[i], myCards[j]) = (myCards[j], myCards[i]);
    //         }
    //     }
    // }

    // printデバッグ用関数
    public void printMyCards()
    {
        // List<CardClass> tmp = new List<CardClass>(myCards);        
        // int n = tmp.Count;
        // for(int i = 0; i < n - 1; i++)
        // {
        //     for(int j = i + 1; j < n; j++)
        //     {
        //         if(tmp[i].cardObject.transform.localPosition.x <= tmp[j].cardObject.transform.localPosition.x)continue;
        //         (tmp[i], tmp[j]) = (tmp[j], tmp[i]);
        //     }
        // }
        // string arrange = "";
        // for(int i = 0; i < tmp.Count; i++)
        // {
        //     arrange += tmp[i].cardNum.ToString() + ", ";
        // }
        GameObject txtObj = GameObject.Find("OderOfCards(Debug)");
        TextMeshProUGUI txt = txtObj.GetComponent<TextMeshProUGUI>();
        // txt.text = arrange;
        
        Debug.Log("All card numbers:");
        string output = "";
        foreach (var card in myCards)
        {
            output += card.cardNum.ToString() + ", ";
        }
        txt.text = output;
        Debug.Log(output);
        // arrange = "";
        // for(int i = 0; i < myCards.Count; i++)
        // {
        //     arrange += myCards[i].cardNum.ToString() + ", ";
        // }
        // Debug.Log(new string((arrange.Reverse().ToArray())));
    }

    void Update()
    {
        // printMyCards();
        // KeepMyCardsFollowingThePositions();
    }
}

/*
CloneMyCardsAsUI: QuestionControllerから参照するための関数。並べ替えには関係ない
GenRandomIdx: 最初にランダムな並びを作るための関数
AddDragFunctionality: カードにドラッグ機能を追加する関数
OnBeginDrag: ドラッグを開始する時に実行される
OnDrag: ドラッグ中にずっと実行される関数
OnEndDrag: カードをドロップした時に実行される関数
GetClosestCardIndex: 現在ドラッグしているカードにx座標が一番近いカードのインデックスを取得する。(ここが結構怪しい)
UpdateTemporaryArrangemen: カードを動かしている時、カードを左右にずらす動作をここでやっている
RearrangeCards: カードを離した時、カードの位置を整列させる。
KeepMyCardsFollowingThePositions: x座標の並びにmyCardの中の並び順を合わせる。
printMyCards: デバッグ用。

無理やり後から修正しているので、関数名と役割が一致していないことがある。
例えばUpdateTemporaryArrangemenなどは、Temporaryと言いつつここで動かしたカードの位置はほぼ確定。
*/