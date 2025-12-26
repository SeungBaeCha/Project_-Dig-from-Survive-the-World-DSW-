using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("체력 설정")]
    public float maxHealth = 100f;
    private float currentHealth;
    public HPBar hpBar; // HP바 UI 참조


    // Enemy의 사망상태 파악
    bool isDead = false;


    // AI 상태를 구분하기 위한 열거형
    private enum State
    {
        Patrolling, // 순찰
        Chasing,    // 추격
        Returning   // 복귀
    }

    private State currentState;

    [Header("기본 설정")]
    private NavMeshAgent agent;
    private Transform player;
    private Vector3 startingPosition; // 처음 위치를 저장할 변수

    [Header("추격 설정")]
    public float detectionRadius = 15f; // 플레이어를 탐지할 반경
    public float chaseSpeed = 6f;       // 추격 시 이동 속도
    [Tooltip("추격 속도의 무작위성 범위. 최종 속도는 chaseSpeed ± speedRandomness/2 이다.")]
    public float speedRandomness = 1f;  // 추격 속도에 추가할 무작위성
    [Tooltip("플레이어 주변에서 목표 지점을 찾을 반경. 값이 클수록 적들이 넓게 퍼진다.")]
    public float chaseDestinationRadius = 3f; // 추격 목표 지점 반경

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
    
    // 추격 목표 지점 관리를 위한 변수들
    private Vector3 currentChaseDestination;
    private float destinationUpdateTimer;
    private float destinationUpdateInterval = 0.1f; // 0.1초마다 목표 지점 갱신 (반응성 향상)

    [Header("눈 색깔 설정")]
    public Color idleColor; // 평상시/순찰 시 색
    public Color chaseColor;  // 추격 시 색
    private MeshRenderer eyesRenderer;

    [Header("드랍 아이템 설정")]
    [Tooltip("적이 사망했을 때 아이템을 드랍할 LootTable")]
    public LootTable enemyLootTable;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        startingPosition = transform.position; // 시작 위치 저장

        // 체력 초기화 및 HP바 업데이트
        currentHealth = maxHealth;
        if (hpBar != null)
        {
            hpBar.UpdateHP(currentHealth, maxHealth);
        }

        // 'Eyes' 자식 오브젝트의 렌더러 찾기
        Transform eyesTransform = transform.Find("Eyes");
        if (eyesTransform != null)
        {
            eyesRenderer = eyesTransform.GetComponent<MeshRenderer>();
        }

        // 순찰 상태로 시작
        SwitchState(State.Patrolling);
    }

    void Update()
    {
        // 죽으면 Enemy의 모든 행동이 실행되지 않는다
        if (isDead)
        {
           return; 
        }
//        // --- 테스트용 데미지 코드 ---
//#if UNITY_EDITOR
//        if (Input.GetKeyDown(KeyCode.Space))
//        {
//            TakeDamage(20);
//        }
//#endif
//        // -------------------------


        // 플레이어 Transform이 할당되지 않았다면 다시 탐색
        if (player == null)
        {
            // "Player" 태그를 가진 게임 오브젝트를 찾아 Transform을 할당
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                // 아직 플레이어를 찾을 수 없으면 Update 로직을 더 이상 진행하지 않음
                return;
            }
        }

        // 밤이면 무조건 플레이어 추격
        if (GameManager.Instance.IsNight)
        {
            if (currentState != State.Chasing)
            {
                SwitchState(State.Chasing);
            }
        }
        
        // --- 아래는 낮 시간 로직 (밤에도 Chasing 상태 로직은 공유) ---

        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 상태에 따라 행동 결정
        switch (currentState)
        {
            case State.Patrolling:
                // 플레이어가 탐지 범위 안에 들어오면 추격 상태로 전환
                if (distanceToPlayer <= detectionRadius)
                {
                    SwitchState(State.Chasing);
                    break;
                }

                // 목적지에 도착했으면 잠시 대기 후 새로운 목적지 설정
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    waitTimer += Time.deltaTime;
                    if (waitTimer >= patrolWaitTime)
                    {
                        SetNewRandomDestination();
                    }
                }
                break;

            case State.Chasing:
                // 낮에만 플레이어가 탐지 범위를 벗어나면 복귀하도록 수정
                if (!GameManager.Instance.IsNight && distanceToPlayer > detectionRadius)
                {
                    SwitchState(State.Returning);
                    break;
                }

                
                // 주기적으로 목표 지점을 플레이어 주변으로 갱신한다.
                destinationUpdateTimer += Time.deltaTime;
                if (destinationUpdateTimer >= destinationUpdateInterval)
                {
                    destinationUpdateTimer = 0f;
                    
                    // 목표 지점 설정 개선 (더 똑똑한 추적 로직)
                    SetChaseDestination();
                }

                // 에이전트의 목적지를 설정한다.
                // agent.destination은 비용이 높은 편이므로, 꼭 필요할 때만 설정하는 것이 좋다.
                if (agent.destination != currentChaseDestination)
                {
                    agent.SetDestination(currentChaseDestination);
                }


                // 플레이어가 공격 가능 거리 안에 있고 공격 쿨다운이 지났는지 확인하여
                if (distanceToPlayer <= attackDistance && Time.time >= lastAttackTime + attackCooldown)
                {
                    // 플레이어 공격
                    Attack();
                }
                break;

            case State.Returning:
                // 복귀 중 플레이어가 다시 탐지 범위에 들어오면 추격
                if (distanceToPlayer <= detectionRadius)
                {
                    SwitchState(State.Chasing);
                    break;
                }

                // 시작 위치에 거의 도착했으면 순찰 상태로 전환
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    SwitchState(State.Patrolling);
                }
                break;
        }
    }

    // 데미지를 받는 함수
    public void TakeDamage(float damage)
    {
        // 죽었으면 데미지 받지 않음
        if (isDead) return;

        currentHealth -= damage;
        if (hpBar != null)
        {
            hpBar.UpdateHP(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 사망 처리 함수
    private void Die()
    {
        isDead = true;

        // NavMeshAgent의 움직임을 즉시 중지
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // 아이템 드랍
        if (enemyLootTable != null)
        {
            enemyLootTable.SpawnLoot(transform.position);
        }

        // 간단하게 오브젝트를 N 초 뒤 파괴. 나중에 여기에 파티클이나 사운드 효과를 추가할 수 있다.
        Destroy(gameObject, 1f);
    }

    // 공격 처리 함수
    private void Attack()
    {
        // 공격 애니메이션이나 효과를 위한 잠시 멈춤
        agent.isStopped = true;

        // 마지막 공격 시간을 현재 시간으로 기록
        lastAttackTime = Time.time;
        Debug.Log("적이 플레이어를 공격했다!");

        // 플레이어의 PlayerHealth 컴포넌트를 가져와 데미지를 준다.
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }

        // 0.5초 후에 ResumeMovement 함수를 호출하여 다시 움직이게 한다.
        Invoke(nameof(ResumeMovement), 0.5f);
    }

    // NavMeshAgent의 이동을 다시 시작하는 함수
    void ResumeMovement()
    {
        // 적이 죽지 않은 상태일 때만 이동을 재개한다.
        if (!isDead)
        {
            agent.isStopped = false;
        }
    }

    // 상태를 전환하는 함수
    private void SwitchState(State newState)
    {
        //// 상태가 같으면 중복 실행을 방지한다.
        if (currentState == newState || isDead) return;

        currentState = newState;
        switch (currentState)
        {
            case State.Patrolling:
                agent.speed = patrolSpeed;
                SetEyeColor(idleColor);
                SetNewRandomDestination();
                break;
            case State.Chasing:
                // 추격 속도에 약간의 무작위성을 부여한다.
                agent.speed = chaseSpeed + Random.Range(-speedRandomness / 2, speedRandomness / 2);
                SetEyeColor(chaseColor);
                break;
            case State.Returning:
                agent.speed = patrolSpeed; // 복귀는 순찰 속도로
                SetEyeColor(idleColor);
                agent.destination = startingPosition;
                break;
        }
    }

    // 새로운 랜덤 목적지를 설정하는 함수
    void SetNewRandomDestination()
    {
        waitTimer = 0f;
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += startingPosition;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    // 더 지능적인 추격 목적지를 설정하는 함수
    void SetChaseDestination()
    {
        NavMeshPath path = new NavMeshPath();
        
        // 플레이어에게 직접 도달 가능한 경로가 있는지 먼저 확인한다.
        if (agent.CalculatePath(player.position, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            // 직접 경로가 있다면, 플레이어를 직접 목표로 삼아 경로의 마지막 지점으로 설정한다.
            // 이렇게 하면 플레이어가 움직이더라도 가장 효율적인 경로를 따라간다.
            currentChaseDestination = path.corners[path.corners.Length - 1];
        }
        else
        {
            // 직접 경로가 없다면 (벽, 장애물 등), 플레이어 주변의 유효한 위치를 찾는다.
            // 이는 AI가 막혔을 때 플레이어 주변을 탐색하게 하여 더 똑똑하게 보이게 한다.
            Vector3 randomDirection = Random.insideUnitSphere * chaseDestinationRadius;
            randomDirection += player.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, chaseDestinationRadius, NavMesh.AllAreas))
            {
                currentChaseDestination = hit.position;
            }
            else
            {
                // 주변 위치도 못찾으면 최후의 수단으로 플레이어 위치를 사용한다.
                // 이 경우 에이전트가 벽에 막힐 수도 있지만, 다음 업데이트에서 새 경로를 찾을 기회가 있다.
                currentChaseDestination = player.position;
            }
        }
    }

    // 눈 색깔을 바꾸는 함수
    void SetEyeColor(Color color)
    {
        if (eyesRenderer != null && eyesRenderer.material.color != color)
        {
            eyesRenderer.material.color = color;
        }
    }

    // 유니티 에디터에서 오브젝트 선택 시 디버그 시각화
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; // 기즈모 색상을 노란색으로 설정
        Gizmos.DrawWireSphere(transform.position, detectionRadius); // detectionRadius 크기의 구체를 그림
    }
}