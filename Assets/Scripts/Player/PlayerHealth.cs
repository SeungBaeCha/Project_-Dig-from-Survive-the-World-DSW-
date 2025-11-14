using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("체력 설정")]
    public float maxHealth = 100f;
    private float currentHealth;
    public HPBar hpBar; // HP바 UI 참조

    void Start()
    {
        // 체력 초기화 및 HP바 업데이트
        currentHealth = maxHealth;
        if (hpBar != null)
        {
            hpBar.UpdateHP(currentHealth, maxHealth);
        }
    }

    void Update()
    {
        // --- 테스트용 데미지 코드 ---
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(10);
        }
#endif
        // -------------------------
    }

    // 데미지를 받는 함수
    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        if (hpBar != null)
        {
            hpBar.UpdateHP(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 사망 처리 함수
    private void Die()
    {
        Debug.Log("플레이어가 사망했습니다.");
        // 나중에 여기에 게임 오버 로직이나 부활 로직을 추가할 수 있습니다.
    }
}
