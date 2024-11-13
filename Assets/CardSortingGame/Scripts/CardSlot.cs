using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Image slotImage;
    public bool isMySlot = false; // 自分のスロットかどうかを示すフラグ
    
    void Awake()
    {
        slotImage = GetComponent<Image>();
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
                if (currentCard != droppedCard.transform)
                {
                    // 既存のカードを元のスロットに戻す
                    currentCard.SetParent(initialSlot, false); // 元のスロットに戻す
                    currentCard.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // スロットの中心に配置
                }
            }

            // ドロップされたカードを新しいスロットに移動
            droppedCard.transform.SetParent(dropSlot, false); // スロットの子に設定（ローカル座標を維持）
            droppedCard.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // スロットの中心に配置
            
            // myCardsリスト内の順序を同期
            if (initialIndex >= 0 && dropIndex >= 0)
            {
                cardsManager.SwapCardsInList(initialIndex, dropIndex);
            }
            
            // プリントデバッグ
            cardsManager.printMyCards();
        }
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
