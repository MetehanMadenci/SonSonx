using UnityEngine;

public class PinchToScale : MonoBehaviour
{
    [Header("Scale Ayarlarý")]
    public float scaleSpeed = 0.01f;
    public float minScale = 0.1f;
    public float maxScale = 2f;

    private float initialDistance;
    private Vector3 initialScale;

    void Update()
    {
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            // Ýlk frame'de baþlangýç mesafesini al
            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                initialDistance = Vector2.Distance(touch0.position, touch1.position);
                initialScale = transform.localScale;
            }
            else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                float currentDistance = Vector2.Distance(touch0.position, touch1.position);
                if (Mathf.Approximately(initialDistance, 0)) return;

                float scaleFactor = currentDistance / initialDistance;
                Vector3 newScale = initialScale * scaleFactor;

                // Scale sýnýrlarýný uygula
                newScale = Vector3.Max(Vector3.one * minScale, Vector3.Min(newScale, Vector3.one * maxScale));
                transform.localScale = newScale;
            }
        }
    }
}
