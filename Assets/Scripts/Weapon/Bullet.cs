using UnityEngine;

/// <summary>
/// 총알의 행동을 제어하는 스크립트.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("총알 설정")]
    [SerializeField] private float speed = 50f; // 총알의 속도
    [SerializeField] private float lifeTime = 3f; // 총알이 파괴되지 않고 살아갈 수 있는 최대 시간

    private Rigidbody rb; 
    private float damage; // 총알의 입힐 데미지. GunController에서 설정
    private Vector3 lastPosition; // 이전 프레임의 위치 (레이캐스팅을 위함)
    private bool hasHit = false; // 총알이 이미 무언가에 맞았는지 여부
    
    /// <summary>
    /// 이 총알이 입힐 데미지를 설정하는 함수.
    /// </summary>
    /// <param name="newDamage">설정할 데미지 값</param>
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
    
    /// <summary>
    /// 총알이 플레이어와 충돌하지 않도록 설정
    /// </summary>
    /// <param name="playerCollider">무시할 플레이어의 Collider</param>
    public void IgnorePlayerCollision(Collider playerCollider)
    {
        Collider bulletCollider = GetComponent<Collider>();
        if (bulletCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(bulletCollider, playerCollider);
        }
    }

    void Awake() // Start에서 Awake로 변경하여 빠른 초기화에 용이하게 함
    {
        rb = GetComponent<Rigidbody>();
        
        // Rigidbody가 있는지 확인
        if(rb == null)
        {
            Debug.LogError("Bullet 스크립트에 Rigidbody 컴포넌트가 없다.");
            Destroy(gameObject);
            return;
        }
        
        // lifeTime 이후에 총알이 스스로 파괴되도록 예약
        Destroy(gameObject, lifeTime);

        lastPosition = transform.position; // 현재 위치를 이전 위치로 초기화
    }
    
    public void FireBullet()
    {
        // 총알을 앞으로 발사
        rb.velocity = transform.forward * speed;
    }

    void FixedUpdate() // 물리 업데이트에서 레이캐스팅 체크
    {
        if (hasHit) return; // 이미 맞았으면 추가 처리하지 않음

        Vector3 currentPosition = transform.position;
        Vector3 direction = (currentPosition - lastPosition).normalized;
        float distance = Vector3.Distance(lastPosition, currentPosition);

        RaycastHit hit;
        // 이전 위치부터 현재 위치까지 레이캐스트를 쏴서 충돌 감지
        // LayerMask.GetMask("Enemy")는 "Enemy" 레이어에 있는 오브젝트만 감지하도록 한다.
        // Enemy 프리팹에 "Enemy" 레이어를 반드시 설정해야 함.
        if (Physics.Raycast(lastPosition, direction, out hit, distance, LayerMask.GetMask("Enemy")))
        {
            // Debug.Log($"[Bullet] Raycast hit {hit.collider.gameObject.name}");
            Enemy enemy = hit.collider.gameObject.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                TakeHit(enemy);
                return; // 적과 충돌했으니 더 이상 진행할 필요 없음
            }
        }

        lastPosition = currentPosition; // 현재 위치를 다음 프레임의 이전 위치로 저장
    }

    /// <summary>
    /// 다른 트리거 Collider에 진입했을 때 호출되는 함수
    /// (레이캐스팅이 놓칠 수 있는 경우를 대비한 보험)
    /// </summary>
    /// <param name="other">진입한 Collider 정보</param>
    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return; // 이미 맞았으면 추가 처리하지 않음

        // 디버깅: 무엇과 충돌했는지 항상 로그를 남긴다.
        //Debug.Log($"[Bullet] OnTriggerEnter with {other.gameObject.name} (Tag: {other.tag}, Layer: {LayerMask.LayerToName(other.gameObject.layer)})");

        Enemy enemy = other.gameObject.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            TakeHit(enemy);
        }
        // 적이 아닌 다른 오브젝트와 충돌했을 때는 아무것도 하지 않는다.
        // 총알은 lifeTime에 의해 알아서 사라질 것이다.
    }

    /// <summary>
    /// 적에게 데미지를 입히고 총알을 파괴하는 헬퍼 메서드
    /// </summary>
    /// <param name="enemy">데미지를 입을 적</param>
    private void TakeHit(Enemy enemy)
    {
        if (hasHit) return; // 이미 처리된 히트라면 중복 실행 방지
        hasHit = true; // 히트 처리 시작

        // Debug.Log($"[Bullet] Enemy '{enemy.name}' detected via collision/raycast! Calling TakeDamage with {damage} damage.");
        enemy.TakeDamage(damage);
        Destroy(gameObject); // 적과 부딪혔을 때만 파괴
    }
}