using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DragRotate : MonoBehaviour
{
    [Header("Speed Settings")]
    public float rotationSpeed = 0.2f;
    public float panSpeed = 0.005f;
    public float zoomSpeed = 0.02f;

    [Header("Zoom Limits")]
    public float minZoomDistance = 0.5f;
    public float maxZoomDistance = 15f;

    private Vector2 prevTouchPos;
    private bool isDragging = false;
    private bool isTouchingModel = false;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private Vector2 prevTwoFingerAvg;
    private float prevTouchDistance = -1f;
    private bool isZooming = false;

    private float touchStartTime;
    private const float clickTimeThreshold = 0.3f;
    private const float clickMoveThreshold = 20f;

    private bool hasHiddenImage = false;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public void ResetTransform()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseRotation();
        HandleMouseZoom();
#else
        HandleMobileInput();
#endif
    }

    // -------------------- MOUSE CONTROLS --------------------
    void HandleMouseRotation()
    {
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (!isDragging && !IsPointerOverUI(Mouse.current.position.ReadValue()))
                TrySelectBone(Mouse.current.position.ReadValue());

            isDragging = false;
            return;
        }

        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 curPos = Mouse.current.position.ReadValue();

            if (!isDragging)
            {
                isDragging = true;
                prevTouchPos = curPos;
                isTouchingModel = CheckIfHitsModel(curPos) && !IsPointerOverUI(curPos);
                HideImageOnFirstTouch(); // 🔹 İlk dokunuşta görseli gizle
                return;
            }

            if (!isTouchingModel) return;

            Vector2 delta = curPos - prevTouchPos;
            prevTouchPos = curPos;

            RotateXYLocal(delta);
        }
    }

    void HandleMouseZoom()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 zoomTarget = GetRendererBoundsCenter(gameObject);
            Vector3 zoomDir = (zoomTarget - Camera.main.transform.position).normalized;
            float currentDistance = Vector3.Distance(Camera.main.transform.position, zoomTarget);
            float targetDistance = currentDistance - (scroll * zoomSpeed);
            targetDistance = Mathf.Clamp(targetDistance, minZoomDistance, maxZoomDistance);
            Camera.main.transform.position = zoomTarget - zoomDir * targetDistance;
        }
    }

    // -------------------- TOUCH CONTROLS --------------------
    void HandleMobileInput()
    {
        if (Touchscreen.current == null || Touchscreen.current.primaryTouch == null) return;

        var touch = Touchscreen.current.primaryTouch;
        Vector2 touchPos = touch.position.ReadValue();

        if (touch.press.wasPressedThisFrame)
        {
            touchStartTime = Time.time;
            prevTouchPos = touchPos;
            isDragging = false;
            isTouchingModel = CheckIfHitsModel(touchPos) && !IsPointerOverUI(touchPos);
            HideImageOnFirstTouch(); // 🔹 İlk dokunuşta görseli gizle
            return;
        }

        if (touch.press.isPressed && isTouchingModel)
        {
            Vector2 delta = touchPos - prevTouchPos;

            if (!isDragging && delta.magnitude > clickMoveThreshold / 2f)
                isDragging = true;

            if (isDragging)
                RotateXYLocal(delta);

            prevTouchPos = touchPos;
        }

        if (touch.press.wasReleasedThisFrame)
        {
            float heldTime = Time.time - touchStartTime;
            float moveDist = Vector2.Distance(touchPos, prevTouchPos);

            if ((!isDragging || heldTime < clickTimeThreshold / 2f) &&
                heldTime < clickTimeThreshold &&
                moveDist < clickMoveThreshold &&
                isTouchingModel)
            {
                TrySelectBone(touchPos);
            }

            isDragging = false;
        }

        // Handle multi-touch for zoom/pan
        int touchCount = 0;
        Vector2 avgPos = Vector2.zero;
        foreach (var t in Touchscreen.current.touches)
        {
            if (t.press.isPressed)
            {
                avgPos += t.position.ReadValue();
                touchCount++;
            }
        }

        if (touchCount >= 2)
{
    avgPos /= touchCount;
    isTouchingModel = false; // İki parmak varsa rotasyonu engelle
    HandleTwoFingerPanAndZoom(avgPos);
    return; // 🔒 ROTATE'i engelle
}

        else
        {
            isZooming = false;
        }
    }

    void HandleTwoFingerPanAndZoom(Vector2 avgPos)
{
    Vector2 p1 = Touchscreen.current.touches[0].position.ReadValue();
    Vector2 p2 = Touchscreen.current.touches[1].position.ReadValue();

    float currentDistance = Vector2.Distance(p1, p2);
    Vector2 currentAvg = (p1 + p2) / 2f;

    if (!isZooming)
    {
        isZooming = true;
        prevTwoFingerAvg = currentAvg;
        prevTouchDistance = currentDistance;
        return;
    }

    Vector2 panDelta = currentAvg - prevTwoFingerAvg;

    // PAN hareketi
    Vector3 panMovement = new Vector3(panDelta.x * panSpeed, panDelta.y * panSpeed, 0);
    transform.Translate(panMovement, Space.World);

    // Limitleme: Model kameradan çok uzaklaşmasın
    Vector3 modelCenter = GetRendererBoundsCenter(gameObject);
    Vector3 screenPos = Camera.main.WorldToViewportPoint(modelCenter);
    if (screenPos.x < 0.1f || screenPos.x > 0.9f || screenPos.y < 0.1f || screenPos.y > 0.9f)
    {
        // Geri al pan hareketini
        transform.Translate(-panMovement, Space.World);
    }

    // Zoom hareketi
    float zoomDelta = currentDistance - prevTouchDistance;
    Vector3 zoomTarget = GetCurrentZoomTarget();
    Vector3 zoomDir = (zoomTarget - Camera.main.transform.position).normalized;
    float targetDistance = Vector3.Distance(Camera.main.transform.position, zoomTarget) - (zoomDelta * zoomSpeed);
    targetDistance = Mathf.Clamp(targetDistance, minZoomDistance, maxZoomDistance);
    Camera.main.transform.position = zoomTarget - zoomDir * targetDistance;

    prevTwoFingerAvg = currentAvg;
    prevTouchDistance = currentDistance;
}


    Vector3 GetCurrentZoomTarget()
    {
        BoneClickHandler selectedBone = FindSelectedBone();
        if (selectedBone != null)
            return selectedBone.transform.position;

        return GetRendererBoundsCenter(gameObject);
    }

    BoneClickHandler FindSelectedBone()
    {
        BoneClickHandler[] handlers = FindObjectsOfType<BoneClickHandler>();
        foreach (var handler in handlers)
        {
            if (handler.gameObject.activeInHierarchy)
                return handler;
        }
        return null;
    }

    void TrySelectBone(Vector2 screenPos)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (var hit in hits)
        {
            var clickHandler = hit.transform.GetComponent<BoneClickHandler>();
            if (clickHandler != null)
            {
                clickHandler.SelectBone();
                return;
            }

            clickHandler = hit.transform.GetComponentInParent<BoneClickHandler>();
            if (clickHandler != null)
            {
                clickHandler.SelectBone();
                return;
            }
        }
    }

    void RotateXYLocal(Vector2 delta)
    {
        Vector3 pivot = GetRendererBoundsCenter(gameObject);
        transform.RotateAround(pivot, Vector3.up, -delta.x * rotationSpeed);
        transform.RotateAround(pivot, Camera.main.transform.right, delta.y * rotationSpeed);
    }

    Vector3 GetRendererBoundsCenter(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return go.transform.position;

        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers)
            bounds.Encapsulate(r.bounds);
        return bounds.center;
    }

    bool CheckIfHitsModel(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        return Physics.Raycast(ray, out RaycastHit hit) && hit.transform.IsChildOf(transform);
    }

    bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null) return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        return results.Count > 0;
    }

    void HideImageOnFirstTouch()
    {
        if (!hasHiddenImage && UIManager.Instance != null && UIManager.Instance.imageToHideOnTouch != null)
        {
            UIManager.Instance.imageToHideOnTouch.SetActive(false);
            hasHiddenImage = true;
        }
    }
}
