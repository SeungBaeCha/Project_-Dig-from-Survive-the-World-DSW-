using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("생성 설정")]
    [SerializeField] private GameObject enemyPrefab; // 생성할 적 프리팹
    [SerializeField] private Transform[] spawnPoints; // 적 생성 위치
    [SerializeField] private List<EnemyStats> enemyTypes; // 생성할 적의 능력치 종류
    
    [Header("주간 웨이브 설정")]
    [Tooltip("주간에 생성할 '일반' 적의 능력치 에셋")]
    [SerializeField] private EnemyStats normalDaytimeEnemy;
    [Tooltip("첫날 주간에 생성할 적의 수")]
    [SerializeField] private int initialDayEnemyCount = 3; 
    [Tooltip("매일 주간에 최소로 추가될 적의 수")]
    [SerializeField] private int minDayEnemiesToAddPerDay;
    [Tooltip("매일 주간에 최대로 추가될 적의 수")]
    [SerializeField] private int maxDayEnemiesToAddPerDay;
    private int currentDayEnemyCount; // 현재 주간에 생성할 적의 수

    [Header("야간 웨이브 설정")]
    [SerializeField] private int initialNightEnemyCount; // 첫날 밤에 생성할 적의 수
    [SerializeField] private int minEnemiesToAddPerDay; // 매일 최소로 추가될 적의 수
    [SerializeField] private int maxEnemiesToAddPerDay; // 매일 최대로 추가될 적의 수
    
    private int currentNightEnemyCount; // 현재 밤에 생성할 적의 수
    private List<Enemy> activeEnemies = new List<Enemy>(); // GameObject 대신 Enemy 컴포넌트를 저장

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // 주간/야간에 생성될 적의 수 초기화
        currentDayEnemyCount = initialDayEnemyCount;
        currentNightEnemyCount = initialNightEnemyCount;
    }

    
    public List<Enemy> GetActiveEnemies()
    {
        // 리스트를 반환하기 전에 파괴된(null) 적들을 제거하여 리스트를 정리
        activeEnemies.RemoveAll(item => item == null);
        return activeEnemies;
    }

    /// <summary>
    /// 적 생성 시스템을 시작하고 GameManager의 낮/밤 이벤트를 구독한다.
    /// 이 메서드는 튜토리얼 종료 후 GameManager에서 호출된다.
    /// </summary>
    public void StartSpawning()
    {
        GameManager.OnDayStart += OnDayStartEventHandler;
        GameManager.OnNightStart += OnNightStartEventHandler;
        Debug.Log("Enemy Spawning Started.");
    }

    /// <summary>
    /// 적 생성 시스템을 중지하고 GameManager의 낮/밤 이벤트 구독을 해제한다.
    /// </summary>
    public void StopSpawning()
    {
        GameManager.OnDayStart -= OnDayStartEventHandler;
        GameManager.OnNightStart -= OnNightStartEventHandler;
        ClearAllEnemies(); // 중지 시 모든 적 제거
        Debug.Log("Enemy Spawning Stopped.");
    }

    // 이벤트 핸들러: OnDayStart 이벤트가 발생하면 코루틴 시작
    private void OnDayStartEventHandler()
    {
        StartCoroutine(HandleDayStartCoroutine());
    }

    // 이벤트 핸들러: OnNightStart 이벤트가 발생하면 코루틴 시작
    private void OnNightStartEventHandler()
    {
        StartCoroutine(HandleNightStartCoroutine());
    }

    private IEnumerator HandleDayStartCoroutine()
    {
        // NavMesh 업데이트가 완료될 때까지 기다린다.
        if (DiggableGrid.Instance != null)
        {
            yield return new WaitWhile(() => DiggableGrid.Instance.IsNavMeshUpdating);
            Debug.Log("NavMesh 업데이트 완료. 낮 적 생성 시작.");
        }
        else
        {
            Debug.LogWarning("DiggableGrid.Instance가 null입니다. NavMesh 업데이트 대기 로직을 건너뜜니다.");
        }

        // 밤에 활동하던 모든 적을 제거
        ClearAllEnemies();

        int day = GameManager.Instance.DayCount;
        if (day > 1)
        {
            int enemiesToAdd = Random.Range(minDayEnemiesToAddPerDay, maxDayEnemiesToAddPerDay + 1);
            currentDayEnemyCount += enemiesToAdd;
            Debug.Log($"오늘은 {day}일차 낮입니다. {enemiesToAdd}마리 더 많은 적이 나타납니다.");
        }

        Debug.Log($"낮이 되었습니다. {currentDayEnemyCount}마리의 일반 적을 생성합니다.");
        if (normalDaytimeEnemy != null)
        {
            SpawnEnemies(currentDayEnemyCount, normalDaytimeEnemy);
        }
        else
        {
            Debug.LogError("'Normal Daytime Enemy'가 설정되지 않았습니다! 주간 적을 생성할 수 없습니다.");
        }
    }

    private IEnumerator HandleNightStartCoroutine()
    {
        // NavMesh 업데이트가 완료될 때까지 기다린다.
        if (DiggableGrid.Instance != null)
        {
            yield return new WaitWhile(() => DiggableGrid.Instance.IsNavMeshUpdating);
            Debug.Log("NavMesh 업데이트 완료. 밤 적 생성 시작.");
        }
        else
        {
            Debug.LogWarning("DiggableGrid.Instance가 null입니다. NavMesh 업데이트 대기 로직을 건너뜜니다.");
        }

        // 낮에 활동하던 모든 적을 제거
        ClearAllEnemies();

        int day = GameManager.Instance.DayCount;
        if (day > 1)
        {
            int enemiesToAdd = Random.Range(minEnemiesToAddPerDay, maxEnemiesToAddPerDay + 1);
            currentNightEnemyCount += enemiesToAdd;
            Debug.Log($"오늘은 {day}일차 밤입니다. 어젯밤보다 {enemiesToAdd}마리 더 많은 적이 몰려옵니다.");
        }
        else
        {
            currentNightEnemyCount = initialNightEnemyCount;
            Debug.Log("첫날 밤입니다. 생존을 준비하십시오.");
        }
        
        Debug.Log($"총 {currentNightEnemyCount}마리의 적을 생성합니다.");
        SpawnEnemies(currentNightEnemyCount, null); // null을 전달하여 가중치 기반 랜덤 생성 로직 사용
    }

    private void SpawnEnemies(int count, EnemyStats specificType = null)
    {
        if (enemyPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogError("적 프리팹 또는 스폰 포인트가 설정되지 않았습니다.");
            return;
        }
        
        // 특정 타입의 적만 생성하는 경우
        if (specificType != null)
        {
            for (int i = 0; i < count; i++)
            {
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                GameObject enemyObject = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                Enemy enemyComponent = enemyObject.GetComponent<Enemy>();
                if (enemyComponent != null)
                {
                    enemyComponent.Initialize(specificType);
                    activeEnemies.Add(enemyComponent);
                }
            }
            return; // 로직 종료
        }

        // --- 다양한 종류의 적을 가중치에 따라 생성하는 기존 로직 ---
        if (enemyTypes == null || enemyTypes.Count == 0)
        {
            Debug.LogError("적 능력치(EnemyTypes)가 설정되지 않았습니다.");
            return;
        }

        int currentDay = GameManager.Instance.DayCount;
        var availableEnemies = new List<EnemyStats>();
        foreach (var type in enemyTypes)
        {
            if (currentDay >= type.startDay)
            {
                availableEnemies.Add(type);
            }
        }

        if (availableEnemies.Count == 0)
        {
            Debug.LogWarning($"현재 {currentDay}일차에는 스폰 가능한 적 유형이 없습니다.");
            return;
        }

        float totalWeight = 0;
        foreach (var type in availableEnemies)
        {
            totalWeight += type.spawnChanceWeight;
        }

        for (int i = 0; i < count; i++)
        {
            EnemyStats selectedStats = null;
            float randomWeight = Random.Range(0, totalWeight);
            float currentWeight = 0;

            foreach (var type in availableEnemies)
            {
                currentWeight += type.spawnChanceWeight;
                if (randomWeight <= currentWeight)
                {
                    selectedStats = type;
                    break;
                }
            }

            if (selectedStats == null && availableEnemies.Count > 0)
            {
                selectedStats = availableEnemies[availableEnemies.Count - 1];
            }

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemyObject = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            
            Enemy enemyComponent = enemyObject.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.Initialize(selectedStats);
                activeEnemies.Add(enemyComponent);
            }
        }
    }

    private void ClearAllEnemies()
    {
        List<Enemy> survivingEnemies = new List<Enemy>();

        // 생성된 적들 중 밤을 생존할 수 있는 적들을 필터링
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null && enemy.GetAssignedStats() != null)
            {
                EnemyStats enemyStats = enemy.GetAssignedStats();
                
                // 생존 확률이 0보다 큰 경우에만 확률 계산
                if (enemyStats.nightSurvivalChance > 0f)
                {
                    // 랜덤 값이 생존 확률보다 낮으면 생존
                    if (UnityEngine.Random.value < enemyStats.nightSurvivalChance)
                    {
                        survivingEnemies.Add(enemy);
                        // Debug.Log($"{enemy.gameObject.name} (ID: {enemy.GetInstanceID()})이(가) 확률({enemyStats.nightSurvivalChance*100}%)로 밤을 생존합니다.");
                    }
                    else
                    {
                        Destroy(enemy.gameObject); // 확률 실패로 파괴
                    }
                }
                else // 생존 확률이 0이거나 음수인 경우 무조건 파괴
                {
                    Destroy(enemy.gameObject); // 밤을 생존할 수 없는 적은 파괴
                }
            }
        }
        
        // activeEnemies 리스트를 생존한 적들로 업데이트
        activeEnemies.Clear();
        activeEnemies.AddRange(survivingEnemies);
        
        // Debug.Log($"밤을 생존한 적: {activeEnemies.Count}마리");
    }
}