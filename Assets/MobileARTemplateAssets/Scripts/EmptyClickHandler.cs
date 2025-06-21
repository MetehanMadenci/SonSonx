using UnityEngine;
using UnityEngine.EventSystems;

public class EmptyClickHandler : MonoBehaviour
{
    public UIManager uiManager;
    public Camera arCamera;

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            Vector2 touchPosition = Input.GetTouch(0).position;

            if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                Ray ray = arCamera.ScreenPointToRay(touchPosition);
                if (!Physics.Raycast(ray))
                {
                    uiManager.ClearSelectionAndUI();
                }
            }
        }

#if UNITY_EDITOR
        if (Input.GetMouseButtonUp(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                Ray ray = arCamera.ScreenPointToRay(Input.mousePosition);
                if (!Physics.Raycast(ray))
                {
                    uiManager.ClearSelectionAndUI();
                }
            }
        }
#endif
    }
}
