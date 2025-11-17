using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("체력 설정")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("배고픔 설정")]
    public float maxHunger = 100f;
    public float currentHunger;

    public HPBar hpBar; // HP바 UI 참조

    [Header("게임 오버 설정")]
    public GameObject gameoverPanel;




    void Start()
    {

        // 게임이 재시작 될 수 있어 시작할 떄 항상 시간을 원래대로 놓는다. (게임실행)
        Time.timeScale = 1f;

        // 체력 초기화 및 HP바 업데이트
        currentHealth = maxHealth;
        //currentHunger = maxHunger; // 배고픔 초기화
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

    // 배고픔을 감소시키는 함수
    public void DecreaseHunger(float amount)
    {
        if (currentHunger <= 0) return;

        currentHunger -= amount;
        if (currentHunger < 0)
        {
            currentHunger = 0;
        }
    }

    // 사망 처리 함수
    private void Die()
    {
        // 나중에 여기에 게임 오버 로직이나 부활 로직을 추가.
        Debug.Log("플레이어가 사망했습니다.");

        if (gameoverPanel != null)
        {
            gameoverPanel.SetActive(true);
        }


        // 게임시간을 멈추기
        Time.timeScale = 0f;
    }
}
