using UnityEngine;

/// <summary>
/// 무기 개체에 부착되어 줍기/버리기 상태를 관리하는 스크립트
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Weapon : MonoBehaviour
{
    // 무기의 물리적 상태를 제어하기 위한 컴포넌트
    private Rigidbody rb;
    private Collider col;

    void Awake()
    {
        // 컴포넌트들을 미리 찾아놓는다
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    /// <summary>
    /// 무기가 주워졌을 때 호출될 함수
    /// </summary>
    /// <param name="parent">무기가 속하게 될 부모 Transform (플레이어의 WeaponHolder)</param>
    public void PickUp(Transform parent)
    {
        // 물리 효과를 비활성화해서 플레이어를 따라다니게 함
        rb.isKinematic = true;
        col.enabled = false;

        // 플레이어의 무기 홀더 자식으로 들어감
        transform.SetParent(parent);
        // 위치와 회전값을 초기화해서 홀더의 위치에 정확히 맞춤
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// 무기가 버려졌을 때 호출될 함수
    /// </summary>
    public void Drop()
    {
        // 부모-자식 관계를 해제
        transform.SetParent(null);

        // 물리 효과를 다시 활성화해서 땅에 떨어지게 함
        rb.isKinematic = false;
        col.enabled = true;

        // 살짝 앞으로 던지는 힘을 가함
        rb.AddForce(transform.forward * 2f, ForceMode.Impulse);
    }
}
