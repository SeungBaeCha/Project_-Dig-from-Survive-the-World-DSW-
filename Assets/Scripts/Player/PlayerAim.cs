using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    [Header("카메라 감도")]
    [Tooltip("Time.deltaTime이 적용되므로, 100~200 사이의 높은 값을 사용해야 합니다.")]
    [SerializeField] private float sensitivity; // Time.deltaTime 보정을 위해 감도 값을 높여야 함

    void Start()
    {
        // 시네머신의 축 값을 가져갈 때 우리가 만든 함수를 대신 사용하도록 설정
        CinemachineCore.GetInputAxis = GetAxisCustom;
    }

    // 시네머신 입력을 제어하는 커스텀 함수
    private float GetAxisCustom(string axisName)
    {
        // Input System을 통해 마우스의 값을 직접 읽어옴
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        if (axisName == "Mouse X")
        {
            // 마우스의 X 움직임에 감도와 Time.deltaTime을 적용하여 반환
            // Time.deltaTime을 곱해 프레임 속도에 관계없이 일정한 속도를 보장
            return mouseDelta.x * sensitivity * Time.deltaTime;
        }
        else if (axisName == "Mouse Y")
        {
            // 마우스의 Y 움직임에 감도와 Time.deltaTime을 적용하여 반환
            return mouseDelta.y * sensitivity * Time.deltaTime;
        }

        // 다른 축 이름이면 0을 반환
        return 0;
    }
}