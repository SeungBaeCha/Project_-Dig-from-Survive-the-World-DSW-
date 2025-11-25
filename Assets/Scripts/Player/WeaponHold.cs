using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHold : MonoBehaviour
{
    [Header("뷰모델 설정")]
    // 현재 들고 있는 무기 오브젝트를 저장. 비어있으면 무기가 없는 상태.
    public GameObject equippedWeapon;
    // 무기를 장착할 손의 위치. Unity 에디터에서 빈 GameObject를 플레이어의 손 위치에 만들고 연결해줘야 해.
    public Transform holdPoint;

    [Header("무기 스웨이(Sway) 설정")]
    // 스웨이 강도. 마우스 움직임에 얼마나 민감하게 반응할지 결정.
    [SerializeField] private float swayAmount = 0.02f;
    // 스웨이 부드러움. 무기가 원래 위치로 돌아오는 속도.
    [SerializeField] private float smoothAmount = 6f;

    // 무기 레이어의 인덱스. 유니티 에디터의 Tags and Layers에서 8번째 레이어를 'Weapon'으로 설정해야 해.
    private const int WEAPON_LAYER_INDEX = 8;


    // 플레이어 근처에 있는 무기 오브젝트 (트리거 안에 들어왔을 때 저장)
    private GameObject nearbyWeapon;
    // 무기 장착 상태를 저장하는 변수
    private bool isEquipped = false;
    // 메인 카메라 참조를 저장할 변수
    private Camera mainCamera;
    // 무기의 초기 회전값 (모델 방향을 맞추기 위함)
    private Quaternion originRotation;

    void Start()
    {
        // 성능을 위해 메인 카메라를 미리 찾아 저장
        mainCamera = Camera.main;

        // --- 뷰모델(ViewModel) 설정: Arm을 카메라의 자식으로 만들어 화면에 고정 ---
        if (mainCamera != null)
        {
            // 1. Arm(holdPoint)을 메인 카메라의 자식으로 설정
            holdPoint.SetParent(mainCamera.transform);

            // 2. Arm의 위치를 카메라 기준으로 오른쪽 아래로 조정 (이 값들을 직접 수정해서 위치 조정 가능)
            holdPoint.localPosition = new Vector3(0.3f, -0.2f, 0.5f);

            // 3. 무기 모델이 앞을 보도록 하는 초기 회전값을 저장
            originRotation = Quaternion.Euler(0, 180f, 0);

            // 4. 무기가 카메라에 잘려보이지 않도록 nearClipPlane 값을 조정
            mainCamera.nearClipPlane = 0.01f;
        }
        else
        {
            Debug.LogError("Main Camera를 찾을 수 없습니다! Main Camera에 'MainCamera' 태그가 설정되어 있는지 확인해주세요.");
        }
    }

    void Update()
    {
        // 무기를 들고 있을 때만 스웨이 로직 실행
        if (isEquipped)
        {
            // 1. 마우스 움직임 값 읽어오기
            float mouseX = Mouse.current.delta.x.ReadValue() * swayAmount;
            float mouseY = Mouse.current.delta.y.ReadValue() * swayAmount;

            // 2. 마우스 움직임에 따른 목표 회전값 계산
            Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
            Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);
            Quaternion targetRotation = originRotation * rotationX * rotationY;

            // 3. 무기의 현재 회전값을 목표 회전값으로 부드럽게 변경 (Slerp)
            // 총이 도는애니메이션
            holdPoint.localRotation = Quaternion.Slerp(holdPoint.localRotation, targetRotation, smoothAmount * Time.deltaTime);
        }
    }

    // PlayerInput 컴포넌트의 'Invoke Unity Events'에서 호출될 함수
    public void OnEquip(InputAction.CallbackContext context)
    {
        // 키가 '눌렸을 때'만 반응하도록 설정
        if (!context.performed)
        {
            return;
        }

        // 주울 수 있는 무기가 있을 때만 로직 실행
        if (nearbyWeapon != null)
        {
            // 만약 이미 다른 무기를 들고 있다면, 교체를 위해 기존 무기 파괴
            if (isEquipped)
            {
                Debug.Log(equippedWeapon.name + "을(를) 버리고 " + nearbyWeapon.name + "을(를) 장착합니다.");
                // 현재 들고있는 무기 오브젝트를 파괴한다.
                Destroy(equippedWeapon);
            }

            // --- 여기부터는 새로운 무기를 장착하는 공통 로직 ---

            // Arm 오브젝트 활성화
            holdPoint.gameObject.SetActive(true);

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
            if (weaponCollider != null)
            {
                weaponCollider.enabled = false;
            }

            // 무기를 장착했으므로 상태를 true로 변경 (이미 true였어도 상관없음)
            isEquipped = true;
            // 주변에 있던 무기는 이제 내가 들었으므로 null로 초기화
            nearbyWeapon = null;

            Debug.Log(equippedWeapon.name + " 무기를 장착했다!");
        }
        // 주울 수 있는 무기가 없을 때 (참고용)
        else if(isEquipped)
        {
            // 이 부분은 '무기 버리기' 로직을 넣을 수 있는 공간
             Debug.Log("주변에 교체할 무기가 없습니다.");
        }
    }

    // isTrigger가 켜진 다른 콜라이더 안에 들어갔을 때 호출되는 함수
    private void OnTriggerEnter(Collider other)
    {
        // 1. 들어온 오브젝트의 태그가 "Weapon"이라면
        // 무기를 들고 있는 상태에서도 다른 무기를 감지해야 교체할 수 있으므로, !isEquipped 조건을 제거
        if (other.CompareTag("Weapon"))
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