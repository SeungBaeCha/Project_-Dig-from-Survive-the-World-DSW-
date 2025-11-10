using UnityEngine;
using UnityEngine.AI; // NavMeshAgent를 사용하기 위해 꼭 필요해!

public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;

    void Start()
    {
        // NavMeshAgent 컴포넌트를 가져온다.
        agent = GetComponent<NavMeshAgent>();
        
        // "Player" 태그를 가진 게임 오브젝트를 찾아 Transform 컴포넌트를 가져온다.
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // agent의 목적지를 플레이어의 현재 위치로 계속 설정한다.
        if (player != null)
        {
            agent.destination = player.position;
        }
    }
}
