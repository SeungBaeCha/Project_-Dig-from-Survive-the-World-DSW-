using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("생성 설정")]
    [SerializeField] private GameObject enemyPrefab; // 생성할 적 프리팹
    [SerializeField] private Transform[] spawnPoints; // 적 생성 위치
    [SerializeField] private int dayEnemyCount = 3; // 낮에 생성할 적의 수
    
    [Header("밤 웨이브 설정")]
    [SerializeField] private int initialNightEnemyCount; // 첫날 밤에 생성할 적의 수
    [SerializeField] private int minEnemiesToAddPerDay; // 매일 최소로 추가될 적의 수
    [SerializeField] private int maxEnemiesToAddPerDay; // 매일 최대로 추가될 적의 수
    
    private int currentNightEnemyCount; // 현재 밤에 생성할 적의 수
    private List<GameObject> activeEnemies = new List<GameObject>();

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

    void OnEnable()
    {
        // GameManager의 이벤트에 구독
        GameManager.OnDayStart += HandleDayStart;
        GameManager.OnNightStart += HandleNightStart;
    }

    void OnDisable()
    {
        // GameManager의 이벤트 구독 해제
        GameManager.OnDayStart -= HandleDayStart;
        GameManager.OnNightStart -= HandleNightStart;
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

        for (int i = 0; i < count; i++)
        {
            // 랜덤한 스폰 포인트를 선택
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            // 적 생성 및 리스트에 추가
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            activeEnemies.Add(enemy);
        }
    }

    private void ClearAllEnemies()
    {
        // 생성된 모든 적을 파괴하고 리스트를 비움
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
    }
}