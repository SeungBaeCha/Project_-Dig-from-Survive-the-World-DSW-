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
    [SerializeField] private float swayAmount;
    // 스웨이 부드러움. 무기가 원래 위치로 돌아오는 속도.
    [SerializeField] private float smoothAmount;

    [Header("무기 드랍 설정")]
    // 무기를 떨어뜨릴 때 던지는 힘의 크기
    [SerializeField] private float dropForce;
    // 무기를 버린 후 플레이어와 물리적 충돌을 무시할 시간 (초)
    [SerializeField] private float ignoreCollisionTime;

    [Header("무기 줍기 설정")]
    // 무기를 버린 후 다시 주울 수 있을 때까지의 쿨다운 시간
    [SerializeField] private float pickupCooldown;

    [Header("UI 설정")]
    // 크로스헤어 UI 오브젝트를 연결할 변수
    public GameObject crosshair;

    // 플레이어 근처에 있는 무기 오브젝트 (트리거 안에 들어왔을 때 저장)
    private GameObject nearbyWeapon;
    // 무기 장착 상태를 저장하는 변수
    private bool isEquipped = false;
    // 메인 카메라 참조를 저장할 변수
    private Camera mainCamera;
    // 무기의 초기 회전값 (모델 방향을 맞추기 위함)
    private Quaternion originRotation;
    // 플레이어 자신의 콜라이더
    private Collider playerCollider;

    void Start()
    {
        // 성능을 위해 메인 카메라를 미리 찾아 저장
        mainCamera = Camera.main;
        // 플레이어 콜라이더를 찾아 저장
        playerCollider = GetComponent<Collider>();

        // --- 뷰모델(ViewModel) 설정: Arm을 카메라의 자식으로 만들어 화면에 고정 ---
        if (mainCamera != null)
        {
            holdPoint.SetParent(mainCamera.transform);
            holdPoint.localPosition = new Vector3(0.3f, -0.2f, 0.5f);
            originRotation = Quaternion.Euler(0, 180f, 0);
            mainCamera.nearClipPlane = 0.01f;
        }
        else
        {
            Debug.LogError("Main Camera를 찾을 수 없습니다! Main Camera에 'MainCamera' 태그가 설정되어 있는지 확인해주세요.");
        }

        // 크로스헤어가 할당되어 있다면, 게임 시작 시 비활성화
        if (crosshair != null)
        {
            crosshair.SetActive(false);
        }
    }

    void Update()
    {
        // 무기를 들고 있을 때만 스웨이 로직 실행
        if (isEquipped)
        {
            float mouseX = Mouse.current.delta.x.ReadValue() * swayAmount;
            float mouseY = Mouse.current.delta.y.ReadValue() * swayAmount;
            Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
            Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);
            Quaternion targetRotation = originRotation * rotationX * rotationY;
            holdPoint.localRotation = Quaternion.Slerp(holdPoint.localRotation, targetRotation, smoothAmount * Time.deltaTime);
        }
    }

    public void OnEquip(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (nearbyWeapon != null)
        {
            if (isEquipped)
            {
                Debug.Log(equippedWeapon.name + "을(를) 버리고 " + nearbyWeapon.name + "을(를) 장착합니다.");
                DropWeapon(equippedWeapon); // 기존 무기 버리기
            }

            EquipWeapon(nearbyWeapon); // 새 무기 장착
        }
        else if (isEquipped)
        {
            Debug.Log("주변에 교체할 무기가 없습니다.");
        }
    }
    
    public void OnDrop(InputAction.CallbackContext context)
    {
        if (!context.performed || !isEquipped) return;

        Debug.Log(equippedWeapon.name + "을(를) 버렸습니다.");
        DropWeapon(equippedWeapon);

        equippedWeapon = null;
        isEquipped = false;

        if (crosshair != null)
        {
            crosshair.SetActive(false);
        }
    }

    private void EquipWeapon(GameObject weaponToEquip)
    {
        holdPoint.gameObject.SetActive(true);
        equippedWeapon = weaponToEquip;
        equippedWeapon.transform.SetParent(holdPoint);
        equippedWeapon.transform.localPosition = Vector3.zero;
        equippedWeapon.transform.localRotation = Quaternion.identity;

        MeshCollider weaponMeshCollider = equippedWeapon.GetComponent<MeshCollider>();
        if (weaponMeshCollider != null) weaponMeshCollider.enabled = false;

        Rigidbody weaponRb = equippedWeapon.GetComponent<Rigidbody>();
        if (weaponRb != null) weaponRb.isKinematic = true;

        isEquipped = true;
        nearbyWeapon = null;

        if (crosshair != null) crosshair.SetActive(true);

        Debug.Log(equippedWeapon.name + " 무기를 장착했다!");
    }

    private void DropWeapon(GameObject weaponToDrop)
    {
        // 무기를 플레이어의 자식에서 해제
        weaponToDrop.transform.SetParent(null);

        // 무기의 물리적 속성 복원
        MeshCollider weaponMeshCollider = weaponToDrop.GetComponent<MeshCollider>();
        if (weaponMeshCollider != null) weaponMeshCollider.enabled = true;

        Rigidbody weaponRb = weaponToDrop.GetComponent<Rigidbody>();
        if (weaponRb != null)
        {
            weaponRb.isKinematic = false;
            weaponRb.AddForce(mainCamera.transform.forward * dropForce, ForceMode.Impulse);
        }

        // --- 핵심 로직 수정 ---
        // 1. 방금 버린 무기와 플레이어의 물리적 충돌을 잠시 무시
        Collider droppedWeaponCollider = weaponToDrop.GetComponent<Collider>();
        if (playerCollider != null && droppedWeaponCollider != null)
        {
            StartCoroutine(IgnoreCollisionTemporarily(droppedWeaponCollider));
        }

        // 2. 방금 버린 무기를 일정 시간 동안 주울 수 없도록 설정
        Weapon weaponScript = weaponToDrop.GetComponent<Weapon>();
        if (weaponScript != null)
        {
            // Weapon 스크립트의 코루틴을 호출하여 줍기 쿨다운을 시작
            weaponScript.StartPickupCooldown(pickupCooldown);
        }
    }

    private IEnumerator IgnoreCollisionTemporarily(Collider weaponCollider)
    {
        // 1. 충돌 무시 시작
        Physics.IgnoreCollision(playerCollider, weaponCollider, true);
        
        // 2. 설정된 시간만큼 대기
        yield return new WaitForSeconds(ignoreCollisionTime);
        
        // 3. 충돌 무시 해제
        Physics.IgnoreCollision(playerCollider, weaponCollider, false);
    }

    public void SetNearbyWeapon(GameObject weapon)
    {
        nearbyWeapon = weapon;
        Debug.Log("주울 수 있는 무기 발견: " + nearbyWeapon.name);
    }

    public void ClearNearbyWeapon(GameObject weapon)
    {
        if (nearbyWeapon == weapon)
        {
            nearbyWeapon = null;
            Debug.Log("무기 범위에서 벗어났다.");
        }
    }
}
