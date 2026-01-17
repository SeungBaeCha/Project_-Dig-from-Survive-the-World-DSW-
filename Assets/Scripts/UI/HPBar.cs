using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용을 위해 추가

public class HPBar : MonoBehaviour
{
    [Header("체력 UI")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    [Header("허기 UI")]
    public Slider hungerSlider;
    public TextMeshProUGUI hungerText;

    /// <summary>
    /// HP 값을 받아서 HP 슬라이더의 값과 텍스트를 업데이트하는 함수
    /// </summary>
    public void UpdateHP(float currentHealth, float maxHealth)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth; // 최대 체력 설정
            hpSlider.value = currentHealth; // 현재 체력 설정
        }

        if (hpText != null)
        {
            hpText.text = $"HP : {Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }

    /// <summary>
    /// Hunger 값을 받아서 허기 슬라이더의 값과 텍스트를 업데이트하는 함수
    /// </summary>
    public void UpdateHunger(float currentHunger, float maxHunger)
    {
        if (hungerSlider != null)
        {
            hungerSlider.maxValue = maxHunger; // 최대 허기 설정
            hungerSlider.value = currentHunger; // 현재 허기 설정
        }

        if (hungerText != null)
        {
            hungerText.text = $"Hunger : {Mathf.CeilToInt(currentHunger)} / {Mathf.CeilToInt(maxHunger)}";
        }
    }
}