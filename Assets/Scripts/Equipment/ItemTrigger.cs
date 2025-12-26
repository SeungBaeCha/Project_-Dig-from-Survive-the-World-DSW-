using UnityEngine;

/// <summary>
/// 땅에 떨어진 아이템이 플레이어에게 '주울 수 있다'고 알려주는 역할을 하는 스크립트.
/// 이 스크립트가 붙은 아이템은 WeaponHold에 의해 장착될 수 있다.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ItemTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어와 부딪혔는지 태그로 확인
        if (other.CompareTag("Player"))
        {
            // 플레이어의 WeaponHold 스크립트를 찾아 '주울 수 있는 무기'로 자신을 등록
            WeaponHold weaponHold = other.GetComponent<WeaponHold>();
            if (weaponHold != null)
            {
                weaponHold.SetNearbyWeapon(this.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 플레이어와 충돌이 끝났는지 태그로 확인
        if (other.CompareTag("Player"))
        {
            // 플레이어의 WeaponHold 스크립트를 찾아 '주울 수 있는 무기' 목록에서 자신을 제거
            WeaponHold weaponHold = other.GetComponent<WeaponHold>();
            if (weaponHold != null)
            {
                weaponHold.ClearNearbyWeapon(this.gameObject);
            }
        }
    }
}