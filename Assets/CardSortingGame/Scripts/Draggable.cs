using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private UnityAction onBeginDrag;
    private UnityAction onDrag;
    private UnityAction onEndDrag;

    public void Initialize(UnityAction onBeginDrag, UnityAction onDrag, UnityAction onEndDrag)
    {
        this.onBeginDrag = onBeginDrag;
        this.onDrag = onDrag;
        this.onEndDrag = onEndDrag;
    }

    // ドラッグ開始時に呼び出される
    public void OnBeginDrag(PointerEventData eventData)
    {
        onBeginDrag?.Invoke();
    }

    // ドラッグ中に呼び出される
    public void OnDrag(PointerEventData eventData)
    {
        onDrag?.Invoke();
        
        // マウスに追従させるためのコード
        Vector3 newPosition = eventData.position;
        newPosition.z = 0; // Z座標を0に固定する
        transform.position = newPosition;
    }

    // ドラッグ終了時に呼び出される
    public void OnEndDrag(PointerEventData eventData)
    {
        onEndDrag?.Invoke();
    }
}
