using System.Collections.Generic;
using UnityEngine;

public class CardsManager : MonoBehaviour
{
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
        {
            GameObject cardObject = cardObjects[i];
            AddDragFunctionality(cardObject, i); // ドラッグ機能を追加
            CardClass card = new CardClass(cardObject, idx[i]);
            myCards.Add(card);
            // Debug.Log(idx[i]);
        }
    }

    // ランダムインデックス生成
    public int[] GenRandomIdx(int origin, int len)
    {
        int[] idx = new int[len];
        for (int i = 0; i < len; i++)
        {
            idx[i] = i + origin;
        }
        for (int i = 0; i < len; i++)
        {
            int j = Random.Range(0, len);
            (idx[i], idx[j]) = (idx[j], idx[i]);
        }
        return idx;
    }

    // ドラッグ機能をカードに追加
    private void AddDragFunctionality(GameObject card, int cardIndex)
    {
        card.AddComponent<BoxCollider2D>(); // コライダーを追加

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
        // マウスの位置にカードを追従させる
        Vector3 newPosition = Input.mousePosition;
        newPosition.z = 10f; // カメラの適切な距離
        card.transform.position = Camera.main.ScreenToWorldPoint(newPosition);

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
        // ドロップ時の処理 (以前と同じ)
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
        printMyCards();
        KeepMyCardsFollowingThePositions();
    }
}
