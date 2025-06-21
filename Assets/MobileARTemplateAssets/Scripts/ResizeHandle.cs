using UnityEngine;
using UnityEngine.EventSystems;

public class ResizeHandle : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    public RectTransform targetRect;
    public enum HandleType { TopLeft, TopRight, BottomLeft, BottomRight }
    public HandleType handleType;

    private bool isDragging = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector2 delta = eventData.delta;

#if UNITY_ANDROID || UNITY_IOS
        delta *= 1.0f; // gerekirse hız ayarı yapılabilir
#endif

        Vector2 size = targetRect.sizeDelta;
        Vector2 pos = targetRect.anchoredPosition;

        switch (handleType)
        {
            case HandleType.TopLeft:
                size += new Vector2(-delta.x, delta.y);
                pos += new Vector2(delta.x / 2, delta.y / 2);
                break;
            case HandleType.TopRight:
                size += new Vector2(delta.x, delta.y);
                pos += new Vector2(delta.x / 2, delta.y / 2);
                break;
            case HandleType.BottomLeft:
                size += new Vector2(-delta.x, -delta.y);
                pos += new Vector2(delta.x / 2, delta.y / 2);
                break;
            case HandleType.BottomRight:
                size += new Vector2(delta.x, -delta.y);
                pos += new Vector2(delta.x / 2, delta.y / 2);
                break;
        }

        size = Vector2.Max(size, new Vector2(100, 100));
        targetRect.sizeDelta = size;
        targetRect.anchoredPosition = pos;
    }
}
