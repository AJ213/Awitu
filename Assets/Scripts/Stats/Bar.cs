using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class Bar : MonoBehaviour
{

    [SerializeField] Text valueText = default;
    float maxValue;
    Slider slider;

    private void Awake()
    {
        slider = this.GetComponent<Slider>();
    }
    public void SetMaxValue(float maxValue)
    {
        if (slider == null)
            throw new UnityException("Slider not set");

        slider.maxValue = maxValue;
        slider.value = maxValue;
        this.maxValue = maxValue;
        SetText(maxValue, maxValue);
    }
    
    public void SetValue(float value)
    {
        if (slider == null)
            return;

        slider.value = value;
        SetText(value, maxValue);
    }

    void SetText(float value, float maxValue)
    {
        if (valueText != null)
        {
            valueText.text = Mathf.Ceil(value) + " / " + Mathf.Ceil(maxValue);
        }
    }
}
