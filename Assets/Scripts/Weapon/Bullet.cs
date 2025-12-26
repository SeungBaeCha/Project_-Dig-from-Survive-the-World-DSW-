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
    }
    
    public void FireBullet()
    {
        // 총알을 앞으로 발사
        rb.velocity = transform.forward * speed;
    }

    void Start()
    {

    }

    /// <summary>
    /// 다른 트리거 Collider에 진입했을 때 호출되는 함수
    /// </summary>
    /// <param name="other">진입한 Collider 정보</param>
    void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트에서 Enemy 컴포넌트를 찾아본다.
        Enemy enemy = other.gameObject.GetComponent<Enemy>();

        // Enemy 컴포넌트가 있다면, 데미지를 입히고 총알을 파괴한다.
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject); // 적과 부딪혔을 때만 파괴
        }
        // 적이 아닌 다른 오브젝트와 충돌했을 때는 아무것도 하지 않는다.
        // 총알은 lifeTime에 의해 알아서 사라질 것이다.
    }
}