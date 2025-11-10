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
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 만약 cameraTransform이 Inspector에서 할당되지 않았다면, Main Camera를 찾아서 할당
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
    
    // InputActions의 Move 액션에서 호출됩니다
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}
