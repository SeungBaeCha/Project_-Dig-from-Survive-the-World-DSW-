using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    public Slider hpSlider;

    // HP 값을 받아서 슬라이더의 값을 업데이트하는 함수
    // currentHealth: 현재 체력, maxHealth: 최대 체력
    public void UpdateHP(float currentHealth, float maxHealth)
    {
        // 슬라이더의 값은 0과 1 사이어야 하므로, 현재 체력을 최대 체력으로 나눈다.
        hpSlider.value = currentHealth / maxHealth;
    }
}
