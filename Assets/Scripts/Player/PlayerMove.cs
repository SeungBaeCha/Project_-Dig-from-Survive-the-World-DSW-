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
        moveInput = context.ReadValue<Vector2>();
    }

    // InputActions의 Fire 액션에서 호출
    public void OnFire(InputAction.CallbackContext context)
    {
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
}
