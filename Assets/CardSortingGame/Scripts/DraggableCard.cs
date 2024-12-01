using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform cardRect; // カードのRectTransform
    private CanvasGroup canvasGroup; // CanvasGroupコンポーネント
    private Vector2 startPosition; // ドラッグ開始時の位置
    public Transform startSlot; // 元の親スロット

    // ドラッグ可能かどうかを制御するフラグ(自分のカードならばtrue, そうでないならfalseになっている)
    public bool isDraggable = true;
    private NetworkSystem networkSystem;

    void Awake()
    {
        cardRect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        networkSystem = FindObjectOfType<NetworkSystem>();
    }

    // ドラッグ開始時の処理
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        startSlot = transform.parent; // 元の親を記録
        startPosition = cardRect.anchoredPosition; // 元の位置を記録

        Transform curParent = networkSystem.questionController.GetCurrentParent();
        transform.SetParent(curParent, false);
        canvasGroup.alpha = 0.6f; // 透明度を下げる
        canvasGroup.blocksRaycasts = false;
        
        transform.localScale *= 1.2f;
    }

    // ドラッグ中の処理
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        Vector2 localPoint;
        RectTransform rect = transform.parent.GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            cardRect.anchoredPosition = localPoint;
        }
    }

    // ドラッグ終了時の処理
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        canvasGroup.alpha = 1f; // 透明度を元に戻す
        canvasGroup.blocksRaycasts = true; // レイキャストを有効にする

        // カードのスケールを元に戻す
        transform.localScale = Vector3.one;

        // 有効なスロットにドロップされなかった場合、元の位置に戻す
        if (transform.parent == networkSystem.questionController.GetCurrentParent())
        {
            transform.SetParent(startSlot, false); // 元の親に戻す
            cardRect.anchoredPosition = startPosition; // 元の位置に戻す
        }
    }
}
