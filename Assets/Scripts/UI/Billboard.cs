using UnityEngine;

/// <summary>
/// 이 스크립트가 부착된 오브젝트가 항상 메인 카메라를 바라보게 만든다. (빌보드 효과)
/// 월드 스페이스 UI가 플레이어 시점에 맞춰 항상 정면으로 보이게 할 때 사용한다.
/// </summary>
public class Billboard : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        // 성능을 위해 메인 카메라의 Transform을 미리 찾아 저장해둔다.
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("Billboard 스크립트가 메인 카메라를 찾지 못했습니다! 카메라에 'MainCamera' 태그가 있는지 확인해주세요.");
        }
    }

    // LateUpdate는 모든 Update 함수가 호출된 후에 실행된다.
    // 카메라의 움직임이 모두 끝난 후에 UI의 방향을 업데이트하기 때문에, 떨림 현상을 방지할 수 있다.
    void LateUpdate()
    {
        // 메인 카메라를 찾았다면
        if (mainCameraTransform != null)
        {
            // 카메라의 반대 방향을 바라보게 해서, 카메라가 오브젝트의 정면을 보도록 만든다.
            transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward,
                             mainCameraTransform.rotation * Vector3.up);
        }
    }
}
