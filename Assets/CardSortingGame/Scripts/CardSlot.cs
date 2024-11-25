using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class CardSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Canvas canvas;
    private Image slotImage;
    public bool isMySlot = false; // 自分のスロットかどうかを示すフラグ

    public AudioClip cardSound;
    public GameObject cardSoundObject;
    
    void Awake()
    {
        slotImage = GetComponent<Image>();
        canvas = FindObjectOfType<Canvas>();
    }

    // ドロップ時の処理
    public void OnDrop(PointerEventData eventData)
    {
        if (!isMySlot) return;
        
        // ドラッグされたオブジェクトからDraggableCardスクリプトを取得
        DraggableCard droppedCard = eventData.pointerDrag.GetComponent<DraggableCard>();
        if (droppedCard != null && droppedCard.isDraggable)
        {
            Transform initialSlot = droppedCard.startSlot; // ドラッグ開始時の元のスロット
            Transform dropSlot = transform; // ドロップ先のスロット

            CardsManager cardsManager = FindObjectOfType<CardsManager>();
            int initialIndex = GetCardIndexInList(droppedCard.gameObject, cardsManager);
            int dropIndex = GetSlotIndexInList(dropSlot.gameObject, cardsManager);
            
            // スロットに既にカードが存在するかチェック
            if (dropSlot.childCount > 0)
            {
                // スロットに他のカードがある場合は、スワップ処理を行う
                Transform currentCard = dropSlot.GetChild(0);
                GameObject soundobj=Instantiate(cardSoundObject);
                soundobj.GetComponent<PlaySound>().PlaySE(cardSound);
                if (currentCard != droppedCard.transform)
                {
                    // 既存のカードを元のスロットにアニメーションで移動
                    StartCoroutine(MoveCardToSlot(currentCard, initialSlot));
                }
            }

            // ドロップされたカードを新しいスロットにアニメーションで移動
            StartCoroutine(MoveCardToSlot(droppedCard.transform, dropSlot));
            
            // myCardsリスト内の順序を同期
            if (initialIndex >= 0 && dropIndex >= 0)
            {
                cardsManager.SwapCardsInList(initialIndex, dropIndex);
            }
            
            // プリントデバッグ
            cardsManager.printMyCards();
        }
    }

    // カードをスロットにスムーズに移動させるコルーチン
    private IEnumerator MoveCardToSlot(Transform card, Transform slot)
    {
        // カードの親をCanvasに設定
        card.SetParent(canvas.transform, true);

        // 開始位置と終了位置
        Vector3 startPos = card.position;
        Vector3 endPos = slot.position;

        float animationTime = 0.2f; // アニメーションの長さ
        float currentTime = 0f; // 経過時間

        while (currentTime < animationTime)
        {
            float progress = currentTime / animationTime;
            // progress = Mathf.SmoothStep(0f, 1f, progress);
            card.position = Vector3.Lerp(startPos, endPos, progress);
            currentTime += Time.deltaTime;
            yield return null;
        }
        
        card.position = endPos; // 最終位置を設定

        // カードの親をスロットに設定し、位置をリセット
        card.SetParent(slot, false);
        card.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    // ドロップされたカードのインデックスを取得
    private int GetCardIndexInList(GameObject card, CardsManager cardsManager)
    {
        for (int i = 0; i < cardsManager.myCards.Count; i++)
            if (cardsManager.myCards[i].cardObject == card)
                return i;
        return -1;
    }

    // スロットのインデックスを取得
    private int GetSlotIndexInList(GameObject slot, CardsManager cardsManager)
    {
        for (int i = 0; i < cardsManager.myCards.Count; i++)
            if (cardsManager.myCards[i].cardObject.transform.parent == slot.transform)
                return i;
        return -1;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (slotImage != null)
        {
            slotImage.color = new Color(slotImage.color.r, slotImage.color.g, slotImage.color.b, 0.5f); // 半透明に変更
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (slotImage != null)
        {
            slotImage.color = new Color(slotImage.color.r, slotImage.color.g, slotImage.color.b, 0f); // 透明に戻す
        }
    }
}
