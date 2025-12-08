using System.Collections.Generic;
using UnityEngine;

// 무기들을 관리하고 교체하는 역할을 담당하는 클래스
public class WeaponManager : MonoBehaviour
{
    // 인스펙터에서 편하게 보기 위한 헤더
    [Header("Weapons")]
    // 플레이어가 사용 가능한 모든 무기들의 리스트. 시작 시 자동으로 채워진다.
    public List<Gun> weapons;

    // 현재 선택된 무기의 인덱스
    private int currentWeaponIndex = -1;

    void Awake()
    {
        // 자신의 자식 오브젝트에 있는 모든 Gun 컴포넌트들을 찾는다.
        // true를 인자로 넘겨주면, 비활성화된 오브젝트에 있는 컴포넌트까지 모두 찾아온다.
        GetComponentsInChildren<Gun>(true, weapons);
        
        // 찾은 무기들을 순회하면서 WeaponManager를 참조하게 설정 (선택적 확장 기능)
        // foreach (var weapon in weapons) { /* 필요 시 weapon.Init(this); 같은 코드 추가 */ }
    }

    void Start()
    {
        // 게임이 시작되면 첫 번째 무기(인덱스 0)를 장착한다.
        EquipWeapon(0);
    }

    void Update()
    {
        // 무기 교체 입력을 감지
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EquipWeapon(0); // 1번 키를 누르면 0번 인덱스 무기 장착
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            EquipWeapon(1); // 2번 키를 누르면 1번 인덱스 무기 장착
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            EquipWeapon(2); // 3번 키를 누르면 2번 인덱스 무기 장착
        }
        // ... 무기 개수만큼 추가 가능
    }

    // 특정 인덱스의 무기를 장착하는 메서드
    public void EquipWeapon(int weaponIndex)
    {
        // 요청된 인덱스가 유효한 범위 내에 있고, 현재 장착된 무기와 다른 경우에만 실행
        if (weaponIndex < 0 || weaponIndex >= weapons.Count || weaponIndex == currentWeaponIndex)
        {
            return;
        }

        // 이전에 장착된 무기가 있었다면 비활성화
        if (currentWeaponIndex != -1)
        {
            weapons[currentWeaponIndex].gameObject.SetActive(false);
        }

        // 새로운 무기 인덱스로 업데이트
        currentWeaponIndex = weaponIndex;
        // 새로운 무기 게임 오브젝트를 활성화
        weapons[currentWeaponIndex].gameObject.SetActive(true);
        
        Debug.Log(weapons[currentWeaponIndex].gameObject.name + " equipped.");
    }

    // 현재 장착된 무기(Gun)의 정보를 반환하는 메서드
    public Gun GetCurrentWeapon()
    {
        // 현재 무기 인덱스가 유효하다면 해당 무기를, 아니면 null을 반환
        if (currentWeaponIndex != -1)
        {
            return weapons[currentWeaponIndex];
        }
        return null;
    }
}
