using UnityEngine;

/// <summary>
/// 게임 월드에 드랍된 아이템을 천천히 회전시켜 시각적으로 잘 보이게 만든다.
/// 이 컴포넌트는 아이템이 인벤토리에 있거나 장착되었을 때는 비활성화되어야 한다.
/// </summary>
public class ItemRotator : MonoBehaviour
{
    [Tooltip("초당 회전 속도")]
    [SerializeField]
    private float rotationSpeed = 30f;

    void Update()
    {
        // Y축을 기준으로 아이템을 회전시킨다.
        // Time.deltaTime을 곱해 프레임 속도에 관계없이 일정한 속도를 유지한다.
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}