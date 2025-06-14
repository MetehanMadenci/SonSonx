using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class XRDragRotate : MonoBehaviour
{
    [Header("Speed Settings")]
    public float rotationSpeed = 0.2f;

    private Vector2 prevTouchPos;
    private bool isDragging = false;
    private bool isTouchingModel = false;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private float touchStartTime;
    private const float clickTimeThreshold = 0.3f;
    private const float clickMoveThreshold = 20f;

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
            {
                TrySelectBone(Mouse.current.position.ReadValue());
            }
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
                return;
            }

            if (!isTouchingModel) return;

            Vector2 delta = curPos - prevTouchPos;
            prevTouchPos = curPos;

            RotateYOnly(delta);
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
            return;
        }

        if (touch.press.isPressed && isTouchingModel)
        {
            Vector2 delta = touchPos - prevTouchPos;

            if (!isDragging && delta.magnitude > clickMoveThreshold / 2f)
            {
                isDragging = true;
            }

            if (isDragging)
            {
                RotateYOnly(delta);
            }

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
    }

    // -------------------- COMMON FUNCTIONS --------------------
    void TrySelectBone(Vector2 screenPos)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(screenPos);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (var hit in hits)
        {
            var clickHandler = hit.transform.GetComponent<XRBoneClickHandler>();
            if (clickHandler != null)
            {
                clickHandler.SelectBone();
                return;
            }

            clickHandler = hit.transform.GetComponentInParent<XRBoneClickHandler>();
            if (clickHandler != null)
            {
                clickHandler.SelectBone();
                return;
            }
        }
    }

    void RotateYOnly(Vector2 delta)
    {
        Vector3 pivot = GetRendererBoundsCenter(gameObject);
        transform.RotateAround(pivot, Vector3.up, -delta.x * rotationSpeed);
    }

    Vector3 GetRendererBoundsCenter(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return go.transform.position;

        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }
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
}