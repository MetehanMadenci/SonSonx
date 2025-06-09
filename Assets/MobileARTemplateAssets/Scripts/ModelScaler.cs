using UnityEngine;

public class ModelScaler : MonoBehaviour
{
    private Vector3 initialScale;

    private void Awake()
    {
        initialScale = transform.localScale;
    }

    public void SetScale(float scaleValue)
    {
        transform.localScale = initialScale * scaleValue;
    }
}
