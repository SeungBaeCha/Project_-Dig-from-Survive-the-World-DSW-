using UnityEngine;

/// <summary>
/// 드랍된 아이템의 물리 동작과 플레이어와의 상호작용을 관리하는 스크립트.
/// LootTable에 의해 생성되는 모든 아이템 프리팹에 이 스크립트를 부착해야 한다.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class ItemController : MonoBehaviour
{
    [Header("아이템 데이터")]
    [Tooltip("이 아이템의 정보를 담고 있는 ScriptableObject")]
    public ItemData itemData;

    [Header("아이템 소멸 설정")]
    [Tooltip("아이템이 사라지기까지의 시간 (초). 0이면 사라지지 않음.")]
    [SerializeField] private float lifeTime = 5.0f;

    [Header("아이템 생성 효과")]
    [Tooltip("아이템이 생성될 때 튀어 오르는 힘. 0이면 효과 없음.")]
    [SerializeField] private float spawnForce = 2.5f;

    void Start()
    {
        // --- 1. 일정 시간 뒤 아이템 자동 파괴 ---
        // lifeTime이 0보다 크면, 해당 시간(초) 후에 아이템 게임 오브젝트를 파괴.
        if (lifeTime > 0)
        {
            Destroy(gameObject, lifeTime);
        }

        // --- 2. 물리 충돌 문제 해결 ---
        // 아이템이 땅을 뚫고 떨어지지 않도록, 물리 충돌 전용 콜라이더를 추가한다.

        // 기존의 BoxCollider는 플레이어 감지를 위한 'Trigger'로 사용.
        BoxCollider triggerCollider = GetComponent<BoxCollider>();
        triggerCollider.isTrigger = true;

        // 물리 충돌을 담당할 SphereCollider를 추가.
        // 이미 물리 콜라이더가 있는지 확인하여 중복 추가를 방지.
        bool hasPhysicsCollider = false;
        Collider[] allColliders = GetComponents<Collider>();
        foreach(var col in allColliders)
        {
            if (!col.isTrigger)
            {
                hasPhysicsCollider = true;
                break;
            }
        }

        // 물리 콜라이더가 없다면 새로 추가.
        if (!hasPhysicsCollider)
        {
            SphereCollider physicsCollider = gameObject.AddComponent<SphereCollider>();
            physicsCollider.isTrigger = false;
        }

        // --- 3. 자연스러운 드랍 효과 ---
        // Rigidbody를 찾아 흩뿌려지는 힘을 적용.
        if (spawnForce > 0)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            
            // 위로 솟구치면서 흩어지는 방향 벡터 생성.
            Vector3 forceDirection = (Random.insideUnitSphere.normalized * 0.5f + Vector3.up).normalized;
            rb.AddForce(forceDirection * spawnForce, ForceMode.Impulse);

            // 자연스러운 회전을 위해 랜덤한 각속도 추가.
            rb.AddTorque(Random.insideUnitSphere * spawnForce, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// 이 아이템의 트리거 콜라이더에 다른 오브젝트가 들어왔을 때 호출됨.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 들어온 오브젝트가 'Player' 태그를 가지고 있는지 확인.
        if (other.CompareTag("Player"))
        {
            // 이 아이템에 데이터가 할당되어 있는지 확인
            if (itemData != null)
            {
                // 플레이어에게 Inventory 컴포넌트가 있는지 확인
                Inventory inventory = other.GetComponent<Inventory>();
                if (inventory != null)
                {
                    // 인벤토리에 아이템 추가
                    inventory.AddItem(itemData);
                }
                else
                {
                    Debug.LogWarning("Player에 Inventory 컴포넌트가 없다!");
                }

                // 아이템 오브젝트를 파괴.
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning(gameObject.name + "에 ItemData가 할당되지 않았다!");
                
                // 데이터가 없어도 일단 사라지게는 하자.
                Destroy(gameObject);
            }
        }
    }
}