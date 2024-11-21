using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform cardRect; // カードのRectTransform
    private CanvasGroup canvasGroup; // CanvasGroupコンポーネント
    private Canvas canvas; // Canvasの参照
    private Vector2 startPosition; // ドラッグ開始時の位置
    public Transform startSlot; // 元の親スロット

    // ドラッグ可能かどうかを制御するフラグ(自分のカードならばtrue, そうでないならfalseになっている)
    public bool isDraggable = true;

    void Awake()
    {
        cardRect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    // ドラッグ開始時の処理
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        startSlot = transform.parent; // 元の親を記録
        startPosition = cardRect.anchoredPosition; // 元の位置を記録

        // カードをCanvasの直下に移動して最前面に表示
        transform.SetParent(canvas.transform, false); // 親をCanvasに変更（ローカル座標を維持）
        canvasGroup.alpha = 0.6f; // 透明度を下げる
        canvasGroup.blocksRaycasts = false; // レイキャストをブロックしないように変更
        
        transform.localScale *= 1.1f; // スケールを10%拡大
    }

    // ドラッグ中の処理
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        Vector2 localPoint;
        // CanvasのRectTransformに基づいてスクリーン座標をローカル座標に変換
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out localPoint))
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
        if (transform.parent == canvas.transform)
        {
            transform.SetParent(startSlot, false); // 元の親に戻す
            cardRect.anchoredPosition = startPosition; // 元の位置に戻す
        }
    }
}
