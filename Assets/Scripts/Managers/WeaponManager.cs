using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 무기 교체를 관리하는 스크립트
/// </summary>
public class WeaponManager : MonoBehaviour
{
    // 인스펙터 창에서 관리할 무기 목록
    // 여기에 플레이어가 사용할 무기들을 순서대로 드래그 앤 드롭
    [SerializeField]
    private List<GameObject> weapons;

    // 현재 선택된 무기의 인덱스
    private int currentWeaponIndex = 0;

    void Start()
    {
        // 게임이 시작될 때 첫 번째 무기를 선택한 상태로 초기화
        SelectWeapon(0);
    }

    void Update()
    {
        // 숫자 1 키를 눌렀을 때
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // 0번 인덱스의 무기 선택
            SelectWeapon(0);
        }
        // 숫자 2 키를 눌렀을 때
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // 1번 인덱스의 무기 선택
            SelectWeapon(1);
        }
    }

    /// <summary>
    /// 특정 인덱스의 무기를 선택하고, 나머지는 비활성화하는 함수
    /// </summary>
    /// <param name="index">선택할 무기의 인덱스</param>
    void SelectWeapon(int index)
    {
        // 선택하려는 무기가 목록 범위 안에 있는지, 그리고 현재 들고 있는 무기와 다른지 확인
        if (index < 0 || index >= weapons.Count || index == currentWeaponIndex)
        {
            return; // 이미 들고 있거나 잘못된 인덱스면 아무것도 하지 않음
        }

        // 이전에 들고 있던 무기는 비활성화
        weapons[currentWeaponIndex].SetActive(false);

        // 새로 선택한 무기는 활성화
        weapons[index].SetActive(true);

        // 현재 무기 인덱스를 새로운 인덱스로 업데이트
        currentWeaponIndex = index;
    }
}
