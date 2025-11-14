using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // 성능을 위해 시작할 때 메인 카메라를 찾아 저장
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // 카메라가 바라보는 방향의 반대로 UI가 카메라를 바라보게 함
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }
}
