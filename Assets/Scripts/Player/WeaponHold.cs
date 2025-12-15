using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어가 무기를 장착하고, 버리고, 줍는 등의 동작을 관리하는 스크립트.
/// 무기의 스웨이(Sway) 효과 및 UI(크로스헤어) 활성화도 제어한다.
/// </summary>
public class WeaponHold : MonoBehaviour
{
    [Header("뷰모델 설정")]
    public GameObject equippedWeapon; // 현재 들고 있는 무기 오브젝트. 비어있으면 무기가 없는 상태.
    public Transform holdPoint; // 무기를 장착할 손의 위치.

    [Header("무기 스웨이(Sway) 설정")]
    [SerializeField] private float swayAmount; // 스웨이 강도. 마우스 움직임에 얼마나 민감하게 반응할지 결정.
    [SerializeField] private float smoothAmount; // 스웨이 부드러움. 무기가 원래 위치로 돌아오는 속도.

    [Header("무기 드랍 설정")]
    [SerializeField] private float dropForce; // 무기를 떨어뜨릴 때 던지는 힘의 크기.
    [SerializeField] private float ignoreCollisionTime; // 무기를 버린 후 플레이어와 물리적 충돌을 무시할 시간 (초).

    [Header("무기 줍기 설정")]
    [SerializeField] private float pickupCooldown; // 무기를 버린 후 다시 주울 수 있을 때까지의 쿨다운 시간.

    [Header("UI 설정")]
    public GameObject crosshair; // 크로스헤어 UI 오브젝트.

    private GameObject nearbyWeapon; // 플레이어 근처에 있는 무기 오브젝트. (트리거 안에 들어왔을 때 저장)
    private bool isEquipped = false; // 무기 장착 상태.
    private Camera mainCamera; // 메인 카메라 참조.
    private Quaternion originRotation; // 무기의 초기 회전값 (모델 방향을 맞추기 위함).
    private Collider playerCollider; // 플레이어 자신의 콜라이더.

    void Start()
    {
        // 메인 카메라 및 플레이어 콜라이더를 찾아 저장.
        mainCamera = Camera.main;
        playerCollider = GetComponent<Collider>();

        // 뷰모델(Arm)을 카메라의 자식으로 만들어 화면에 고정.
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

        // 게임 시작 시 크로스헤어 비활성화.
        if (crosshair != null)
        {
            crosshair.SetActive(false);
        }
    }

    void Update()
    {
        // 무기를 들고 있을 때만 스웨이 로직 실행.
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

    /// <summary>
    /// 무기 장착 입력 처리.
    /// </summary>
    public void OnEquip(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (nearbyWeapon != null)
        {
            if (isEquipped)
            {
                DropWeapon(equippedWeapon); // 기존 무기 버리기.
            }
            EquipWeapon(nearbyWeapon); // 새 무기 장착.
        }
    }
    
    /// <summary>
    /// 무기 버리기 입력 처리.
    /// </summary>
    public void OnDrop(InputAction.CallbackContext context)
    {
        if (!context.performed || !isEquipped) return;

        DropWeapon(equippedWeapon); // 현재 무기 버리기.
        equippedWeapon = null;
        isEquipped = false;
        if (crosshair != null)
        {
            crosshair.SetActive(false); // 크로스헤어 비활성화.
        }
    }

    /// <summary>
    /// 무기를 장착하는 실제 로직.
    /// </summary>
    private void EquipWeapon(GameObject weaponToEquip)
    {
        holdPoint.gameObject.SetActive(true);
        equippedWeapon = weaponToEquip;
        equippedWeapon.transform.SetParent(holdPoint); // 무기를 HoldPoint의 자식으로 설정.

        // 아이템의 속성(위치/회전 오프셋) 적용.
        ItemProperties properties = equippedWeapon.GetComponent<ItemProperties>();
        if (properties != null)
        {
            equippedWeapon.transform.localPosition = properties.positionOffset;
            equippedWeapon.transform.localRotation = Quaternion.Euler(properties.rotationOffset);
        }
        else
        {
            equippedWeapon.transform.localPosition = Vector3.zero;
            equippedWeapon.transform.localRotation = Quaternion.identity;
        }

        // 아이템의 모든 콜라이더 비활성화 (플레이어가 들고 있을 때는 물리 충돌 방지).
        Collider[] allColliders = equippedWeapon.GetComponentsInChildren<Collider>();
        foreach (var col in allColliders)
        {
            col.enabled = false;
        }

        // 리지드바디 키네마틱 설정 (물리 영향 받지 않도록).
        Rigidbody weaponRb = equippedWeapon.GetComponent<Rigidbody>();
        if (weaponRb != null) weaponRb.isKinematic = true;

        isEquipped = true;
        nearbyWeapon = null;
        if (crosshair != null) crosshair.SetActive(true); // 크로스헤어 활성화.

        // --- 장착된 무기 타입에 따른 추가 초기화 ---
        // Gun 컴포넌트가 있다면 플레이어 정보 전달.
        Gun gun = equippedWeapon.GetComponentInChildren<Gun>();
        if (gun != null)
        {
            gun.SetPlayerCollider(playerCollider);
            gun.SetPlayerCamera(mainCamera);
        }

        // ShovelHold 컴포넌트가 있다면 초기화 메소드 호출.
        ShovelHold shovelHold = equippedWeapon.GetComponentInChildren<ShovelHold>();
        if (shovelHold != null)
        {
            shovelHold.Initialize(this);
        }
    }

    /// <summary>
    /// 무기를 버리는 실제 로직.
    /// </summary>
    private void DropWeapon(GameObject weaponToDrop)
    {
        // ShovelHold 컴포넌트가 있다면 초기화 해제 메소드 호출.
        ShovelHold shovelHold = weaponToDrop.GetComponentInChildren<ShovelHold>();
        if (shovelHold != null)
        {
            shovelHold.Deinitialize();
        }

        weaponToDrop.transform.SetParent(null); // 무기를 부모에서 분리.

        // 무기의 모든 콜라이더 활성화.
        Collider[] allColliders = weaponToDrop.GetComponentsInChildren<Collider>();
        foreach (var col in allColliders)
        {
            col.enabled = true;
        }

        Rigidbody weaponRb = weaponToDrop.GetComponent<Rigidbody>();
        if (weaponRb != null)
        {
            weaponRb.isKinematic = false; // 리지드바디 키네마틱 해제 (물리 영향 받도록).
            weaponRb.AddForce(mainCamera.transform.forward * dropForce, ForceMode.Impulse); // 버리는 힘 적용.
        }
        
        // 방금 버린 무기와 플레이어의 물리적 충돌 잠시 무시.
        Collider droppedWeaponCollider = weaponToDrop.GetComponent<Collider>();
        if (playerCollider != null && droppedWeaponCollider != null)
        {
            StartCoroutine(IgnoreCollisionTemporarily(droppedWeaponCollider));
        }

        // 무기 스크립트가 있다면 줍기 쿨다운 시작.
        Weapon weaponScript = weaponToDrop.GetComponent<Weapon>();
        if (weaponScript != null)
        {
            weaponScript.StartPickupCooldown(pickupCooldown);
        }
    }

    /// <summary>
    /// 무기와 플레이어의 충돌을 일시적으로 무시하는 코루틴.
    /// </summary>
    private IEnumerator IgnoreCollisionTemporarily(Collider weaponCollider)
    {
        Physics.IgnoreCollision(playerCollider, weaponCollider, true); // 충돌 무시 시작.
        yield return new WaitForSeconds(ignoreCollisionTime); // 설정된 시간만큼 대기.
        Physics.IgnoreCollision(playerCollider, weaponCollider, false); // 충돌 무시 해제.
    }

    /// <summary>
    /// 근처에 무기가 감지되었을 때 호출.
    /// </summary>
    public void SetNearbyWeapon(GameObject weapon)
    {
        nearbyWeapon = weapon;
    }

    /// <summary>
    /// 근처 무기가 사라졌을 때 호출.
    /// </summary>
    public void ClearNearbyWeapon(GameObject weapon)
    {
        if (nearbyWeapon == weapon)
        {
            nearbyWeapon = null;
        }
    }
}
