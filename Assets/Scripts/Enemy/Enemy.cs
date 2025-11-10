using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("추격 설정")]
    public float detectionRadius = 15f; // 플레이어를 탐지할 반경

    [Header("눈 색깔 설정")]
    public Color idleColor = Color.white; // 평상시 색
    public Color chaseColor = Color.red;  // 추격 시 색

    private NavMeshAgent agent;
    private Transform player;
    private MeshRenderer eyesRenderer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // 'Eyes'라는 이름의 자식 오브젝트를 찾고, 그 오브젝트의 MeshRenderer를 가져온다.
        Transform eyesTransform = transform.Find("Eyes");
        if (eyesTransform != null)
        {
            eyesRenderer = eyesTransform.GetComponent<MeshRenderer>();
        }

        // 초기 색상 설정
        if (eyesRenderer != null)
        {
            eyesRenderer.material.color = idleColor;
        }
    }

    void Update()
    {
        if (player == null)
        {
            // 플레이어가 없으면 정지
            agent.isStopped = true;
            return;
        }

        // 플레이어와 이 적 사이의 거리를 계산
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 거리가 탐지 반경 안에 들어왔을 때
        if (distanceToPlayer <= detectionRadius)
        {
            // 추격을 시작하고, 눈 색깔을 변경
            agent.isStopped = false;
            agent.destination = player.position;
            SetEyeColor(chaseColor);
        }
        else // 거리가 탐지 반경 밖에 있을 때
        {
            // 추격을 멈추고, 눈 색깔을 원래대로
            agent.isStopped = true;
            SetEyeColor(idleColor);
        }
    }

    // 눈 색깔을 바꾸는 함수
    void SetEyeColor(Color color)
    {
        // 현재 색과 바꾸려는 색이 다를 때만 변경 (최적화)
        if (eyesRenderer != null && eyesRenderer.material.color != color)
        {
            eyesRenderer.material.color = color;
        }
    }
}
