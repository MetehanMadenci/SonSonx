using UnityEngine;
using UnityEngine.EventSystems;

public class DragUI : MonoBehaviour, IDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchoredPosition += eventData.delta;
    }
}

