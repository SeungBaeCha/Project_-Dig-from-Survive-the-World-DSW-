using UnityEngine;
using UnityEngine.AI;
using System.Linq; // LINQ 사용을 위해 추가

public class Enemy : MonoBehaviour
{
    [Header("체력 설정")]
    public float maxHealth = 100f;
    private float currentHealth;
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

    [Header("추격 설정")]
    public float detectionRadius = 15f; // 플레이어를 탐지할 반경
    public float chaseSpeed = 6f;       // 추격 시 이동 속도
    
    [Header("공격 설정")]
    public float attackDamage = 10f;    // 플레이어에게 입힐 데미지 양
    public float attackCooldown = 2f;   // 공격 사이의 최소 시간 간격 (초)
    public float attackDistance = 2f;   // 플레이어를 공격할 수 있는 최대 거리
    private float lastAttackTime;       // 마지막으로 공격한 시간을 저장하는 변수

    [Header("순찰 설정")]
    public float patrolRadius = 10f;    // 순찰 반경
    public float patrolSpeed = 3f;      // 순찰 시 이동 속도
    public float patrolWaitTime = 3f;   // 순찰 지점 도착 후 대기 시간
    private float waitTimer;

    // 목표 지점 갱신 타이머
    private float destinationUpdateTimer;
    private float destinationUpdateInterval = 0.5f; // 0.5초마다 목표 지점 갱신

    [Header("눈 색깔 설정")]
    public Color idleColor; // 평상시/순찰 시 색
    public Color chaseColor;  // 추격 시 색
    private MeshRenderer eyesRenderer;

    [Header("드랍 아이템 설정")]
    public LootTable enemyLootTable;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        startingPosition = transform.position;
        
        // DiggableGrid 인스턴스를 찾아서 참조를 저장
        diggableGrid = FindObjectOfType<DiggableGrid>();

        currentHealth = maxHealth;
        if (hpBar != null) hpBar.UpdateHP(currentHealth, maxHealth);

        // 첫 공격이 즉시 가능하도록 lastAttackTime을 초기화합니다.
        // 이렇게 하면 게임 시작 후 플레이어가 처음 공격 범위에 들어왔을 때 바로 공격할 수 있습니다.
        lastAttackTime = -attackCooldown;

        Transform eyesTransform = transform.Find("Eyes");
        if (eyesTransform != null) eyesRenderer = eyesTransform.GetComponent<MeshRenderer>();
        
        SwitchState(State.Patrolling);
    }

    void Update()
    {
        if (isDead) return;

        if (player == null)
        {
            // 플레이어를 계속 찾아본다.
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if(player == null) return;
        }

        // 밤이면 무조건 플레이어 추격 상태로 전환
        if (GameManager.Instance != null && GameManager.Instance.IsNight)
        {
            if (currentState != State.Chasing)
            {
                SwitchState(State.Chasing);
            }
        }

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
                // 주기적으로 목표 지점을 플레이어 위치 또는 가장 가까운 입구로 갱신
                destinationUpdateTimer += Time.deltaTime;
                if (destinationUpdateTimer >= destinationUpdateInterval)
                {
                    destinationUpdateTimer = 0f;
                    SetChaseDestination();
                }

                // 공격 로직
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
                SetChaseDestination(); // 추격 상태로 전환 시 즉시 목표 설정
                break;
        }
    }

    /// <summary>
    /// 추격 상태일 때 목표 지점을 설정하는 새로운 지능형 메서드
    /// </summary>
    private void SetChaseDestination()
    {
        if (!agent.isOnNavMesh || player == null) return;

        // 1. 플레이어에게 직접 가는 경로가 있는지 확인
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(player.position, path);

        // 2. 경로가 완전하다면(막히지 않았다면) 플레이어를 목표로 설정
        if (path.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetDestination(player.position);
            //Debug.Log($"<color=green>[Enemy AI - {gameObject.name}]</color> 플레이어에게 가는 길이 있습니다! 직접 추적합니다. 목표: {player.position}");
        }
        // 3. 경로가 불완전하다면(막혔다면) 가장 가까운 '입구'를 찾아 목표로 설정
        else
        {
            Debug.Log($"<color=orange>[Enemy AI - {gameObject.name}]</color> 플레이어에게 가는 길이 막혔습니다! 우회할 입구를 찾습니다.");
            if (diggableGrid != null && diggableGrid.GetEntrances().Any())
            {
                // 가장 가까운 입구 찾기
                Vector3 closestEntrance = diggableGrid.GetEntrances()
                    .OrderBy(entrance => Vector3.Distance(transform.position, entrance))
                    .First();
                
                agent.SetDestination(closestEntrance);
                //Debug.Log($"<color=cyan>[Enemy AI - {gameObject.name}]</color> 가장 가까운 입구({closestEntrance})로 이동합니다.");
            }
            // 입구가 없다면, 그냥 현재 위치에 멈춤 (또는 다른 행동 추가 가능)
            else
            {
                //Debug.LogWarning($"<color=red>[Enemy AI - {gameObject.name}]</color> 길이 막혔지만, 우회할 입구를 찾지 못했습니다!");
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
        if (hpBar != null) hpBar.UpdateHP(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
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