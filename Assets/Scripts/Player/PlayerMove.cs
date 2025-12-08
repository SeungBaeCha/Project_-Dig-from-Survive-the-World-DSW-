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
    private WeaponHold weaponHold; // WeaponHold 변수 추가

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
        // 카메라의 정면과 오른쪽 방향을 가져옴
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        
        // 수평 움직임을 위해 y축 값은 0으로 설정
        forward.y = 0;
        right.y = 0;
        
        // 방향 벡터의 길이를 1로 만들어 속도를 일정하게 유지
        forward.Normalize();
        right.Normalize();
        
        // 입력값과 카메라 방향을 조합하여 최종 이동 방향을 계산
        Vector3 moveDirection = right * moveInput.x + forward * moveInput.y;
        
        // 계산된 방향으로 캐릭터를 이동시킴
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
    
    // InputActions의 Move 액션에서 호출
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // InputActions의 Fire 액션에서 호출
    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed) // 버튼을 누르는 순간 (Performd)
        {
            if (weaponHold != null && weaponHold.equippedWeapon != null)
            {
                Gun currentGun = weaponHold.equippedWeapon.GetComponent<Gun>();
                if (currentGun != null)
                {
                    currentGun.TryFire();
                }
                else
                {
                    // Debug.LogError("장착된 오브젝트는 Gun 스크립트를 가지고 있지 않습니다!");
                }
            }
            else
            {
                // Debug.Log("총을 장착하고 있지 않습니다.");
            }
        }
    }
}
