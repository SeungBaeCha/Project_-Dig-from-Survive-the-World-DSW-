using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임에 존재하는 모든 조합법(레시피)의 목록을 담고 있는 ScriptableObject.
/// </summary>
[CreateAssetMenu(fileName = "RecipeBook", menuName = "Data/Recipe Book")]
public class RecipeBook : ScriptableObject
{
    [Header("레시피 목록")]
    [Tooltip("게임에 등장하는 모든 레시피 에셋을 여기에 등록하세요.")]
    public List<CraftingRecipe> allRecipes;

    /// <summary>
    /// 모든 레시피의 '발견' 상태를 초기화한다. (테스트 및 게임 초기화용)
    /// </summary>
    public void ResetAllDiscoveries()
    {
        foreach (var recipe in allRecipes)
        {
            recipe.ResetDiscovery();
        }
        Debug.Log("모든 레시피의 '발견' 상태가 초기화되었다.");
    }
}