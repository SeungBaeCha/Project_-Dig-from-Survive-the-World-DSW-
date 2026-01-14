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

    void Awake() // Start 대신 Awake 사용
    {
        rb = GetComponent<Rigidbody>();

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
    }
    
    // InputActions의 Move 액션에서 호출
    public void OnMove(InputAction.CallbackContext context)
    {
        // 게임이 실제로 일시정지 상태일 때만 입력을 무시한다.
        if (UIManager.IsGamePaused)
        {
            moveInput = Vector2.zero; // 움직임 입력을 0으로 초기화
            return;
        }
        moveInput = context.ReadValue<Vector2>();
    }
}