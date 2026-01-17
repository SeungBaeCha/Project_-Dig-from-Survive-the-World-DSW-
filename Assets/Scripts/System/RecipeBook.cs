using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임에 존재하는 모든 조합법(레시피)의 목록을 관리하고, 발견 상태를 추적하는 클래스.
/// </summary>
public class RecipeBook : MonoBehaviour
{
    public static RecipeBook Instance { get; private set; }

    [Header("레시피 목록")]
    [Tooltip("게임에 등장하는 모든 레시피 에셋을 여기에 등록하세요.")]
    public List<CraftingRecipe> allRecipes;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 레시피를 '발견' 상태로 만든다.
    /// </summary>
    /// <param name="recipeToDiscover">발견할 레시피</param>
    public void DiscoverRecipe(CraftingRecipe recipeToDiscover)
    {
        if (recipeToDiscover != null)
        {
            // 실제 레시피 목록에 있는 레시피인지 확인(선택적)
            if (allRecipes.Contains(recipeToDiscover))
            {
                recipeToDiscover.Discover();
            }
            else
            {
                Debug.LogWarning($"RecipeBook에 등록되지 않은 레시피({recipeToDiscover.name})를 발견하려고 합니다.");
                // 등록되지 않은 레시피도 발견 처리할 수 있도록 그냥 호출
                recipeToDiscover.Discover();
            }
        }
    }


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