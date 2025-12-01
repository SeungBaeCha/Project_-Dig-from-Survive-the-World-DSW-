
using UnityEngine;

/// <summary>
/// 아이템을 손에 들었을 때의 위치(Position)와 회전(Rotation) 오프셋을 지정하는 스크립트.
/// WeaponHold 스크립트가 이 컴포넌트의 값을 읽어 아이템의 최종 모습을 조정한다.
/// </summary>
public class ItemProperties : MonoBehaviour
{
    [Tooltip("손에 들었을 때 적용될 위치 오프셋")]
    public Vector3 positionOffset = Vector3.zero;

    [Tooltip("손에 들었을 때 적용될 회전 오프셋 (Euler Angles)")]
    public Vector3 rotationOffset = Vector3.zero;
}
