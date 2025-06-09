using UnityEngine;
using UnityEngine.UI;

public class SliderScaleController : MonoBehaviour
{
    public Slider scaleSlider;
    private GameObject targetModel;
    private Vector3 initialScale = Vector3.one;
    private float lastValue = 1f;

    void Start()
    {
        if (scaleSlider != null)
        {
            scaleSlider.onValueChanged.AddListener(OnScaleChanged);
        }
    }

    public void SetTargetModel(GameObject model)
    {
        targetModel = model;
        if (model != null)
        {
            initialScale = model.transform.localScale;
            lastValue = 1f;
            scaleSlider.value = lastValue;
        }
    }

    private void OnScaleChanged(float value)
    {
        if (targetModel == null) return;

        // Küçük deðiþikliklerde güncelleme yapma
        if (Mathf.Abs(value - lastValue) < 0.01f) return;

        targetModel.transform.localScale = initialScale * value;
        lastValue = value;
    }
}
