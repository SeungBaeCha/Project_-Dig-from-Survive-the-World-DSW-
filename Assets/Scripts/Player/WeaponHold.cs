using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHold : MonoBehaviour
{
    // 현재 들고 있는 무기 오브젝트를 저장. 비어있으면 무기가 없는 상태.
    public GameObject equippedWeapon;
    // 무기를 장착할 손의 위치. Unity 에디터에서 빈 GameObject를 플레이어의 손 위치에 만들고 연결해줘야 해.
    public Transform holdPoint;
    
    // 플레이어 근처에 있는 무기 오브젝트 (트리거 안에 들어왔을 때 저장)
    private GameObject nearbyWeapon;
    // 무기 장착 상태를 저장하는 변수
    private bool isEquipped = false;

    // PlayerInput 컴포넌트의 'Invoke Unity Events'에서 호출될 함수
    public void OnEquip(InputAction.CallbackContext context)
    {
        
        // 키가 '눌렸을 때'만 반응하도록 설정
        if (!context.performed)
        {
            return;
        }

        // 1. 아직 무기를 들고 있지 않고 (isEquipped == false)
        // 2. 주변에 주울 무기가 있다면 (nearbyWeapon != null)

        if (!isEquipped && nearbyWeapon != null)
        {
            // 주울 무기를 equippedWeapon 변수에 저장
            equippedWeapon = nearbyWeapon;
            
            // 주운 무기의 부모를 holdPoint로 설정해서 손에 따라다니게 만듦
            equippedWeapon.transform.SetParent(holdPoint);

            // 주운 무기의 위치를 손 위치와 정확히 일치시킴
            equippedWeapon.transform.localPosition = Vector3.zero;

            // 주운 무기의 회전 값을 초기화해서 플레이어와 같은 방향을 보게 함
            equippedWeapon.transform.localRotation = Quaternion.identity;

            // 주운 무기의 콜라이더를 비활성화해서 물리적 충돌을 막음
            Collider weaponCollider = equippedWeapon.GetComponent<Collider>();
            if(weaponCollider != null)
            {
                weaponCollider.enabled = false;
            }
            
            // 무기를 장착했으므로 상태를 true로 변경
            isEquipped = true;
            // 주변에 있던 무기는 이제 내가 들었으므로 null로 초기화
            nearbyWeapon = null;
            
            Debug.Log(equippedWeapon.name + " 무기를 장착했다!");
        }
        // 이미 다른 무기를 들고 있는 경우
        else if(isEquipped)
        {
            Debug.Log("이미 무기를 들고 있어 다른 무기를 장착할 수 없다.");
            // 여기에 나중에 무기를 버리거나 교체하는 로직을 추가
        }
    }

    // isTrigger가 켜진 다른 콜라이더 안에 들어갔을 때 호출되는 함수
    private void OnTriggerEnter(Collider other)
    {
        // 1. 들어온 오브젝트의 태그가 "Weapon"이고
        // 2. 아직 내가 무기를 들고 있지 않다면
        if (other.CompareTag("Weapon") && !isEquipped)
        {
            // 그 오브젝트를 '주울 수 있는 무기'로 저장
            nearbyWeapon = other.gameObject;
            Debug.Log("주울 수 있는 무기 발견: " + nearbyWeapon.name);
        }
    }

    // 콜라이더 안에서 빠져나왔을 때 호출되는 함수
    private void OnTriggerExit(Collider other)
    {
        // 빠져나온 오브젝트가 내가 '주울 수 있는 무기'로 저장해뒀던 바로 그 오브젝트라면
        if (other.gameObject == nearbyWeapon)
        {
            // '주울 수 있는 무기'를 null로 초기화해서 더 이상 주울 수 없게 함
            nearbyWeapon = null;
            Debug.Log("무기 범위에서 벗어났다.");
        }
    }
}