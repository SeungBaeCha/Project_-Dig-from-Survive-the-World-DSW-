using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // 성능을 위해 시작할 때 메인 카메라를 찾아 저장해 둡니다.
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // 카메라가 바라보는 방향의 반대로 UI가 바라보게 만듭니다.
        // 이렇게 하면 UI의 정면이 항상 카메라를 향하게 됩니다.
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }
}
