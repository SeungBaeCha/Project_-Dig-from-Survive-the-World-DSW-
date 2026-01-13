using UnityEngine;

/// <summary>
/// 사용 시 체력이나 허기 등 능력치를 회복시켜주는 소모성 아이템의 데이터입니다.
/// </summary>
[CreateAssetMenu(fileName = "New Consumable Data", menuName = "DataAssets/Item/Consumable Data")]
public class ConsumableData : ItemData
{
    [Header("소모품 효과")]
    [Tooltip("이 아이템을 사용했을 때 회복되는 체력의 양입니다.")]
    public int healthToRestore;

    [Tooltip("이 아이템을 사용했을 때 회복되는 허기의 양입니다.")]
    public int hungerToRestore;
}
