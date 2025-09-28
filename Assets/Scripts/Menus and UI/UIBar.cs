using System;
using UnityEngine;
using UnityEngine.UI;

//Used for the Volume bars
public class UIBar : MonoBehaviour
{

    public Action<float> ValueChanged;

    [Header("References")]
    [SerializeField] Image barFill;

    public Button leftArrow;
    public Button rightArrow;

    [Header("Attributes")]
    [SerializeField] float maxValue;

    [SerializeField] float value;

    public void SetValue(float perc)
    {
        value = perc * maxValue;
    }
    // Start is called before the first frame update
    void Start()
    {
        leftArrow.onClick.AddListener(ReduceValue);
        rightArrow.onClick.AddListener(AddValue);
    }

    // Update is called once per frame
    void Update()
    {
        SetBarFill();
    }

    void SetBarFill()
    {
        barFill.fillAmount = value / maxValue;
    }

    void ReduceValue()
    {
        if (value > 0)
        {
            value -= 1;
            ValueChanged?.Invoke(value / maxValue);
        }
    }

    void AddValue()
    {
        if (value < maxValue)
        {
            value += 1;
            ValueChanged?.Invoke(value / maxValue);
        }
    }
}
