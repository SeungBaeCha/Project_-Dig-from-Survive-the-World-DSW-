using UnityEngine;
using UnityEngine.AI;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
public class Enemy : MonoBehaviour
{
    // --- 능력치 변수 (Initialize에서 설정됨) ---
    private float maxHealth;
    private float currentHealth;
    private float chaseSpeed;
    private float patrolSpeed;
    private float attackDamage;
    private float attackDistance; // stats에서 받아온 실제 공격 사거리
    
    private EnemyStats assignedStats; // 이 적에게 할당된 EnemyStats 참조 추가
    
    [Header("UI")]
    public HPBar hpBar; // HP바 UI 참조
    private bool isDead = false;

    // AI 상태를 구분하기 위한 열거형
    private enum State
    {
        Patrolling, // 순찰
        Chasing,    // 추격
    }
    private State currentState;

    [Header("기본 설정")]
    private NavMeshAgent agent;
    private Transform player;
    private Vector3 startingPosition; // 처음 위치를 저장할 변수
    private DiggableGrid diggableGrid; // DiggableGrid 참조

    [Header("탐지 및 공격 설정")]
    public float detectionRadius = 15f; // 플레이어를 탐지할 반경
    public float attackCooldown = 2f;   // 공격 사이의 최소 시간 간격 (초)
    private float lastAttackTime;       // 마지막으로 공격한 시간을 저장하는 변수

    [Header("순찰 설정")]
    public float patrolRadius = 10f;    // 순찰 반경
    public float patrolWaitTime = 3f;   // 순찰 지점 도착 후 대기 시간
    private float waitTimer;

    // 목표 지점 갱신 타이머
    private float destinationUpdateTimer;
    private float destinationUpdateInterval = 0.5f; // 0.5초마다 목표 지점 갱신

    [Header("눈 색깔 설정")]
    public Color idleColor; // 평상시/순찰 시 색
    public Color chaseColor;  // 추격 시 색
    private MeshRenderer eyesRenderer;
    private MeshRenderer bodyRenderer; // 몸의 렌더러 참조

    [Header("드랍 아이템 설정")]
    public LootTable enemyLootTable;

    [Header("사운드 설정")]
    public AudioClip hitSound; // 피격 시 재생할 사운드
    [Range(0f, 1f)]
    public float hitSoundVolume = 0.5f; // 피격 사운드 볼륨
    private AudioSource audioSource; // 사운드 재생을 위한 AudioSource

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>(); // AudioSource 컴포넌트 가져오기
        
        // 눈과 몸 렌더러 찾기
        eyesRenderer = transform.Find("Eyes")?.GetComponent<MeshRenderer>();
        bodyRenderer = transform.Find("Body")?.GetComponent<MeshRenderer>();
        if(bodyRenderer == null)
        {
            // "Body"가 없을 경우, 눈이 아닌 다른 메시 렌더러를 찾는다.
            bodyRenderer = GetComponentsInChildren<MeshRenderer>().FirstOrDefault(r => r.gameObject.name != "Eyes");
        }
    }

    /// <summary>
    /// EnemyManager가 적을 생성한 직후 호출하여 능력치를 설정하는 메서드.
    /// </summary>
    /// <param name="stats">적용할 능력치가 담긴 ScriptableObject</param>
    public void Initialize(EnemyStats stats)
    {
        assignedStats = stats; // EnemyStats 참조 저장

        // ScriptableObject로부터 능력치 적용
        maxHealth = stats.maxHealth;
        chaseSpeed = stats.chaseSpeed;
        attackDamage = stats.attackDamage;
        patrolSpeed = chaseSpeed * 0.5f; // 순찰 속도는 추격 속도의 절반으로 설정
        
        // 크기를 반영한 최종 공격 거리 계산 -> 이제 스케일과 상관없이 stats의 값을 그대로 사용
        // 기본 공격 거리에 스케일의 x값을 곱하여, 적이 커질수록 공격 사거리도 늘어나게 함
        attackDistance = stats.attackDistance; 

        currentHealth = maxHealth;

        // 외형 변경
        transform.localScale = stats.scale;
        if (bodyRenderer != null)
        {
            bodyRenderer.material.color = stats.bodyColor;
        }

        // AI 및 기타 초기화
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        startingPosition = transform.position;
        diggableGrid = FindObjectOfType<DiggableGrid>();

        // NavMeshAgent의 정지 거리를 크기가 반영된 공격 가능 거리와 동기화
        if (agent != null)
        {
            agent.stoppingDistance = attackDistance;
        }

        if (hpBar != null) hpBar.UpdateHP(currentHealth, maxHealth);

        lastAttackTime = -attackCooldown;

        SwitchState(State.Patrolling);
    }
    
    // 이 적의 EnemyStats를 외부에 노출하는 Public Getter
    public EnemyStats GetAssignedStats()
    {
        return assignedStats;
    }
    
    void Update()
    {
        if (isDead || player == null) return;
        
        // isGameStarted 체크를 제거하고 GameManager의 존재 여부만으로 밤 추적 결정
        if (GameManager.Instance != null && GameManager.Instance.IsNight)
        {
            if (currentState != State.Chasing)
            {
                SwitchState(State.Chasing);
            }
        }

//#if UNITY_EDITOR
//        // T 키를 누르면 해당 적에게 10 데미지를 입힌다. 테스트 코드
//        if (Input.GetKeyDown(KeyCode.T))
//        {
//            TakeDamage(100); 
//        }
//#endif

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrolling:
                if (distanceToPlayer <= detectionRadius)
                {
                    SwitchState(State.Chasing);
                    break;
                }

                if (agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    waitTimer += Time.deltaTime;
                    if (waitTimer >= patrolWaitTime)
                    {
                        SetNewRandomPatrolDestination();
                    }
                }
                break;

            case State.Chasing:
                destinationUpdateTimer += Time.deltaTime;
                if (destinationUpdateTimer >= destinationUpdateInterval)
                {
                    destinationUpdateTimer = 0f;
                    SetChaseDestination();
                }

                if (distanceToPlayer <= attackDistance && Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                }
                break;
        }
    }

    private void SwitchState(State newState)
    {
        if (currentState == newState || isDead) return;

        currentState = newState;

        switch (currentState)
        {
            case State.Patrolling:
                agent.speed = patrolSpeed;
                SetEyeColor(idleColor);
                SetNewRandomPatrolDestination();
                break;
            case State.Chasing:
                agent.speed = chaseSpeed;
                SetEyeColor(chaseColor);
                SetChaseDestination();
                break;
        }
    }
    
    private void SetChaseDestination()
    {
        if (!agent.isOnNavMesh || player == null) return;

        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(player.position, path);

        if (path.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            if (diggableGrid != null && diggableGrid.GetEntrances().Any())
            {
                Vector3 closestEntrance = diggableGrid.GetEntrances()
                    .OrderBy(entrance => Vector3.Distance(transform.position, entrance))
                    .First();
                agent.SetDestination(closestEntrance);
            }
        }
    }

    void SetNewRandomPatrolDestination()
    {
        waitTimer = 0f;
        if (!agent.isOnNavMesh) return;

        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += startingPosition;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        //Debug.Log($"[Enemy] '{gameObject.name}' took {damage} damage. Current health: {currentHealth}/{maxHealth}");

        if (hpBar != null) hpBar.UpdateHP(currentHealth, maxHealth);

        // 피격 사운드를 0.1초 지연 후 재생하기 위해 코루틴을 시작한다.
        if (hitSound != null && audioSource != null)
        {
            StartCoroutine(PlayHitSoundWithDelay(0.1f));
        }


        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 지정된 시간만큼 기다린 후 피격 사운드를 재생하는 코루틴.
    /// </summary>
    /// <param name="delay">사운드 재생 전 대기할 시간(초)</param>
    /// <returns></returns>
    private System.Collections.IEnumerator PlayHitSoundWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.PlayOneShot(hitSound, hitSoundVolume);
    }


    private void Die()
    {
        isDead = true;
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (enemyLootTable != null) enemyLootTable.SpawnLoot(transform.position);
        
        Destroy(gameObject, 1f);
    }

    private void Attack()
    {
        lastAttackTime = Time.time;
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }
    
    void SetEyeColor(Color color)
    {
        if (eyesRenderer != null && eyesRenderer.material.color != color)
        {
            eyesRenderer.material.color = color;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}