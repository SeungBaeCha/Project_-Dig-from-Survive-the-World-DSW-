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
    [Header("홀더 설정")]
    public GameObject equippedWeapon; // 현재 들고 있는 무기 오브젝트. 비어있으면 무기가 없는 상태.
    public Transform holdPoint; // 무기를 장착할 손의 위치.

    [Header("무기 스웨이(Sway) 설정")]
    [SerializeField] private float swayAmount; // 스웨이 강도. 마우스 움직임에 얼마나 민감하게 반응할지 결정.
    [SerializeField] private float smoothAmount; // 스웨이 부드러움. 무기가 원래 위치로 돌아오는 속도.

    [Header("무기 드랍 설정")]
    [SerializeField] private float dropForce; // 무기를 떨어뜨릴 때 가하는 힘의 크기.
    [SerializeField] private float ignoreCollisionTime; // 무기를 버린 후 플레이어와 물리적 충돌을 무시할 시간 (초).

    [Header("무기 줍기 설정")]
    [SerializeField] private float pickupCooldown; // 무기를 버린 후 다시 주울 수 있을 때까지의 쿨다운 시간.

    [Header("UI 설정")]
    public GameObject crosshair; // 크로스헤어 UI 오브젝트.

    private GameObject nearbyWeapon; // 플레이어 근처에 있는 무기 오브젝트. (트리거 안에 들어왔을 때 저장)
    private bool isEquipped = false; // 무기 장착 상태.
    private bool isFiring = false; // 현재 발사 중인지 여부 (자동 발사를 위해)
    private Camera mainCamera; // 메인 카메라 참조.
    private Quaternion originRotation; // 무기의 초기 회전값(모델 방향을 맞추기 위함).
    private Collider playerCollider; // 플레이어 자신의 콜라이더.

    // --- 무기 컴포넌트 캐싱 ---
    private Gun currentGun;
    private Shovel currentShovel;
    private ShovelHold currentShovelHold;

    void Start()
    {
        // 메인 카메라 및 플레이어 콜라이더를 찾아 저장.
        mainCamera = Camera.main;
        playerCollider = GetComponent<Collider>();

        // 홀더(Arm)를 카메라의 자식으로 만들어 화면에 고정.
        if (mainCamera != null)
        {
            holdPoint.SetParent(mainCamera.transform);
            holdPoint.localPosition = new Vector3(0.3f, -0.2f, 0.5f);
            originRotation = Quaternion.Euler(0, 180f, 0);
            mainCamera.nearClipPlane = 0.01f;
        }
        else
        {
            Debug.LogError("Main Camera를 찾을 수 없다! Main Camera에 'MainCamera' 태그가 설정되어 있는지 확인해줘.");
        }

        // 게임 시작 시 크로스헤어 비활성화.
        if (crosshair != null)
        {
            crosshair.SetActive(false);
        }
    }

    void Update()
    {
        // 무기를 들고 있을 때만 로직 실행
        if (isEquipped)
        {
            // 무기 스웨이 로직
            HandleSway();

            // 자동 발사 로직 (isFiring 플래그가 true일 때)
            if (isFiring && currentGun != null)
            {
                currentGun.TryFire();
            }
        }
    }

    /// <summary>
    /// 무기 스웨이 로직을 처리하는 헬퍼 메서드
    /// </summary>
    private void HandleSway()
    {
        float mouseX = Mouse.current.delta.x.ReadValue() * swayAmount;
        float mouseY = Mouse.current.delta.y.ReadValue() * swayAmount;
        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);
        Quaternion targetRotation = originRotation * rotationX * rotationY;
        holdPoint.localRotation = Quaternion.Slerp(holdPoint.localRotation, targetRotation, smoothAmount * Time.deltaTime);
    }

    /// <summary>
    /// 발사 입력 처리 (PlayerInput에서 호출)
    /// </summary>
    public void OnFire(InputAction.CallbackContext context)
    {
        // UI가 열려있거나 무기가 없으면 발사 상태를 해제하고 중단
        if (UIManager.IsUIOpen || !isEquipped)
        {
            isFiring = false;
            return;
        }

        // 버튼이 눌려있는지 여부를 isFiring 플래그에 저장 (자동 발사는 Update에서 처리)
        isFiring = context.ReadValueAsButton();

        // 단발 무기 및 도구 사용 처리 (버튼을 처음 누른 시점)
        if (context.performed)
        {
            if (currentGun != null)
            {
                // 들고 있는 총이 단발이라면 여기서 즉시 발사
                if (!currentGun.gunData.isAutomatic)
                {
                    currentGun.TryFire();
                }
            }
            else if (currentShovel != null)
            {
                // 총이 아니고 삽이라면 사용
                currentShovel.Use();
            }
        }
    }

    /// <summary>
    /// 재장전 입력 처리 (PlayerInput에서 호출)
    /// </summary>
    public void OnReload(InputAction.CallbackContext context)
    {
        // 키를 눌렀다 떼는 시점에만 실행하고, 총이 없으면 중단
        if (!context.performed || currentGun == null) return;
        
        currentGun.Reload();
    }

    [Header("상호작용 설정")]
    [SerializeField] private float interactionDistance = 2f; // 상호작용 가능 거리
    [SerializeField] private LayerMask interactableLayer; // 상호작용할 레이어

    /// <summary>
    /// 무기 장착 입력 처리. F키를 누르면 상호작용 또는 무기 장착을 시도한다.
    /// </summary>
    public void OnEquip(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        // 최우선 순위: 상자 열기 시도
        if (TryOpenLootBox())
        {
            return; // 상자를 열었으면 무기 줍기 로직을 실행하지 않음
        }

        // 상자가 근처에 없을 때만 무기 줍기/교체 로직 실행
        if (nearbyWeapon != null)
        {
            if (isEquipped)
            {
                DropWeapon(); // 기존 무기 버리기
            }
            EquipWeapon(nearbyWeapon); // 새 무기 장착.
        }
    }

    /// <summary>
    /// 주변의 루트박스를 찾아 열기를 시도하는 헬퍼 메서드
    /// </summary>
    /// <returns>상자를 열었으면 true, 아니면 false</returns>
    private bool TryOpenLootBox()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactionDistance, interactableLayer);
        foreach (var col in colliders)
        {
            if (col.TryGetComponent<LootBox>(out var lootBox))
            {
                lootBox.OpenBox();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 무기 버리기 입력 처리.
    /// </summary>
    public void OnDrop(InputAction.CallbackContext context)
    {
        if (!context.performed || !isEquipped) return;

        DropWeapon(); // 현재 무기 버리기
    }

    /// <summary>
    /// 무기를 장착하는 실제 로직.
    /// </summary>
    private void EquipWeapon(GameObject weaponToEquip)
    {
        holdPoint.gameObject.SetActive(true);
        equippedWeapon = weaponToEquip;
        equippedWeapon.transform.SetParent(holdPoint);

        // 아이템의 속성(위치/회전 오프셋)을 적용.
        ApplyItemProperties(equippedWeapon);

        // 아이템의 물리적 상태 설정 (콜라이더 비활성화, 키네마틱 설정).
        SetWeaponPhysics(equippedWeapon, false);
        
        // Weapon 스크립트에게 주워졌음을 알려 UI 등을 처리하게 함
        var weaponComponent = equippedWeapon.GetComponentInChildren<Weapon>();
        weaponComponent?.HandlePickup();

        isEquipped = true;
        nearbyWeapon = null;
        if (crosshair != null) crosshair.SetActive(true);

        // --- 장착한 무기 타입에 따른 컴포넌트 캐싱 및 초기화 ---
        CacheAndInitializeWeaponComponents(equippedWeapon);
    }
    
    /// <summary>
    /// 무기를 버리는 실제 로직.
    /// </summary>
    private void DropWeapon()
    {
        if (equippedWeapon == null) return;

        GameObject weaponToDrop = equippedWeapon;

        // 장착된 무기 컴포넌트 초기화 해제
        DeinitializeWeaponComponents();

        weaponToDrop.transform.SetParent(null); // 무기를 부모에서 분리.
        SetWeaponPhysics(weaponToDrop, true); // 물리 상태 활성화

        // 리지드바디에 힘을 가해 버리는 동작 연출
        var weaponRb = weaponToDrop.GetComponent<Rigidbody>();
        if (weaponRb != null)
        {
            weaponRb.AddForce(mainCamera.transform.forward * dropForce, ForceMode.Impulse);
        }
        
        // 방금 버린 무기와 플레이어의 물리적 충돌 일시 무시.
        var droppedWeaponCollider = weaponToDrop.GetComponent<Collider>();
        if (playerCollider != null && droppedWeaponCollider != null)
        {
            StartCoroutine(IgnoreCollisionTemporarily(droppedWeaponCollider));
        }

        // Weapon 스크립트에게 버려졌음을 알려 쿨다운 시작 및 UI 처리
        var weaponScript = weaponToDrop.GetComponentInChildren<Weapon>();
        if (weaponScript != null)
        {
            weaponScript.HandleDrop(); // UI 다시 활성화
            weaponScript.StartPickupCooldown(pickupCooldown);
        }

        // 장착 상태 해제
        equippedWeapon = null;
        isEquipped = false;
        if (crosshair != null) crosshair.SetActive(false);
    }

    // --- Equip/Drop 헬퍼 메서드 ---

    private void ApplyItemProperties(GameObject item)
    {
        if (item.TryGetComponent<ItemProperties>(out var properties))
        {
            item.transform.localPosition = properties.positionOffset;
            item.transform.localRotation = Quaternion.Euler(properties.rotationOffset);
        }
        else
        {
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
        }
    }

    private void SetWeaponPhysics(GameObject weapon, bool enabled)
    {
        // 모든 콜라이더 활성/비활성화
        var allColliders = weapon.GetComponentsInChildren<Collider>();
        foreach (var col in allColliders)
        {
            col.enabled = enabled;
        }

        // 리지드바디 키네마틱 설정
        if (weapon.TryGetComponent<Rigidbody>(out var weaponRb))
        {
            weaponRb.isKinematic = !enabled;
        }
    }
    
    private void CacheAndInitializeWeaponComponents(GameObject weapon)
    {
        // Gun 컴포넌트 캐싱 및 초기화
        currentGun = weapon.GetComponentInChildren<Gun>();
        if (currentGun != null)
        {
            currentGun.SetPlayerCollider(playerCollider);
            currentGun.SetPlayerCamera(mainCamera);
        }

        // Shovel 관련 컴포넌트 캐싱 및 초기화
        currentShovel = weapon.GetComponentInChildren<Shovel>();
        currentShovelHold = weapon.GetComponentInChildren<ShovelHold>();
        if (currentShovelHold != null)
        {
            currentShovelHold.Initialize(this);
        }
    }

    private void DeinitializeWeaponComponents()
    {
        // ShovelHold 초기화 해제
        if (currentShovelHold != null)
        {
            currentShovelHold.Deinitialize();
        }

        // 캐시된 컴포넌트 참조 해제
        currentGun = null;
        currentShovel = null;
        currentShovelHold = null;
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