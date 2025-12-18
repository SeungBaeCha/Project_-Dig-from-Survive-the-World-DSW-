using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 내 모든 조합법(Recipe)의 발견 상태를 관리하는 싱글턴 클래스.
/// 게임 시작 시 모든 레시피를 '미발견' 상태로 초기화하는 역할을 한다.
/// </summary>
public class RecipeBook : MonoBehaviour
{
    // --- 싱글턴 ---
    public static RecipeBook Instance { get; private set; }

    [Header("관리할 레시피 목록")]
    [Tooltip("게임에 존재하는 모든 레시피 ScriptableObject 목록")]
    public List<CraftingRecipe> allRecipes;

    private void Awake()
    {
        // --- 싱글턴 패턴 구현 ---
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않도록 설정
        }
        else
        {
            Destroy(gameObject); // 이미 인스턴스가 있다면 이 오브젝트는 파괴
            return;
        }

        // --- 레시피 상태 초기화 ---
        // 게임이 시작될 때, 모든 레시피를 '미발견' 상태로 초기화한다.
        // 이는 게임을 재시작했을 때 항상 동일한 상태에서 시작하도록 보장한다.
        // 나중에 게임 저장/불러오기 기능을 구현한다면 이 로직은 수정되어야 한다.
        ResetAllRecipes();
    }

    /// <summary>
    /// allRecipes 목록에 있는 모든 레시피의 isDiscovered 상태를 false로 초기화합니다.
    /// </summary>
    public void ResetAllRecipes()
    {
        Debug.Log("모든 레시피를 '미발견' 상태로 초기화합니다.");
        foreach (var recipe in allRecipes)
        {
            if (recipe != null)
            {
                recipe.ResetDiscovery();
            }
        }
    }

    /// <summary>
    /// 주어진 레시피를 '발견' 상태로 만듭니다.
    /// </summary>
    /// <param name="recipeToDiscover">발견할 레시피</param>
    public void DiscoverRecipe(CraftingRecipe recipeToDiscover)
    {
        if (recipeToDiscover != null && allRecipes.Contains(recipeToDiscover))
        {
            if (!recipeToDiscover.isDiscovered)
            {
                recipeToDiscover.Discover();
            }
            else
            {
                Debug.LogWarning($"이미 발견된 레시피입니다: {recipeToDiscover.name}");
            }
        }
        else
        {
            Debug.LogError("RecipeBook에 등록되지 않았거나 null인 레시피입니다.");
        }
    }
}
