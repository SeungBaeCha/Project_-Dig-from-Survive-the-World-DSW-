using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임의 전반적인 상태(시간, 날씨, 낮/밤 주기)를 관리하고,
/// 플레이어의 허기 및 보급 상자 스폰과 같은 주요 게임 메커니즘을 제어하는 싱글턴 매니저.
/// </summary>
[System.Serializable]
public struct WeatherPreset
{
    public string presetName;
    [Header("조명")]
    public Color ambientColor;
    public Color sunColor;
    public float sunIntensity;
    public Vector3 sunRotation;
    [Header("안개")]
    public Color fogColor;
    public float fogDensity;
}

public class GameManager : MonoBehaviour
{
    // GameManager의 싱글턴 인스턴스. 어디서든 쉽게 접근할 수 있도록 한다.
    public static GameManager Instance { get; private set; }

    // 게임이 시작되었는지 여부를 나타내는 플래그.
    public bool isGameStarted { get; private set; }

    [Header("시간 설정")]
    [Tooltip("낮 시간의 지속 시간 (초)")]
    [SerializeField] private float dayDuration = 60f;
    [Tooltip("밤 시간의 지속 시간 (초)")]
    [SerializeField] private float nightDuration = 60f;
    [Tooltip("낮과 밤 전환 시 효과가 지속되는 시간 (초)")]
    [SerializeField] private float transitionDuration = 5f;

    [Header("날씨 프리셋")]
    [SerializeField] private List<WeatherPreset> dayWeatherPresets;
    [SerializeField] private List<WeatherPreset> nightWeatherPresets;

    private WeatherPreset currentDayWeather;
    private WeatherPreset currentNightWeather;

    [Header("Enemy Manager")]
    [Tooltip("적 관리자 (EnemyManager) 스크립트 참조")]
    [SerializeField] private EnemyManager enemyManager;

    [Header("보급 상자 설정")]
    [SerializeField] private GameObject lootBoxPrefab;
    [Tooltip("3일차부터 매일 증가할 보급 상자 등장 확률 (예: 0.1 = 10%)")]
    [SerializeField] private float lootBoxSpawnChanceIncrease = 0.1f;
    [Tooltip("보급 상자가 생성될 수 있는 모든 위치 목록")]
    [SerializeField] private List<Transform> lootBoxSpawnPoints;

    [Header("배고픔 설정")]
    [SerializeField] private float hungerDecreaseInterval;
    [SerializeField, Range(0, 1)] private float hungerDecreaseChance;
    [SerializeField] private float hungerDecreaseAmount;
    private float hungerTimer;

    // 플레이어의 체력 컴포넌트 참조
    private PlayerHealth playerHealth;
    // 플레이어의 Transform 컴포넌트 참조 (보급 상자 스폰 위치 계산에 사용)
    private Transform playerTransform;
    // 씬의 태양광 오브젝트
    [SerializeField] private Light sun;

    // 현재 씬에 활성화된 보급 상자 오브젝트. 한 번에 하나만 추적한다.
    private GameObject activeLootBox;
    // 현재 activeLootBox가 생성된 게임 내 날짜. 사라지는 로직에 사용된다.
    private int lootBoxSpawnDay;

    // 현재 게임이 밤인지 여부.
    public bool IsNight { get; private set; }
    // 현재 게임의 일차.
    public int DayCount { get; private set; } = 0;
    // 현재 낮 또는 밤이 지속되는 시간을 카운트하는 타이머.
    private float timer;
    // 날씨 전환 코루틴을 제어하기 위한 참조.
    private Coroutine transitionCoroutine;

    public static event Action OnDayStart;
    public static event Action OnNightStart;

    /// <summary>
    /// GameManager 인스턴스가 생성될 때 호출. 싱글턴 패턴을 구현하고 InputActions를 초기화한다.
    /// </summary>
    void Awake()
    {
        // 싱글턴 인스턴스 확인 및 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // 씬이 변경되어도 파괴되지 않도록 설정 (루트 오브젝트를 대상으로 함)
            DontDestroyOnLoad(transform.root.gameObject);
        }
    }

    /// <summary>
    /// 스크립트가 활성화될 때 호출. 낮/밤 전환 및 보급 상자 수명 주기 관리에 대한 이벤트를 구독한다.
    /// </summary>
    void OnEnable()
    {
        OnDayStart += StartDayTransition; // 낮 전환 시작
        OnNightStart += StartNightTransition; // 밤 전환 시작
        OnDayStart += ManageLootBoxLifecycle; // 매일 아침 보급 상자 수명 주기 관리
    }

    /// <summary>
    /// 스크립트가 비활성화될 때 호출. 구독했던 이벤트를 해지하여 메모리 누수를 방지한다.
    /// </summary>
    void OnDisable()
    {
        OnDayStart -= StartDayTransition;
        OnNightStart -= StartNightTransition;
        OnDayStart -= ManageLootBoxLifecycle;
    }

    /// <summary>
    /// 게임 시작 시 초기 설정을 수행한다.
    /// </summary>
    void Start()
    {
        // 게임 시작 전 초기 상태: 게임 시작 플래그 비활성화, 태양 비활성화, 앰비언트 라이트 설정
        isGameStarted = false;
        if(sun != null) sun.gameObject.SetActive(false);
        RenderSettings.ambientLight = new Color(0.1f, 0.1f, 0.1f);
        
        // "Player" 태그를 가진 게임 오브젝트를 찾아 PlayerHealth 및 Transform 컴포넌트를 가져온다.
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerHealth = playerObject.GetComponent<PlayerHealth>();
            playerTransform = playerObject.transform;
        }
    }

    /// <summary>
    /// GameStartTrigger에 의해 호출되어 튜토리얼 시퀀스를 시작한다.
    /// 게임 시작 전에 튜토리얼 UI를 표시하도록 UIManager에 요청한다.
    /// </summary>
    public void StartTutorialSequence()
    {
        UIManager.Instance.StartTutorial();
    }
    
    /// <summary>
    /// 튜토리얼 UI의 '닫기' 버튼 등에 의해 호출되어 실제 게임 플레이를 시작한다.
    /// 게임 상태를 초기화하고, HUD를 표시하며, 첫 낮 시간을 시작한다.
    /// </summary>
    public void BeginGameplay()
    {
        // 이미 게임이 시작된 상태라면 추가로 시작하지 않는다.
        if (isGameStarted) return;

        isGameStarted = true;
        Debug.Log("====== 게임 플레이 시작! ======");

        // UIManager에 게임 HUD 표시를 요청한다.
        UIManager.Instance.ShowGameHUD();
        
        // 태양 오브젝트를 활성화하고, 첫 낮 시간 및 허기 타이머를 설정한다.
        if (sun != null) sun.gameObject.SetActive(true);
        IsNight = false; // 시작은 낮
        timer = dayDuration; // 낮 시간 타이머 초기화
        hungerTimer = hungerDecreaseInterval; // 허기 감소 타이머 초기화
        RenderSettings.fog = true; // 안개 활성화

        // 새로운 날씨 프리셋을 선택하고 즉시 적용한다.
        SelectWeatherForNewDay();
        SetWeatherImmediate(currentDayWeather);

        // 튜토리얼 종료 후 적 생성 시스템을 시작한다.
        // OnDayStart 이벤트가 호출되기 전에 먼저 구독하도록 한다.
        if (enemyManager != null)
        {
            enemyManager.StartSpawning();
        }
        else
        {
            Debug.LogWarning("EnemyManager가 할당되지 않아 적 생성 시스템을 시작할 수 없습니다.");
        }

        DayCount = 1; // 첫째 날 시작
        Debug.Log($"Day {DayCount} has started.");
        OnDayStart?.Invoke(); // 낮 시작 이벤트 발생
    }

    /// <summary>
    /// 매 프레임마다 호출되어 게임 시간 관리 및 허기 감소를 처리한다.
    /// </summary>
    void Update()
    {
        // 게임이 시작되지 않았다면 아무것도 처리하지 않는다.

        timer -= Time.deltaTime;
        HandleHunger();

        if (timer <= 0)
        {
            IsNight = !IsNight;

            if (IsNight)
            {
                timer = nightDuration;
                Debug.Log($"Night of Day {DayCount} has started.");
                OnNightStart?.Invoke();
            }
            else
            {
                DayCount++;
                timer = dayDuration;
                Debug.Log($"Day {DayCount} has started.");
                SelectWeatherForNewDay();
                OnDayStart?.Invoke();
            }
        }
    }

    private void SelectWeatherForNewDay()
    {
        if (dayWeatherPresets.Count > 0)
        {
            currentDayWeather = dayWeatherPresets[UnityEngine.Random.Range(0, dayWeatherPresets.Count)];
            Debug.Log($"오늘의 낮 날씨: {currentDayWeather.presetName}");
        }
        if (nightWeatherPresets.Count > 0)
        {
            currentNightWeather = nightWeatherPresets[UnityEngine.Random.Range(0, nightWeatherPresets.Count)];
            Debug.Log($"오늘의 밤 날씨: {currentNightWeather.presetName}");
        }
    }

    private void HandleHunger()
    {
        if (playerHealth == null || !isGameStarted) return;

        hungerTimer -= Time.deltaTime;
        if (hungerTimer <= 0f)
        {
            if (UnityEngine.Random.value < hungerDecreaseChance)
            {
                playerHealth.DecreaseHunger(hungerDecreaseAmount);
            }
            hungerTimer = hungerDecreaseInterval;
        }
    }

    private void StartDayTransition()
    {
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(TransitionWeather(currentDayWeather));
    }

    private void StartNightTransition()
    {
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(TransitionWeather(currentNightWeather));
    }

    private void SetWeatherImmediate(WeatherPreset preset)
    {
        if (sun == null) return;
        
        RenderSettings.ambientLight = preset.ambientColor;
        sun.color = preset.sunColor;
        sun.intensity = preset.sunIntensity;
        sun.transform.rotation = Quaternion.Euler(preset.sunRotation);
        RenderSettings.fogColor = preset.fogColor;
        RenderSettings.fogDensity = preset.fogDensity;
    }

    private IEnumerator TransitionWeather(WeatherPreset preset)
    {
        if (sun == null) yield break;

        float elapsedTime = 0f;

        Color startAmbient = RenderSettings.ambientLight;
        Color startSunColor = sun.color;
        float startSunIntensity = sun.intensity;
        Quaternion startSunRotation = sun.transform.rotation;
        Color startFogColor = RenderSettings.fogColor;
        float startFogDensity = RenderSettings.fogDensity;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / transitionDuration;

            RenderSettings.ambientLight = Color.Lerp(startAmbient, preset.ambientColor, progress);
            sun.color = Color.Lerp(startSunColor, preset.sunColor, progress);
            sun.intensity = Mathf.Lerp(startSunIntensity, preset.sunIntensity, progress);
            sun.transform.rotation = Quaternion.Slerp(startSunRotation, Quaternion.Euler(preset.sunRotation), progress);
            RenderSettings.fogColor = Color.Lerp(startFogColor, preset.fogColor, progress);
            RenderSettings.fogDensity = Mathf.Lerp(startFogDensity, preset.fogDensity, progress);

            yield return null;
        }

        SetWeatherImmediate(preset);
    }

    /// <summary>
    /// 현재 낮/밤이 끝나기까지 남은 시간을 반환한다.
    /// </summary>
    /// <returns>남은 시간 (초)</returns>
    public float GetRemainingTime()
    {
        return timer;
    }

    /// <summary>
    /// 매일 아침 호출되어 보급 상자의 수명 주기를 관리한다.
    /// 이전 상자를 제거하고, 확률에 따라 새 상자를 생성한다.
    /// </summary>
    private void ManageLootBoxLifecycle()
    {
        // --- 1. 이전 보급 상자 제거 로직 ---
        // activeLootBox가 비어있지 않고(즉, 이전에 생성된 상자가 있고),
        // 현재 날짜가 상자가 생성된 날짜보다 크면(즉, 하루가 지났으면) 해당 상자를 제거한다.
        if (activeLootBox != null && DayCount > lootBoxSpawnDay)
        {
            Debug.Log($"<color=orange>지난 ({lootBoxSpawnDay}일차) 보급 상자를 제거합니다.</color>");
            Destroy(activeLootBox);
            activeLootBox = null;
        }

        // --- 2. 새 보급 상자 생성 로직 ---
        // 이미 맵에 보급 상자가 있다면 새로 생성하지 않는다 (한 번에 하나의 상자만 활성화).
        if (activeLootBox != null) return;
        
        // 3일차부터 보급 상자 등장 확률이 적용된다.
        if (DayCount < 3) return;

        // 날짜가 지날수록 확률 증가 (예: 3일차 10%, 4일차 20%...)
        float spawnChance = (DayCount - 2) * lootBoxSpawnChanceIncrease;
        
        // 계산된 확률에 따라 보급 상자 생성 시도
        if (UnityEngine.Random.value < spawnChance)
        {
            // 보급 상자 프리팹과 스폰 지점 목록이 유효한지 확인
            if (lootBoxPrefab != null && lootBoxSpawnPoints != null && lootBoxSpawnPoints.Count > 0)
            {
                // 지정된 생성 위치 목록에서 랜덤한 지점 하나를 선택한다.
                Transform spawnPoint = lootBoxSpawnPoints[UnityEngine.Random.Range(0, lootBoxSpawnPoints.Count)];
                
                // 보급 상자를 생성하고 activeLootBox 변수에 저장하여 추적한다.
                activeLootBox = Instantiate(lootBoxPrefab, spawnPoint.position, spawnPoint.rotation);
                // 보급 상자가 생성된 날짜를 기록한다.
                lootBoxSpawnDay = DayCount;

                // UIManager를 통해 보급 상자 생성 알림을 표시한다.
                string message = $"희귀 보급 상자가 '{spawnPoint.name}' 위치에 나타났습니다!";
                UIManager.Instance.ShowSupplyBoxNotification(message); 
                Debug.Log($"<color=yellow>{message} ({DayCount}일차)</color>");
            }
            else
            {
                // 프리팹 또는 스폰 지점 설정 누락 경고
                if (lootBoxPrefab == null)
                {
                    Debug.LogWarning("보급 상자 프리팹이 지정되지 않아 생성에 실패했습니다.");
                }
                if (lootBoxSpawnPoints == null || lootBoxSpawnPoints.Count == 0)
                {
                    Debug.LogWarning("보급 상자 생성 지점이 지정되지 않아 생성에 실패했습니다.");
                }
            }
        }
    }
}