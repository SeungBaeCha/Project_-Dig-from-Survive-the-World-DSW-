using UnityEngine;

/// <summary>
/// 모든 아이템 데이터의 기반이 되는 ScriptableObject.
/// 아이템 이름, 설명, 아이콘 등 공통 정보를 가진다.
/// </summary>
public class ItemData : ScriptableObject
{
    [Header("공통 정보")]
    [Tooltip("아이템의 이름 (UI에 표시)")]
    public string itemName;

    [Tooltip("아이템에 대한 설명")]
    [TextArea]
    public string description;

    [Tooltip("인벤토리나 UI에 표시될 아이템 아이콘")]
    public Sprite icon;

}
