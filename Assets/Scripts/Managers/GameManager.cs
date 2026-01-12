using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public static GameManager Instance { get; private set; }

    public bool isGameStarted { get; private set; }

    [Header("시간 설정")]
    [SerializeField] private float dayDuration = 60f;
    [SerializeField] private float nightDuration = 60f;
    [SerializeField] private float transitionDuration = 5f;

    [Header("날씨 프리셋")]
    [SerializeField] private List<WeatherPreset> dayWeatherPresets;
    [SerializeField] private List<WeatherPreset> nightWeatherPresets;

    private WeatherPreset currentDayWeather;
    private WeatherPreset currentNightWeather;

    [Header("배고픔 설정")]
    [SerializeField] private float hungerDecreaseInterval;
    [SerializeField, Range(0, 1)] private float hungerDecreaseChance;
    [SerializeField] private float hungerDecreaseAmount;
    private float hungerTimer;

    private PlayerHealth playerHealth;
    [SerializeField] private Light sun;

    public bool IsNight { get; private set; }
    public int DayCount { get; private set; } = 0;
    private float timer;
    private Coroutine transitionCoroutine;

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
            DontDestroyOnLoad(transform.root.gameObject);
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
        // 게임 시작 전 초기 상태
        isGameStarted = false;
        if(sun != null) sun.gameObject.SetActive(false);
        RenderSettings.ambientLight = new Color(0.1f, 0.1f, 0.1f);
        
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerHealth = playerObject.GetComponent<PlayerHealth>();
        }
    }

    /// <summary>
    /// GameStartTrigger에 의해 호출되어 튜토리얼 시퀀스를 시작한다.
    /// </summary>
    public void StartTutorialSequence()
    {
        // UIManager에게 튜토리얼을 보여달라고 요청한다.
        UIManager.Instance.StartTutorial();
    }
    
    /// <summary>
    /// 튜토리얼 UI의 '닫기' 버튼에 의해 호출되어 실제 게임 플레이를 시작한다.
    /// </summary>
    public void BeginGameplay()
    {
        if (isGameStarted) return;

        isGameStarted = true;
        Debug.Log("====== 게임 플레이 시작! ======");

        // UIManager에게 게임 HUD를 보여달라고 요청한다.
        UIManager.Instance.ShowGameHUD();
        
        // 태양 활성화 및 첫 날 시작 로직
        if (sun != null) sun.gameObject.SetActive(true);
        IsNight = false;
        timer = dayDuration;
        hungerTimer = hungerDecreaseInterval;
        RenderSettings.fog = true;

        SelectWeatherForNewDay();
        SetWeatherImmediate(currentDayWeather);

        DayCount = 1;
        Debug.Log($"Day {DayCount} has started.");
        OnDayStart?.Invoke();
    }

    void Update()
    {
        if (!isGameStarted) return;

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

    public float GetRemainingTime()
    {
        return timer;
    }
}