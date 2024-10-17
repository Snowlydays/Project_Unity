using UnityEngine;
using UnityEngine.Events;

public class Draggable : MonoBehaviour
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

    void OnMouseDown()
    {
        onBeginDrag?.Invoke();
    }

    void OnMouseDrag()
    {
        onDrag?.Invoke();
    }

    void OnMouseUp()
    {
        onEndDrag?.Invoke();
    }
}