using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 'SceneManager'를 사용하기 위해 추가

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
    [Header("배경음악")]
    public AudioClip bgm;
}

public class GameManager : MonoBehaviour
{
    // GameManager의 싱글턴 인스턴스. 어디서든 쉽게 접근할 수 있도록 한다.
    public static GameManager Instance { get; private set; }

    // 게임이 시작되었는지 여부를 나타내는 플래그.
    public bool isGameStarted { get; private set; }
    // 플레이어가 사망했는지 여부를 나타내는 플래그
    public bool IsPlayerDead { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;

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
    
    [Header("BGM")]
    [Tooltip("배경음악 볼륨")]
    [SerializeField, Range(0, 1)] private float bgmVolume = 0.5f;
    private AudioSource bgmSourceA;
    private AudioSource bgmSourceB;
    public AudioSource ActiveBgmSource { get; private set; }


    [Header("Enemy Manager")]
    [Tooltip("적 관리자 (EnemyManager) 스크립트 참조")]
    [SerializeField] private EnemyManager enemyManager;

    [Header("보급 상자 설정")]
    [SerializeField] private GameObject lootBoxPrefab;
    [Tooltip("보급 상자가 처음으로 등장할 수 있는 날짜")]
    [SerializeField] private int lootBoxStartDay = 3;
    [Tooltip("매일 증가할 보급 상자 등장 확률 (예: 0.1 = 10%)")]
    [SerializeField] private float lootBoxSpawnChanceIncrease = 0.1f;
    [Tooltip("보급 상자가 생성될 수 있는 모든 위치 목록")]
    [SerializeField] private List<Transform> lootBoxSpawnPoints;
    [Tooltip("보급 상자가 나타날 때 재생할 사운드")]
    [SerializeField] private AudioClip lootBoxSpawnSound;
    [Tooltip("보급 상자 나타나는 소리의 볼륨")]
    [SerializeField, Range(0f, 1f)] private float lootBoxSpawnVolume = 1.0f;
    [Tooltip("보급 상자가 사라질 때 재생할 사운드")]
    [SerializeField] private AudioClip lootBoxDespawnSound;
    [Tooltip("보급 상자 사라지는 소리의 볼륨")]
    [SerializeField, Range(0f, 1f)] private float lootBoxDespawnVolume = 1.0f;

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

            // BGM 소스 동적 생성 및 설정
            InitializeBgmSources();
        }
    }
    
    private void InitializeBgmSources()
    {
        // "BGMSourceA"라는 이름의 자식 오브젝트를 찾아, 없으면 새로 생성합니다.
        GameObject sourceAObject = new GameObject("BGMSourceA");
        sourceAObject.transform.SetParent(this.transform); // GameManager의 자식으로 설정
        bgmSourceA = sourceAObject.AddComponent<AudioSource>();
        bgmSourceA.loop = true; // BGM은 반복 재생
        bgmSourceA.volume = 0;  // 초기 볼륨은 0

        // "BGMSourceB"라는 이름의 자식 오브젝트를 찾아, 없으면 새로 생성합니다.
        GameObject sourceBObject = new GameObject("BGMSourceB");
        sourceBObject.transform.SetParent(this.transform); // GameManager의 자식으로 설정
        bgmSourceB = sourceBObject.AddComponent<AudioSource>();
        bgmSourceB.loop = true; // BGM은 반복 재생
        bgmSourceB.volume = 0;  // 초기 볼륨은 0

        // 첫 활성 BGM 소스를 A로 지정합니다.
        ActiveBgmSource = bgmSourceA;
    }


    /// <summary>
    /// 스크립트가 활성화될 때 호출. 낮/밤 전환 및 보급 상자 수명 주기 관리에 대한 이벤트를 구독한다.
    /// </summary>
    void OnEnable()
    {
        // 씬이 로드될 때마다 OnSceneLoaded 메서드가 호출되도록 이벤트를 구독합니다.
        // 이는 게임 재시작 시 상태를 깨끗하게 리셋하기 위함입니다.
        SceneManager.sceneLoaded += OnSceneLoaded;

        OnDayStart += StartDayTransition; // 낮 전환 시작
        OnNightStart += StartNightTransition; // 밤 전환 시작
        OnDayStart += ManageLootBoxLifecycle; // 매일 아침 보급 상자 수명 주기 관리
    }

    /// <summary>
    /// 스크립트가 비활성화될 때 호출. 구독했던 이벤트를 해지하여 메모리 누수를 방지한다.
    /// </summary>
    void OnDisable()
    {
        // OnEnable에서 구독했던 모든 이벤트를 해지합니다.
        SceneManager.sceneLoaded -= OnSceneLoaded;

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
        // Debug.Log("====== 게임 플레이 시작! ======");

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
            // Debug.LogWarning("EnemyManager가 할당되지 않아 적 생성 시스템을 시작할 수 없습니다.");
        }

        DayCount = 1; // 첫째 날 시작
        // Debug.Log($"Day {DayCount} has started.");
        OnDayStart?.Invoke(); // 낮 시작 이벤트 발생
    }

    /// <summary>
    /// 매 프레임마다 호출되어 게임 시간 관리 및 허기 감소를 처리한다.
    /// </summary>
    void Update()
    {
        // 게임이 시작되지 않았다면 아무것도 처리하지 않는다.
        if (!isGameStarted) return;

        timer -= Time.deltaTime;
        HandleHunger();

        if (timer <= 0)
        {
            IsNight = !IsNight;

            if (IsNight)
            {
                timer = nightDuration;
                // Debug.Log($"Night of Day {DayCount} has started.");
                OnNightStart?.Invoke();
            }
            else
            {
                DayCount++;
                timer = dayDuration;
                // Debug.Log($"Day {DayCount} has started.");
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
            // Debug.Log($"오늘의 낮 날씨: {currentDayWeather.presetName}");
        }
        if (nightWeatherPresets.Count > 0)
        {
            currentNightWeather = nightWeatherPresets[UnityEngine.Random.Range(0, nightWeatherPresets.Count)];
            // Debug.Log($"오늘의 밤 날씨: {currentNightWeather.presetName}");
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
        // 환경 설정 즉시 적용
        if (sun != null)
        {
            RenderSettings.ambientLight = preset.ambientColor;
            sun.color = preset.sunColor;
            sun.intensity = preset.sunIntensity;
            sun.transform.rotation = Quaternion.Euler(preset.sunRotation);
            RenderSettings.fogColor = preset.fogColor;
            RenderSettings.fogDensity = preset.fogDensity;
        }

        // BGM 즉시 변경
        // 현재 활성화된 소스가 아닌 다른 소스를 정지
        AudioSource otherSource = (ActiveBgmSource == bgmSourceA) ? bgmSourceB : bgmSourceA;
        otherSource.Stop();
        otherSource.volume = 0;

        // 활성화된 소스에 새 클립 설정 및 재생
        if (ActiveBgmSource.clip != preset.bgm)
        {
            ActiveBgmSource.clip = preset.bgm;
        }
        ActiveBgmSource.volume = (preset.bgm != null) ? bgmVolume : 0;
        if (preset.bgm != null && !ActiveBgmSource.isPlaying)
        {
            ActiveBgmSource.Play();
        }
        else if (preset.bgm == null)
        {
            ActiveBgmSource.Stop();
        }
    }

    private IEnumerator TransitionWeather(WeatherPreset preset)
    {
        // --- BGM 크로스페이드 설정 ---
        AudioSource oldSource = ActiveBgmSource;
        AudioSource newSource = (ActiveBgmSource == bgmSourceA) ? bgmSourceB : bgmSourceA;
        
        newSource.clip = preset.bgm;
        if (newSource.clip != null)
        {
            newSource.Play(); // 새 BGM 재생 시작 (볼륨은 0)
        }

        // --- 환경 설정 값 가져오기 ---
        float visualElapsedTime = 0f; // 시각 효과 경과 시간 (Time.deltaTime 사용)
        float bgmElapsedTime = 0f;    // BGM 크로스페이드 경과 시간 (Time.unscaledDeltaTime 사용)

        Color startAmbient = RenderSettings.ambientLight;
        Color startSunColor = sun.color;
        float startSunIntensity = sun.intensity;
        Quaternion startSunRotation = sun.transform.rotation;
        Color startFogColor = RenderSettings.fogColor;
        float startFogDensity = RenderSettings.fogDensity;

        // --- 전환 루프 ---
        while (visualElapsedTime < transitionDuration || bgmElapsedTime < transitionDuration)
        {
            // 시각 효과는 Time.deltaTime을 사용하여 게임 일시정지 시 함께 멈춤
            if (visualElapsedTime < transitionDuration)
            {
                visualElapsedTime += Time.deltaTime;
            }
            float visualProgress = Mathf.Min(visualElapsedTime / transitionDuration, 1f); // progress는 1.0f를 초과하지 않도록 보장

            // BGM은 Time.unscaledDeltaTime을 사용하여 게임 일시정지와 무관하게 전환
            if (bgmElapsedTime < transitionDuration)
            {
                bgmElapsedTime += Time.unscaledDeltaTime;
            }
            float bgmProgress = Mathf.Min(bgmElapsedTime / transitionDuration, 1f); // progress는 1.0f를 초과하지 않도록 보장

            // 환경 전환
            RenderSettings.ambientLight = Color.Lerp(startAmbient, preset.ambientColor, visualProgress);
            sun.color = Color.Lerp(startSunColor, preset.sunColor, visualProgress);
            sun.intensity = Mathf.Lerp(startSunIntensity, preset.sunIntensity, visualProgress);
            sun.transform.rotation = Quaternion.Slerp(startSunRotation, Quaternion.Euler(preset.sunRotation), visualProgress);
            RenderSettings.fogColor = Color.Lerp(startFogColor, preset.fogColor, visualProgress);
            RenderSettings.fogDensity = Mathf.Lerp(startFogDensity, preset.fogDensity, visualProgress);

            // BGM 크로스페이드
            oldSource.volume = Mathf.Lerp(bgmVolume, 0f, bgmProgress);
            if (newSource.clip != null)
            {
                newSource.volume = Mathf.Lerp(0f, bgmVolume, bgmProgress);
            }

            yield return null;
        }

        // --- 전환 완료 ---
        oldSource.Stop();
        oldSource.volume = 0;
        ActiveBgmSource = newSource; // 활성 소스 교체

        // 최종 상태를 한 번 더 설정하여 정확성을 보장
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
        if (activeLootBox != null && DayCount > lootBoxSpawnDay)
        {
            // 상자가 사라지는 소리 재생
            if (lootBoxDespawnSound != null)
            {
                AudioSource.PlayClipAtPoint(lootBoxDespawnSound, activeLootBox.transform.position, lootBoxDespawnVolume);
            }

            string expiryMessage = $"지난 {lootBoxSpawnDay}일차 보급 상자가 사라졌습니다.";
            UIManager.Instance.ShowSupplyBoxNotification(expiryMessage);
            // Debug.Log($"<color=orange>{expiryMessage}</color>");
            
            Destroy(activeLootBox);
            activeLootBox = null;
        }

        // --- 2. 새 보급 상자 생성 로직 ---
        if (activeLootBox != null) return;
        
        // 설정된 시작일 이전에는 생성하지 않음
        if (DayCount < lootBoxStartDay) return;

        // 날짜가 지날수록 확률 증가
        float spawnChance = (DayCount - (lootBoxStartDay - 1)) * lootBoxSpawnChanceIncrease;
        
        if (UnityEngine.Random.value < spawnChance)
        {
            if (lootBoxPrefab != null && lootBoxSpawnPoints != null && lootBoxSpawnPoints.Count > 0)
            {
                Transform spawnPoint = lootBoxSpawnPoints[UnityEngine.Random.Range(0, lootBoxSpawnPoints.Count)];
                
                activeLootBox = Instantiate(lootBoxPrefab, spawnPoint.position, spawnPoint.rotation);
                lootBoxSpawnDay = DayCount;

                // 상자가 나타나는 소리 재생
                if (lootBoxSpawnSound != null)
                {
                    // 플레이어가 존재하면 플레이어 위치에서, 아니면 월드 원점에서 소리 재생
                    Vector3 soundPosition = (playerTransform != null) ? playerTransform.position : Vector3.zero;
                    AudioSource.PlayClipAtPoint(lootBoxSpawnSound, soundPosition, lootBoxSpawnVolume);
                }

                string message = $"보급이 떨어졌습니다!"; // 사용자가 요청한 메시지로 변경
                UIManager.Instance.ShowSupplyBoxNotification(message); 
                // Debug.Log($"<color=yellow>{message} ({DayCount}일차)</color>");
            }
            else
            {
                if (lootBoxPrefab == null)
                {
                    // Debug.LogWarning("보급 상자 프리팹이 지정되지 않아 생성에 실패했습니다.");
                }
                if (lootBoxSpawnPoints == null || lootBoxSpawnPoints.Count == 0)
                {
                    // Debug.LogWarning("보급 상자 생성 지점이 지정되지 않아 생성에 실패했습니다.");
                }
            }
        }
    }
    
    /// <summary>
    /// 씬이 로드될 때마다 호출되어 게임의 상태를 초기화
    /// 게임 재시작 시 모든 것을 처음부터 시작할 수 있도록 보장
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 게임 상태를 초기화
        isGameStarted = false;
        IsPlayerDead = false; // 사망 상태 리셋
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false); // 게임오버 패널 숨기기
        }
        
        if(sun != null) sun.gameObject.SetActive(false);
        RenderSettings.ambientLight = new Color(0.1f, 0.1f, 0.1f);
        RenderSettings.fog = false;

        // 모든 타이머와 카운터 리셋
        DayCount = 0;
        IsNight = false;
        timer = 0;
        hungerTimer = 0;
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        // 새 씬에서 플레이어 다시 찾기
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerHealth = playerObject.GetComponent<PlayerHealth>();
            playerTransform = playerObject.transform;
        }
        else
        {
            // 재시작 시점에 플레이어를 못찾는 경우를 대비해 null 처리
            playerHealth = null;
            playerTransform = null;
        }

        // BGM 리셋
        if (bgmSourceA != null)
        {
            bgmSourceA.Stop();
            bgmSourceA.volume = 0;
        }
        if (bgmSourceB != null)
        {
            bgmSourceB.Stop();
            bgmSourceB.volume = 0;
        }
        ActiveBgmSource = bgmSourceA;

        // 이전 보급 상자 참조 제거
        activeLootBox = null;
        
        // Debug.Log("GameManager state has been reset on scene load.");
    }

    /// <summary>
    /// 플레이어의 사망을 처리합니다. PlayerHealth에서 호출
    /// </summary>
    public void HandlePlayerDeath()
    {
        if (IsPlayerDead) return;

        IsPlayerDead = true;
        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        //else
        //{
        //    Debug.LogError("GameOverPanel 참조가 GameManager에 할당되지 않았습니다! 인스펙터에서 확인해주세요.");
        //}

        // 게임오버 시 커서를 보이게 하고 잠금을 해제
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// 게임을 재시작하는 함수. GameOver UI의 재시작 버튼에 연결
    /// </summary>
    public void RestartGame()
    {
        // 게임 시간을 다시 정상으로 설정
        Time.timeScale = 1f;

        // 현재 씬을 다시 로드. OnSceneLoaded 이벤트가 게임 상태를 리셋
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 게임을 종료하는 함수. GameOver UI의 종료 버튼에 연결
    /// </summary>
    public void QuitGame()
    {
        // Debug.Log("게임을 종료합니다.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}