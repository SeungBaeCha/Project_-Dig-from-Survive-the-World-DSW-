using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 제작에 필요한 재료와 개수를 정의하는 클래스.
/// </summary>
[System.Serializable]
public class RequiredMaterial
{
    [Tooltip("필요한 재료 아이템")]
    public ItemData item;
    [Tooltip("필요한 재료의 개수")]
    [Range(1, 999)]
    public int quantity = 1;
}


/// <summary>
/// 아이템 조합법(레시피) 하나를 정의하는 ScriptableObject.
/// </summary>
[CreateAssetMenu(fileName = "New Recipe", menuName = "Data/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("조합법 정보")]
    [Tooltip("조합에 필요한 재료와 개수 목록")]
    public List<RequiredMaterial> materials;

    [Tooltip("조합 결과물 아이템")]
    public ItemData result;

    [Header("레시피 발견 상태")]
    [Tooltip("플레이어가 이 레시피를 발견했는지 여부. true이면 조합 가능")]
    public bool isDiscovered = false;

    /// <summary>
    /// 이 레시피를 '발견' 상태로 변경한다.
    /// </summary>
    public void Discover()
    {
        isDiscovered = true;
        // 참고: ScriptableObject의 변경사항은 플레이 중에만 유지된다.
        // 게임을 재시작하면 isDiscovered의 값은 에디터에서 설정한 기본값으로 초기화된다.
        // 영구적인 저장을 위해서는 별도의 저장 시스템(ex. PlayerPrefs, Json)이 필요하다.
        Debug.Log($"레시피 발견: {this.name}");
    }

    /// <summary>
    /// 테스트 및 디버깅을 위해 레시피를 '미발견' 상태로 초기화한다.
    /// </summary>
    public void ResetDiscovery()
    {
        isDiscovered = false;
    }
}