using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("생성 설정")]
    [SerializeField] private GameObject enemyPrefab; // 생성할 적 프리팹
    [SerializeField] private Transform[] spawnPoints; // 적 생성 위치
    [SerializeField] private int dayEnemyCount = 3; // 낮에 생성할 적의 수
    [SerializeField] private int nightEnemyCount = 10; // 밤에 생성할 적의 수

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
    }

    void OnEnable()
    {
        GameManager.OnDayStart += HandleDayStart;
        GameManager.OnNightStart += HandleNightStart;
    }

    void OnDisable()
    {
        GameManager.OnDayStart -= HandleDayStart;
        GameManager.OnNightStart -= HandleNightStart;
    }

    private void HandleDayStart()
    {
        // 낮 시간 적 수에 맞게 조절
        AdjustEnemyCount(dayEnemyCount);
    }

    private void HandleNightStart()
    {
        // 밤 시간 적 수에 맞게 조절
        AdjustEnemyCount(nightEnemyCount);
    }

    // 목표 수에 맞게 적의 수를 조절하는 함수
    private void AdjustEnemyCount(int targetCount)
    {
        // 리스트에서 파괴된(null) 적들을 제거
        activeEnemies.RemoveAll(item => item == null);

        int currentCount = activeEnemies.Count;

        if (currentCount < targetCount)
        {
            // 목표보다 적으면 부족한 만큼 추가 생성
            SpawnEnemies(targetCount - currentCount);
        }
        else if (currentCount > targetCount)
        {
            // 목표보다 많으면 초과하는 만큼 제거
            int removeCount = currentCount - targetCount;
            for (int i = activeEnemies.Count - 1; i >= 0 && removeCount > 0; i--)
            {
                GameObject enemyToDestroy = activeEnemies[i];
                if (enemyToDestroy != null)
                {
                    Destroy(enemyToDestroy);
                }
                activeEnemies.RemoveAt(i);
                removeCount--;
            }
        }
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
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            activeEnemies.Add(enemy);
        }
    }

    // (참고) 이 함수는 이제 낮/밤 전환에 직접 사용되지 않지만, 필요시 모든 적을 제거하는 기능으로 남겨둘 수 있습니다.
    private void ClearAllEnemies()
    {
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
