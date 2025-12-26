using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    [Header("카메라 감도")]
    [SerializeField] private float sensitivity; // 기본 감도를 적절한 값으로 조절

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
            // 마우스의 X 움직임에 감도를 적용하여 반환
            return mouseDelta.x * sensitivity;
        }
        else if (axisName == "Mouse Y")
        {
            // 마우스의 Y 움직임에 감도를 적용하여 반환
            return mouseDelta.y * sensitivity;
        }

        // 다른 축 이름이면 0을 반환
        return 0;
    }
}