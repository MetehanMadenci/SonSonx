using UnityEngine;
using UnityEngine.EventSystems;

public class EmptyClickHandlerXR : MonoBehaviour
{
    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 screenPos = Input.mousePosition;
            if (!IsPointerOverUI(screenPos))
                HandleRay(screenPos);
        }
#else
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            Vector2 screenPos = Input.GetTouch(0).position;
            if (!IsPointerOverUI(screenPos))
                HandleRay(screenPos);
        }
#endif
    }

    private void HandleRay(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        bool hitBone = false;

        foreach (var hit in hits)
        {
            if (hit.transform.GetComponent<XRBoneClickHandler>() != null ||
                hit.transform.GetComponentInParent<XRBoneClickHandler>() != null)
            {
                hitBone = true;
                break;
            }
        }

        if (!hitBone)
        {
            XRUIManager.Instance?.ClearSelectionAndUI();
        }
    }

    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        return results.Count > 0;
    }
}
