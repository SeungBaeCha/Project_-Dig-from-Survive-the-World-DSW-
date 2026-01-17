using UnityEngine;
using UnityEngine.SceneManagement; // SceneManagement를 사용하기 위해 추가

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
        // currentHunger = maxHunger; // 인스펙터에서 설정된 초기값을 사용하도록 변경
        if (hpBar != null)
        {
            hpBar.UpdateHP(currentHealth, maxHealth);
            hpBar.UpdateHunger(currentHunger, maxHunger); // 배고픔 UI도 시작 시 업데이트
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

        // 플레이어의 UI 입력 비활성화
        UIManager.Instance.DisablePlayerInput();
    }

    /// <summary>
    /// 게임 오버 후 게임을 재시작하는 함수.
    /// 게임 재시작 버튼의 OnClick 이벤트에 연결된다.
    /// </summary>
    public void RestartGame()
    {
        // 게임 시간을 다시 정상으로 설정
        Time.timeScale = 1f;

        // 플레이어의 UI 입력을 다시 활성화
        UIManager.Instance.EnablePlayerInput();

        // 게임 오버 패널 비활성화
        if (gameoverPanel != null)
        {
            gameoverPanel.SetActive(false);
        }

        // 현재 씬을 다시 로드하여 게임 재시작
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 게임을 종료하는 함수.
    /// 게임 종료 버튼의 OnClick 이벤트에 연결된다.
    /// 에디터에서는 플레이 모드를 중지하고, 빌드에서는 애플리케이션을 종료한다.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("게임을 종료합니다.");

#if UNITY_EDITOR
        // 유니티 에디터에서 실행 중일 때는 플레이 모드를 중지
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 애플리케이션에서는 애플리케이션 종료
        Application.Quit();
#endif
    }
}