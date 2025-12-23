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

    [Tooltip("이 아이템의 게임 오브젝트 프리팹 (세상에 생성될 때 사용)")]
    public GameObject itemPrefab;

    [Header("사용 효과")]
    [Tooltip("이 아이템을 '사용'했을 때 해금될 레시피. 레시피 아이템이 아니라면 비워두세요.")]
    public CraftingRecipe recipeToUnlock;

}
