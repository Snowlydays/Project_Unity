using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardsManager : MonoBehaviour
{
    public List<CardClass> myCards = new List<CardClass>(); // 手札のリスト
    private Vector3 originalPosition; // 元の位置を記憶
    private GameObject draggingCard = null; // 現在ドラッグ中のカード
    private int draggingCardIndex = -1; // ドラッグ中のカードのインデックス
    private int hoveringIndex = -1; // マウスが重なっているカードのインデックス

    public List<CardClass> myCards = new List<CardClass>(); // 手札のリスト
    private Vector3 originalPosition; // 元の位置を記憶
    private GameObject draggingCard = null; // 現在ドラッグ中のカード
    private int draggingCardIndex = -1; // ドラッグ中のカードのインデックス
    private int hoveringIndex = -1; // マウスが重なっているカードのインデックス

    void Start()
    {
        MainSystemScript mainSystem = FindObjectOfType<MainSystemScript>();
        GameObject[] cardObjects = mainSystem.GetMyCards(); // MainSystemScriptで生成されたカードを取得
        
        int[] idx = GenRandomIdx(1, cardObjects.Length); // シャッフルインデックスを生成
        
        // カードにドラッグ機能を追加してリストに登録
        for (int i = 0; i < cardObjects.Length; i++)
        GameObject[] cardObjects = mainSystem.GetMyCards(); // MainSystemScriptで生成されたカードを取得
        
        int[] idx = GenRandomIdx(1, cardObjects.Length); // シャッフルインデックスを生成
        
        // カードにドラッグ機能を追加してリストに登録
        for (int i = 0; i < cardObjects.Length; i++)
        {
            GameObject cardObject = cardObjects[i];
            AddDragFunctionality(cardObject, i); // ドラッグ機能を追加
            CardClass card = new CardClass(cardObject, idx[i]);
            GameObject cardObject = cardObjects[i];
            AddDragFunctionality(cardObject, i); // ドラッグ機能を追加
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
            var card = myCards[i];
            // カードのゲームオブジェクトを複製
            GameObject clonedCard = new GameObject("ClonedCard_" + card.cardIdx);
            
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
            // Debug.Log(idx[i]);
        }

        return clonedCards.ToArray();
    }

    // ランダムインデックス生成
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

    // ドラッグ機能をカードに追加
    private void AddDragFunctionality(GameObject card, int cardIndex)
    {
        var image = card.GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = true; // RaycastTargetを有効にする
        }

        // マウスダウンでドラッグ開始
        card.AddComponent<Draggable>().Initialize(
            onBeginDrag: () => OnBeginDrag(card, cardIndex),
            onDrag: () => OnDrag(card),
            onEndDrag: () => OnEndDrag(card)
        );
    }

    private void OnBeginDrag(GameObject card, int index)
    {
        originalPosition = card.transform.localPosition; // 元の位置を保存
        draggingCard = card; // ドラッグ中のカードを記録
        draggingCardIndex = index;
    }

    private void OnDrag(GameObject card)
    {
        // UI座標系でマウスの位置を取得し、カードに反映させる
        Vector3 newPosition = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            card.transform.parent as RectTransform, // 親のRectTransform
            newPosition,
            Camera.main, // カメラ
            out Vector2 localPoint // ローカル座標系に変換されたマウス位置
        );
        card.transform.localPosition = localPoint;

        // 一時的な並べ替えを仮表示
        /*
        ここで引数として渡している値が、カードの中心ではない可能性が高い
        中心を正しく指定して、一時的な並べ替えが綺麗に行えるようにしたい
        */
        int closestIndex = GetClosestCardIndex(card.transform.localPosition.x);

        if (closestIndex != -1 && closestIndex != draggingCardIndex)
        {
            hoveringIndex = closestIndex;
            // 一時的な配置を視覚化
            UpdateTemporaryArrangement(draggingCardIndex, closestIndex);
        }
        else
        {
            // 元の並びに戻す
            ResetTemporaryRearrangement();
        }
    }

    // 一時的にカードを並べ替える
    private void UpdateTemporaryArrangement(int oldIndex, int newIndex)
    {
        for (int i = 0; i < myCards.Count; i++)
        {
            if (i == oldIndex)
            {
                // ドラッグ中のカードの元の位置は空白
                continue;
            }

            if (i > oldIndex && i <= newIndex)
            {
                // ドラッグ中のカードが右に移動、他のカードを左にずらす
                myCards[i].cardObject.transform.localPosition = new Vector3(1.5f * (3 - (i - 1)), -0.75f, 0);
            }
            else if (i < oldIndex && i >= newIndex)
            {
                /*
                カードを元の位置より左に持ってくるとバグる
                いや、そうではなさそう
                */
                // ドラッグ中のカードが左に移動、他のカードを右にずらす
                myCards[i].cardObject.transform.localPosition = new Vector3(1.5f * (3 - (i + 1)), -0.75f, 0);
            }
            else
            {
                // 通常の配置
                myCards[i].cardObject.transform.localPosition = new Vector3(1.5f * (3 - i), -0.75f, 0);
            }
        }
    }

    private void OnEndDrag(GameObject card)
    {
        // ドロップ時の処理
        if (hoveringIndex != -1)
        {
            RearrangeCards(draggingCardIndex, hoveringIndex);
        }
        else
        {
            // 元の位置に戻す
            card.transform.localPosition = originalPosition;
        }

        hoveringIndex = -1;
        draggingCard = null;
        draggingCardIndex = -1;
    }

    // 元の配置に戻す関数
    private void ResetTemporaryRearrangement()
    {
        // カードの元の位置を復元
        for (int i = 0; i < myCards.Count; i++)
        {
            myCards[i].cardObject.transform.localPosition = new Vector3(1.5f * (3 - i), -0.75f, 0.0f);
        }
    }

    // 最も近いカードのインデックスを取得
    private int GetClosestCardIndex(float xPosition)
    {
        float minDistance = Mathf.Infinity;
        int closestIndex = -1;

        // 各カードのx座標を基準に、最も近いカードを判定
        for (int i = 0; i < myCards.Count; i++)
        {
            float cardX = 1.5f * (3 - i); // 配置座標に基づくx位置
            float distance = Mathf.Abs(cardX - xPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }
        return closestIndex;
    }

    // カードを再配置するロジック
    private void RearrangeCards(int oldIndex, int newIndex)
    {
        Debug.Log("Rearranged: " + oldIndex + " and " + newIndex);
        GameObject card = myCards[oldIndex].cardObject;

        // カードの再配置
        for (int i = 0; i < myCards.Count; i++)
        {
            myCards[i].cardObject.transform.localPosition = new Vector3(1.5f * (3 - i), -0.75f, 0);
        }
    }

    // myCardsでのインデックスをx座標の関係と同期
    void KeepMyCardsFollowingThePositions()
    {
        if(hoveringIndex != -1) return;
        // バブルソートで実装
        int n = myCards.Count;
        for(int i = 0; i < n - 1; i++)
        {
            for(int j = i + 1; j < n; j++)
            {
                if(myCards[i].cardObject.transform.localPosition.x <= myCards[j].cardObject.transform.localPosition.x)continue;
                (myCards[i].cardIdx, myCards[j].cardIdx) = (myCards[j].cardIdx, myCards[i].cardIdx);
                (myCards[i], myCards[j]) = (myCards[j], myCards[i]);
            }
        }
    }

    // printデバッグ用関数
    void printMyCards()
    {
        string arrange = "";
        for(int i = 0; i < myCards.Count; i++)
        {
            arrange += myCards[i].cardIdx.ToString() + ", ";
        }
        Debug.Log(arrange);
    }

    void Update()
    {
        // printMyCards();
        KeepMyCardsFollowingThePositions();printMyCards();
        KeepMyCardsFollowingThePositions();
    }
}
