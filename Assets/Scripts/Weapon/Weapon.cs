using UnityEngine;

/// <summary>
/// 무기 개체에 부착되어 줍기/버리기 상태를 관리하는 스크립트
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Weapon : MonoBehaviour
{
    // 무기의 물리적 상태를 제어하기 위한 컴포넌트
    private Rigidbody rb;
    private Collider col;

    void Awake()
    {
        // 컴포넌트들을 미리 찾아놓는다
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // 무기가 어색하게 서 있는 현상을 방지하고 자연스럽게 넘어지게 하기 위해 무게 중심을 위로 살짝 올린다.
        rb.centerOfMass = new Vector3(0, -0.1f, 0);
    }

    

    private void OnCollisionEnter(Collision collision)
    {
        // 충돌 로그를 통해 어떤 오브젝트와 부딪혔는지 확인
        Debug.Log(gameObject.name + "이(가) " + collision.gameObject.name + " (태그: " + collision.gameObject.tag + ") 와 충돌했습니다!", collision.gameObject);

        // 물리 엔진이 자연스럽게 안정화시키도록 isKinematic 설정 로직을 제거함.
    }

    /// <summary>
    /// 이 무기의 (넓은) 트리거 콜라이더에 다른 오브젝트가 들어왔을 때 호출된다.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 들어온 오브젝트가 'Player' 태그를 가지고 있는지 확인
        if (other.CompareTag("Player"))
        {
            // 플레이어에게서 WeaponHold 스크립트를 찾는다.
            WeaponHold weaponHold = other.GetComponent<WeaponHold>();

            // 스크립트를 찾았다면, 이 무기를 '주울 수 있는 무기'로 설정하라고 알려준다.
            if (weaponHold != null)
            {
                weaponHold.SetNearbyWeapon(this.gameObject);
            }
        }
    }

    /// <summary>
    /// 다른 오브젝트가 이 무기의 트리거 콜라이더에서 빠져나갔을 때 호출된다.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        // 나간 오브젝트가 'Player' 태그를 가지고 있는지 확인
        if (other.CompareTag("Player"))
        {
            // 플레이어에게서 WeaponHold 스크립트를 찾는다.
            WeaponHold weaponHold = other.GetComponent<WeaponHold>();

            // 스크립트를 찾았다면, 이 무기가 더 이상 근처에 없다고 알려준다.
            if (weaponHold != null)
            {
                weaponHold.ClearNearbyWeapon(this.gameObject);
            }
        }
    }



}
