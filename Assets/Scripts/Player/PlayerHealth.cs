using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("체력 설정")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("배고픔 설정")]
    public float maxHunger = 100f;
    public float currentHunger;
    private float hungerDamageTimer = 0f; // 배고픔 데미지 타이머

    public HPBar hpBar; // HP바 UI 참조

    [Header("게임 오버 설정")]
    public GameObject gameoverPanel;

    void Start()
    {
        // 게임이 재시작되었을 수 있어 시작할 때 항상 시간을 원래대로 놓는다 (게임실행)
        Time.timeScale = 1f;

        // 체력 초기화 및 HP바 업데이트
        currentHealth = maxHealth;
        currentHunger = maxHunger; // 배고픔 초기화
        if (hpBar != null)
        {
            hpBar.UpdateHP(currentHealth, maxHealth);
        }

        // 시작할때 gameoverPanel이 꺼져있도록 처리
        if (gameoverPanel != null)
        {
            gameoverPanel.SetActive(false);
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
        if (Input.GetKeyDown(KeyCode.Y)) // 배고픔 수치 감소 테스트
        {
            currentHunger = 0;
        }
#endif
        // -------------------------

        // 배고픔 수치가 0 이하면 1초마다 10의 데미지를 입는다.
        if (currentHunger <= 0)
        {
            // 시간의 흐름을 기록한다.
            hungerDamageTimer += Time.deltaTime;
            // 1초가 지났는지 확인한다.
            if (hungerDamageTimer >= 1f)
            {
                // 배고픔 데미지를 입힌다.
                TakeHungerDamage(10f);
                // 타이머를 리셋한다.
                hungerDamageTimer = 0f;
            }
        }
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
    
    // 배고픔 데미지를 받는 함수
    private void TakeHungerDamage(float damage)
    {
        // 체력이 10보다 클 때만 데미지를 입는다.
        if (currentHealth > 10)
        {
            currentHealth -= damage;
            // 데미지를 입은 후 체력이 10보다 낮아지면 10으로 고정한다.
            if (currentHealth < 10)
            {
                currentHealth = 10;
            }

            if (hpBar != null)
            {
                hpBar.UpdateHP(currentHealth, maxHealth);
            }
        }
    }


    // 배고픔을 감소시키는 함수
    public void DecreaseHunger(float amount)
    {
        if (currentHunger <= 0) return;

        currentHunger -= amount;
        if (currentHunger < 0)
        {
            currentHunger = 0;
        }
        
        // 허기 UI 업데이트
        if (hpBar != null)
        {
            hpBar.UpdateHunger(currentHunger, maxHunger);
        }
    }

    /// <summary>
    /// 소모품 아이템을 사용해서 체력과 허기를 회복합니다.
    /// </summary>
    /// <param name="consumable">사용할 아이템의 ConsumableData</param>
    public void UseConsumable(ConsumableData consumable)
    {
        // 체력 회복
        if (consumable.healthToRestore > 0)
        {
            currentHealth += consumable.healthToRestore;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // 최대 체력을 넘지 않도록
            Debug.Log($"체력을 {consumable.healthToRestore}만큼 회복했습니다. 현재 체력: {currentHealth}");
        }

        // 허기 회복
        if (consumable.hungerToRestore > 0)
        {
            currentHunger += consumable.hungerToRestore;
            currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger); // 최대 허기를 넘지 않도록
            Debug.Log($"허기를 {consumable.hungerToRestore}만큼 회복했습니다. 현재 허기: {currentHunger}");
        }

        // UI 업데이트
        if (hpBar != null)
        {
            hpBar.UpdateHP(currentHealth, maxHealth);
            hpBar.UpdateHunger(currentHunger, maxHunger);
        }
    }

    // 사망 처리 함수
    private void Die()
    {
        // 나중에 여기에 게임 오버 로직이나 부활 로직을 추가.
        Debug.Log("플레이어가 사망했다.");

        if (gameoverPanel != null)
        {
            gameoverPanel.SetActive(true);
        }

        // 게임시간 멈추기
        Time.timeScale = 0f;
    }
}