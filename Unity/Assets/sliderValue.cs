using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class sliderValue : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI textComp; 
    public PlayerManager pm;

    void Awake()
    {
        slider = GetComponentInParent<Slider>();
        textComp = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        UpdateText(slider.value);
        slider.onValueChanged.AddListener(UpdateText);
        slider.value = 30f;
    }

    void UpdateText(float val)
    {
        textComp.text = slider.value.ToString();
        
    }

    void Update()
    {
        pm.duration = (int)slider.value;
        pm.TimeLeft.text = slider.value.ToString();
        pm.currentTime = (int)slider.value;
    }
}
