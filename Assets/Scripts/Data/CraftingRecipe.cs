using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템 조합법(레시피) 하나를 정의하는 ScriptableObject.
/// </summary>
[CreateAssetMenu(fileName = "New Recipe", menuName = "Data/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("조합법 정보")]
    [Tooltip("조합에 필요한 재료 아이템 목록")]
    public List<ItemData> materials;

    [Tooltip("조합 결과물 아이템")]
    public ItemData result;
}
