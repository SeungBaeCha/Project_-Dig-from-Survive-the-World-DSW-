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
    
    [Header("낮/밤 웨이브 설정")]
    [SerializeField] private int dayEnemyCount = 3; // 낮에 생성할 적의 수
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

        // 밤에 생성될 적의 수 초기화
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
        GameManager.OnDayStart += HandleDayStart;
        GameManager.OnNightStart += HandleNightStart;
        Debug.Log("Enemy Spawning Started.");
    }

    /// <summary>
    /// 적 생성 시스템을 중지하고 GameManager의 낮/밤 이벤트 구독을 해제한다.
    /// </summary>
    public void StopSpawning()
    {
        GameManager.OnDayStart -= HandleDayStart;
        GameManager.OnNightStart -= HandleNightStart;
        ClearAllEnemies(); // 중지 시 모든 적 제거
        Debug.Log("Enemy Spawning Stopped.");
    }

    private void HandleDayStart()
    {
        // 밤에 활동하던 모든 적을 제거
        ClearAllEnemies();
        // 낮에 활동할 적들을 생성
        Debug.Log($"낮이 되었습니다. {dayEnemyCount}마리의 적을 생성합니다.");
        SpawnEnemies(dayEnemyCount);
    }

    private void HandleNightStart()
    {
        // 낮에 활동하던 모든 적을 제거
        ClearAllEnemies();

        // 날짜에 따라 생성할 적의 수를 계산
        int day = GameManager.Instance.DayCount;
        if (day > 1)
        {
            // 둘째 날부터 적의 수를 랜덤하게 늘림
            int enemiesToAdd = Random.Range(minEnemiesToAddPerDay, maxEnemiesToAddPerDay + 1);
            currentNightEnemyCount += enemiesToAdd;
            Debug.Log($"오늘은 {day}일차 밤입니다. 어젯밤보다 {enemiesToAdd}마리 더 많은 적이 몰려옵니다.");
        }
        else
        {
            // 첫날 밤
            currentNightEnemyCount = initialNightEnemyCount;
            Debug.Log("첫날 밤입니다. 생존을 준비하십시오.");
        }
        
        // 계산된 수만큼 밤에 활동할 적들을 생성
        Debug.Log($"총 {currentNightEnemyCount}마리의 적을 생성합니다.");
        SpawnEnemies(currentNightEnemyCount);
    }

    private void SpawnEnemies(int count)
    {
        if (enemyPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogError("적 프리팹 또는 스폰 포인트가 설정되지 않았습니다.");
            return;
        }

        if (enemyTypes == null || enemyTypes.Count == 0)
        {
            Debug.LogError("적 능력치(EnemyTypes)가 설정되지 않았습니다.");
            return;
        }

        // 1. 현재 날짜에 스폰 가능한 적들만 필터링
        int currentDay = GameManager.Instance.DayCount;
        var availableEnemies = new List<EnemyStats>();
        foreach (var type in enemyTypes)
        {
            if (currentDay >= type.startDay)
            {
                availableEnemies.Add(type);
            }
        }

        // 스폰 가능한 적이 없는 경우
        if (availableEnemies.Count == 0)
        {
            Debug.LogWarning($"현재 {currentDay}일차에는 스폰 가능한 적 유형이 없습니다.");
            return;
        }

        // 2. 가중치 총합 계산
        float totalWeight = 0;
        foreach (var type in availableEnemies)
        {
            totalWeight += type.spawnChanceWeight;
        }

        for (int i = 0; i < count; i++)
        {
            // 3. 가중치 기반으로 랜덤 적 유형 선택
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

            // 만약 선택되지 않았다면(부동 소수점 오류 등 예외 처리), 마지막 적을 선택
            if (selectedStats == null && availableEnemies.Count > 0)
            {
                selectedStats = availableEnemies[availableEnemies.Count - 1];
            }

            // 랜덤한 스폰 포인트를 선택
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            // 적 생성
            GameObject enemyObject = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            
            // 생성된 적에게 선택된 능력치 부여
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
        // 생성된 모든 적을 파괴하고 리스트를 비움
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject); // Enemy 컴포넌트가 아닌 GameObject를 파괴
            }
        }
        activeEnemies.Clear();
    }
}