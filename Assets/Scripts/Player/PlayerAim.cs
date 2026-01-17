using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    [Header("카메라 감도")]
    [Tooltip("마우스 감도를 조절합니다. 1.0 ~ 10.0 사이의 값을 권장합니다.")]
    [SerializeField] private float sensitivity = 2.0f;

    void Awake()
    {
        // 시네머신의 축 값을 가져갈 때 우리가 만든 함수를 대신 사용하도록 설정
        CinemachineCore.GetInputAxis = GetAxisCustom;
    }

    /// <summary>
    /// SettingsMenu에서 감도 값을 설정하기 위한 public 메서드
    /// </summary>
    public void SetSensitivity(float newSensitivity)
    {
        sensitivity = newSensitivity;
    }

    /// <summary>
    /// 현재 감도 값을 외부에서 읽기 위한 public 메서드 (UI 동기화용)
    /// </summary>
    public float GetSensitivity()
    {
        return sensitivity;
    }

    // 시네머신 입력을 제어하는 커스텀 함수
    private float GetAxisCustom(string axisName)
    {
        // UI가 열려있으면 카메라 움직임 멈춤
        if (UIManager.IsUIOpen || Time.timeScale <= 0f) // Time.deltaTime 대신 Time.timeScale로 확인
        {
            return 0f;
        }
        
        // Input System을 통해 마우스의 델타 값을 직접 읽어옴
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // 프레임당 마우스 움직임(mouseDelta)에 감도를 곱한 값을 시네머신에 전달
        // 시네머신이 이 값을 받아 내부적으로 프레임에 독립적인 부드러운 움직임으로 변환해 줌
        if (axisName == "Mouse X")
        {
            return mouseDelta.x * sensitivity;
        }
        else if (axisName == "Mouse Y")
        {
            return mouseDelta.y * sensitivity;
        }

        // 다른 축 이름이면 0을 반환
        return 0;
    }
}