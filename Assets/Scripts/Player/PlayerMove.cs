using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    // 카메라의 Transform을 참조하기 위한 변수
    public Transform cameraTransform;
    
    private Vector2 moveInput;
    private Rigidbody rb;
    private WeaponHold weaponHold;
    private bool isFiring = false; // 발사 버튼이 눌리고 있는지 여부

    void Awake() // Start 대신 Awake 사용
    {
        rb = GetComponent<Rigidbody>();
        weaponHold = GetComponent<WeaponHold>(); // WeaponHold 초기화

        // cameraTransform 초기화 (기존 Start() 로직)
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // --- 이동 로직 (기존과 동일) ---
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        Vector3 moveDirection = right * moveInput.x + forward * moveInput.y;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
        // ------------------------------------
        
        // --- 자동 발사 로직 ---
        // 발사 버튼을 누르고 있고, 무기를 들고 있을 때
        if (isFiring && weaponHold != null && weaponHold.equippedWeapon != null)
        {
            Gun currentGun = weaponHold.equippedWeapon.GetComponent<Gun>();
            // 현재 총이 존재하고, '자동 발사'가 활성화된 총일 경우에만 매 프레임 발사 시도
            if (currentGun != null && currentGun.gunData.isAutomatic)
            {
                currentGun.TryFire();
            }
        }
    }
    
    // InputActions의 Move 액션에서 호출
    public void OnMove(InputAction.CallbackContext context)
    {
        // UI가 열려있으면 입력을 무시한다.
        if (UIManager.IsUIOpen)
        {
            moveInput = Vector2.zero; // 움직임 입력을 0으로 초기화
            return;
        }
        moveInput = context.ReadValue<Vector2>();
    }

    // InputActions의 Fire 액션에서 호출
    public void OnFire(InputAction.CallbackContext context)
    {
        // UI가 열려있으면 입력을 무시한다.
        if (UIManager.IsUIOpen)
        {
            isFiring = false; // UI가 열리면 발사 상태를 강제로 해제
            return;
        }

        if (context.performed) // 버튼을 처음 눌렀을 때
        {
            isFiring = true;

            // --- 단발 무기 발사 로직 ---
            if (weaponHold != null && weaponHold.equippedWeapon != null)
            {
                Gun currentGun = weaponHold.equippedWeapon.GetComponent<Gun>();
                // 현재 총이 존재하고, '자동 발사'가 아닌 총일 경우에만 여기서 한 번 발사
                if (currentGun != null && !currentGun.gunData.isAutomatic)
                {
                    currentGun.TryFire();
                }
            }
        }
        else if (context.canceled) // 버튼에서 손을 뗐을 때
        {
            isFiring = false;
        }
    }

    // InputActions의 Reload 액션에서 호출
    public void OnReload(InputAction.CallbackContext context)
    {
        // UI가 열려있거나, 키가 눌리는 순간이 아니면 실행하지 않는다.
        if (UIManager.IsUIOpen || !context.performed)
        {
            return;
        }

        // WeaponHold 스크립트와 장착된 무기가 있는지 확인
        if (weaponHold != null && weaponHold.equippedWeapon != null)
        {
            // 현재 들고 있는 무기에서 Gun 컴포넌트를 가져온다.
            Gun currentGun = weaponHold.equippedWeapon.GetComponent<Gun>();
            if (currentGun != null)
            {
                // Gun 컴포넌트가 있다면, 재장전 함수를 호출한다.
                currentGun.Reload();
            }
        }
    }
}
