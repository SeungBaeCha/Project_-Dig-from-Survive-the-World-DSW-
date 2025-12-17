using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 게임의 모든 조합법을 관리하고, 유효한 조합인지 판별하는 시스템.
/// </summary>
public class CraftingSystem : MonoBehaviour
{
    [Tooltip("게임에 존재하는 모든 레시피 목록")]
    [SerializeField] private List<CraftingRecipe> recipes;

    /// <summary>
    /// 주어진 재료 목록과 일치하는 레시피를 찾는다.
    /// 재료의 순서는 상관없다.
    /// </summary>
    /// <param name="materials">플레이어가 조합 슬롯에 올린 재료들</param>
    /// <returns>일치하는 레시피를 찾으면 해당 레시피를, 없으면 null을 반환한다.</returns>
    public CraftingRecipe FindRecipe(List<ItemData> materials)
    {
        // 모든 레시피를 하나씩 확인
        foreach (var recipe in recipes)
        {
            // 1. 재료의 개수가 레시피와 다르면, 이 레시피는 아니다.
            if (materials.Count != recipe.materials.Count)
            {
                continue; // 다음 레시피로 넘어간다.
            }

            // 2. 재료 목록이 레시피와 일치하는지 확인 (순서 상관없이)
            // 플레이어가 넣은 재료들을 복사해서 임시 리스트를 만든다. (원본을 건드리지 않기 위해)
            var materialsCopy = new List<ItemData>(materials);
            bool allMaterialsFound = true;

            // 레시피에 필요한 재료들을 하나씩 보면서,
            foreach (var recipeMaterial in recipe.materials)
            {
                // 플레이어가 넣은 재료 중에 해당 재료가 있는지 찾는다.
                if (materialsCopy.Contains(recipeMaterial))
                {
                    // 찾았으면, 임시 리스트에서 하나를 지운다. (중복 재료 체크를 위함)
                    materialsCopy.Remove(recipeMaterial);
                }
                else
                {
                    // 플레이어가 넣은 재료 중에 필요한 재료가 하나라도 없으면,
                    allMaterialsFound = false; // 일치 실패!
                    break; // 더 볼 것도 없이 이 레시피는 아니다.
                }
            }
            
            // 모든 재료가 다 있었다면, 레시피를 찾은 것이다!
            if (allMaterialsFound)
            {
                return recipe;
            }
        }

        // 모든 레시피를 다 뒤져봤는데도 일치하는게 없었다.
        return null;
    }
}
