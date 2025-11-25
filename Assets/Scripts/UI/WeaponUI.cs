//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI; 


//// WeaponUI 클래스는 무기 교체 UI의 시각적 표현을 담당합니다.
//public class WeaponUI : MonoBehaviour
//{
//    [Header("참조 설정")]
//    // 어떤 무기 슬롯이 채워져 있는지 확인하기
//    public WeaponHold weaponHold;

//    [Header("UI 설정")]
//    // 무기 슬롯 배경으로 사용될 Image 컴포넌트들의 배열.
//    public Image[] weaponSlots;

//    [Header("슬롯 색상 설정")]
//    // 무기가 선택되었을 때 슬롯의 색상.
//    public Color selectedColor = Color.white;
//    // 무기가 있지만 선택되지 않았을 때의 색상.
//    public Color unselectedColor = Color.gray;
//    // 해당 슬롯에 무기가 없을 때의 색상.
//    public Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f); // 어둡고 반투명한 색

//    void Start()
//    {
//        // weaponHold 참조가 설정되었는지 확인
//        if (weaponHold == null)
//        {
//            Debug.LogError("WeaponUI에 WeaponHold 참조가 연결되지 않았습니다!");
//            return;
//        }
//        // 게임 시작 시, 선택된 무기가 없는 상태(-1)로 UI를 초기화
//        UpdateSelectedWeaponUI(-1);
//    }

//    // 현재 선택된 무기 인덱스를 받아서 UI를 업데이트하는 함수.
//    public void UpdateSelectedWeaponUI(int selectedIndex)
//    {
//        // 모든 UI 슬롯을 순회
//        for (int i = 0; i < weaponSlots.Length; i++)
//        {
//            // i가 실제 무기 슬롯의 범위 안에 있고, 해당 슬롯에 무기가 존재하는지 확인
//            if (i < weaponHold.weapons.Length && weaponHold.weapons[i] != null)
//            {
//                // 슬롯에 무기가 있는 경우
//                if (i == selectedIndex)
//                {
//                    // 현재 선택된 무기 슬롯이면 'selectedColor'를 적용
//                    weaponSlots[i].color = selectedColor;
//                }
//                else
//                {
//                    // 무기는 있지만 선택되지 않은 슬롯이면 'unselectedColor'를 적용
//                    weaponSlots[i].color = unselectedColor;
//                }
//            }
//            else
//            {
//                // 실제 무기 슬롯이 없거나, 슬롯이 비어있으면 'emptyColor'를 적용
//                weaponSlots[i].color = emptyColor;
//            }
//        }
//    }
//}

