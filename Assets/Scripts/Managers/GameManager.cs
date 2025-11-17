using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }

    [Header("시간 설정")]
    [SerializeField] private float dayDuration = 60f; // 낮 지속 시간 (초)
    [SerializeField] private float nightDuration = 60f; // 밤 지속 시간 (초)

    [Header("조명 및 하늘 설정")]
    [SerializeField] private Light sun; // 씬의 주 조명 (Directional Light)
    [SerializeField] private float transitionDuration = 5f; // 조명 전환 시간

    [Header("낮 조명")]
    [SerializeField] private Color dayAmbientColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color daySunColor = Color.white;
    [SerializeField] private float daySunIntensity = 1f;
    [SerializeField] private Vector3 daySunRotation = new Vector3(50, -30, 0);

    [Header("밤 조명")]
    [SerializeField] private Color nightAmbientColor = new Color(0.1f, 0.1f, 0.2f);
    [SerializeField] private Color nightSunColor = new Color(0.8f, 0.8f, 1f);
    [SerializeField] private float nightSunIntensity = 0.2f;        // 밤의 밝기 조정
    [SerializeField] private Vector3 nightSunRotation = new Vector3(-90, -30, 0);

    [Header("배고픔 설정")]
    [SerializeField] private float hungerDecreaseInterval; // 배고픔 감소 체크 주기 (초)
    [SerializeField, Range(0, 1)] private float hungerDecreaseChance; // 배고픔 감소 확률
    [SerializeField] private float hungerDecreaseAmount; // 배고픔 감소량
    private float hungerTimer;

    // 참조
    private PlayerHealth playerHealth;

    // 현재 시간대와 타이머
    public bool IsNight { get; private set; }
    private float timer;
    private Coroutine lightingCoroutine;
    private float logTimer = 1f; // 1초마다 로그를 출력하기 위한 타이머

    // 시간대 변경 시 호출될 이벤트
    public static event Action OnDayStart;
    public static event Action OnNightStart;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void OnEnable()
    {
        OnDayStart += StartDayTransition;
        OnNightStart += StartNightTransition;
    }

    void OnDisable()
    {
        OnDayStart -= StartDayTransition;
        OnNightStart -= StartNightTransition;
    }

    void Start()
    {
        // 플레이어 참조 찾기
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerHealth = playerObject.GetComponent<PlayerHealth>();
        }

        // 낮부터 시작
        IsNight = false;
        timer = dayDuration;
        hungerTimer = hungerDecreaseInterval; // 배고픔 타이머 초기화
        SetLightingImmediate(false); // 시작 시 낮 조명 즉시 설정
        Debug.Log("Day has started.");
        OnDayStart?.Invoke();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        HandleHunger(); // 배고픔 처리 로직 호출

        // 1초마다 타이머 값을 정수로 출력
        logTimer -= Time.deltaTime;
        if (logTimer <= 0f)
        {
            Debug.Log("Current Timer (int): " + Mathf.FloorToInt(timer));
            logTimer = 1f; // logTimer 리셋
        }

        if (timer <= 0)
        {
            IsNight = !IsNight;

            if (IsNight)
            {
                timer = nightDuration;
                Debug.Log("Night has started.");
                OnNightStart?.Invoke();
            }
            else
            {
                timer = dayDuration;
                Debug.Log("Day has started.");
                OnDayStart?.Invoke();
            }
        }
    }

    private void HandleHunger()
    {
        if (playerHealth == null) return;

        hungerTimer -= Time.deltaTime;
        if (hungerTimer <= 0f)
        {
            // 확률 체크
            if (UnityEngine.Random.value < hungerDecreaseChance)
            {
                playerHealth.DecreaseHunger(hungerDecreaseAmount);
                Debug.Log($"Hunger decreased by {hungerDecreaseAmount}. Current Hunger: {playerHealth.currentHunger}");
            }
            hungerTimer = hungerDecreaseInterval; // 타이머 리셋
        }
    }

    private void StartDayTransition()
    {
        if (lightingCoroutine != null) StopCoroutine(lightingCoroutine);
        lightingCoroutine = StartCoroutine(TransitionLighting(false));
    }

    private void StartNightTransition()
    {
        if (lightingCoroutine != null) StopCoroutine(lightingCoroutine);
        lightingCoroutine = StartCoroutine(TransitionLighting(true));
    }

    private void SetLightingImmediate(bool isNight)
    {
        if (sun == null) return;

        if (isNight)
        {
            RenderSettings.ambientLight = nightAmbientColor;
            sun.color = nightSunColor;
            sun.intensity = nightSunIntensity;
            sun.transform.rotation = Quaternion.Euler(nightSunRotation);
        }
        else
        {
            RenderSettings.ambientLight = dayAmbientColor;
            sun.color = daySunColor;
            sun.intensity = daySunIntensity;
            sun.transform.rotation = Quaternion.Euler(daySunRotation);
        }
    }

    private IEnumerator TransitionLighting(bool isNight)
    {
        if (sun == null) yield break;

        float elapsedTime = 0f;

        // 시작 값 저장
        Color startAmbient = RenderSettings.ambientLight;
        Color startSunColor = sun.color;
        float startSunIntensity = sun.intensity;
        Quaternion startSunRotation = sun.transform.rotation;

        // 목표 값 설정
        Color targetAmbient = isNight ? nightAmbientColor : dayAmbientColor;
        Color targetSunColor = isNight ? nightSunColor : daySunColor;
        float targetSunIntensity = isNight ? nightSunIntensity : daySunIntensity;
        Quaternion targetSunRotation = Quaternion.Euler(isNight ? nightSunRotation : daySunRotation);

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / transitionDuration;

            // 값들을 부드럽게 보간
            RenderSettings.ambientLight = Color.Lerp(startAmbient, targetAmbient, progress);
            sun.color = Color.Lerp(startSunColor, targetSunColor, progress);
            sun.intensity = Mathf.Lerp(startSunIntensity, targetSunIntensity, progress);
            sun.transform.rotation = Quaternion.Slerp(startSunRotation, targetSunRotation, progress);

            yield return null;
        }

        // 전환이 끝나면 목표 값으로 정확히 설정
        SetLightingImmediate(isNight);
    }

    public float GetRemainingTime()
    {
        return timer;
    }
}
