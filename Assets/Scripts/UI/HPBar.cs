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
            // 슬라이더의 값은 0과 1 사이여야 하므로, 현재 체력을 최대 체력으로 나눈다.
            hpSlider.value = currentHealth / maxHealth;
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
            hungerSlider.value = currentHunger / maxHunger;
        }

        if (hungerText != null)
        {
            hungerText.text = $"Hunger : {Mathf.CeilToInt(currentHunger)} / {Mathf.CeilToInt(maxHunger)}";
        }
    }
}