using UnityEngine;
using UnityEngine.SceneManagement; // SceneManagement를 사용하기 위해 추가
using UnityEngine.UI; // UI Image 사용을 위해 추가
using Cinemachine; // 시네머신 사용을 위해 추가

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CinemachineImpulseSource))] // Cinemachine Impulse Source 추가
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
    
    [Header("피격 효과")] // 기존 피격 연출 및 사운드 설정 통합
    [Tooltip("체력이 낮을수록 강해지는 흔들림의 강도")]
    public float healthBasedImpulseMultiplier = 1f; // 체력에 따른 흔들림 강도 배율
    public AudioClip hitSound; // 피격 시 재생할 사운드
    [Range(0f, 1f)]
    public float hitSoundVolume = 0.5f; // 피격 사운드 볼륨
    [Tooltip("체력이 낮을수록 붉게 변하는 화면 효과")]
    public Image damageOverlay; // 화면 오버레이 이미지
    public Color damageOverlayColor = Color.red; // 오버레이 색상
    
    private AudioSource audioSource; // 사운드 재생을 위한 AudioSource
    private CinemachineImpulseSource impulseSource; // 카메라 흔들림을 위한 ImpulseSource

    void Start()
    {
        // 게임이 재시작되었을 수 있어 시작할 때 항상 시간을 원래대로 놓는다 (게임실행)
        Time.timeScale = 1f;

        audioSource = GetComponent<AudioSource>(); // AudioSource 컴포넌트 가져오기
        impulseSource = GetComponent<CinemachineImpulseSource>(); // ImpulseSource 컴포넌트 가져오기

        // 체력 초기화 및 HP바 업데이트
        currentHealth = maxHealth;
        // currentHunger = maxHunger; // 인스펙터에서 설정된 초기값을 사용하도록 변경
        if (hpBar != null)
        {
            hpBar.UpdateHP(currentHealth, maxHealth);
            hpBar.UpdateHunger(currentHunger, maxHunger); // 배고픔 UI도 시작 시 업데이트
        }
        
        UpdateDamageOverlay(); // 오버레이 초기화
    }

    void Update()
    {
        // --- 테스트용 데미지 코드 ---
//#if UNITY_EDITOR
//        if (Input.GetKeyDown(KeyCode.T))
//        {
//            TakeDamage(10);
//        }
//        if (Input.GetKeyDown(KeyCode.Y)) // 배고픔 수치 감소 테스트
//        {
//            currentHunger = 0;
//        }
//#endif
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
        UpdateDamageOverlay(); // 데미지를 입었을 때 오버레이 업데이트
        
        if (hpBar != null)
        {
            hpBar.UpdateHP(currentHealth, maxHealth);
        }
        
        // --- 피격 효과 ---
        // 1. 피격 사운드 재생
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound, hitSoundVolume);
        }

        // 2. 시네머신 Impulse를 사용한 카메라 흔들림
        // 체력이 낮을수록 더 강하게 흔들린다.
        float healthPercent = Mathf.Max(0, currentHealth) / maxHealth; // 0~1 사이의 체력 비율
        float impulseStrength = (1 - healthPercent) * healthBasedImpulseMultiplier;
        if (impulseSource != null) // impulseSource가 null이 아닌지 확인
        {
             impulseSource.GenerateImpulseWithVelocity(Random.insideUnitSphere.normalized * impulseStrength);
        }
        // ------------------

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
            UpdateDamageOverlay(); // 배고픔 데미지를 입었을 때 오버레이 업데이트

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
        
        UpdateDamageOverlay(); // 아이템 사용 후 오버레이 업데이트
    }

    // 사망 처리 함수
    private void Die()
    {
        Debug.Log("플레이어가 사망했다.");

        // 플레이어 사망 시 화면 오버레이 비활성화
        if (damageOverlay != null)
        {
            damageOverlay.gameObject.SetActive(false);
        }

        // 모든 사망 관련 처리를 GameManager에 위임합니다.
        GameManager.Instance.HandlePlayerDeath();
    }
    
    /// <summary>
    /// 체력 변화에 따라 화면 오버레이 효과를 업데이트합니다.
    /// </summary>
    private void UpdateDamageOverlay()
    {
        if (damageOverlay == null) return;

        float healthPercent = Mathf.Max(0, currentHealth) / maxHealth; // 0~1 사이의 체력 비율
        float alpha = 1.0f - healthPercent; // 체력이 낮을수록 alpha값이 1에 가까워짐

        damageOverlay.color = new Color(damageOverlayColor.r, damageOverlayColor.g, damageOverlayColor.b, alpha);
    }


}