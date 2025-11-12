using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
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

    [Header("순찰 설정")]
    public float patrolRadius = 10f;    // 순찰 반경
    public float patrolSpeed = 3f;      // 순찰 시 이동 속도
    public float patrolWaitTime = 3f;   // 순찰 지점 도착 후 대기 시간
    private float waitTimer;

    [Header("눈 색깔 설정")]
    public Color idleColor; // 평상시/순찰 시 색
    public Color chaseColor;  // 추격 시 색
    private MeshRenderer eyesRenderer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        startingPosition = transform.position; // 시작 위치 저장

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
            agent.destination = player.position;
            return; // 밤에는 아래의 낮 시간 로직을 실행하지 않음
        }

        // --- 아래는 낮 시간 로직 ---

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
                // 플레이어가 탐지 범위를 벗어나면 복귀 상태로 전환
                if (distanceToPlayer > detectionRadius)
                {
                    SwitchState(State.Returning);
                    break;
                }
                agent.destination = player.position;
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

    // 상태를 전환하는 함수
    private void SwitchState(State newState)
    {
        // 상태가 같으면 중복 실행 방지
        if (currentState == newState) return;

        currentState = newState;
        switch (currentState)
        {
            case State.Patrolling:
                agent.speed = patrolSpeed;
                SetEyeColor(idleColor);
                SetNewRandomDestination();
                break;
            case State.Chasing:
                agent.speed = chaseSpeed;
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

    // 눈 색깔을 바꾸는 함수
    void SetEyeColor(Color color)
    {
        if (eyesRenderer != null && eyesRenderer.material.color != color)
        {
            eyesRenderer.material.color = color;
        }
        // 유니티 에디터에서 오브젝트 선택 시 디버그 시각화
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow; // 기즈모 색상을 노란색으로 설정
            Gizmos.DrawWireSphere(transform.position, detectionRadius); // detectionRadius 크기의 구체를 그림
        }
    }
}
