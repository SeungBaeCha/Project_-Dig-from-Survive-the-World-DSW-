using UnityEngine;

/// <summary>
/// 파괴 가능한 지형의 기본 단위인 '청크'를 관리하는 스크립트.
/// 이 스크립트는 Chunk 프리팹에 부착되어야함
/// </summary>
public class Chunk : MonoBehaviour
{
    // 주석: 청크의 체력. 1이면 한 번의 공격으로 파괴
    [Tooltip("청크의 체력. 1이면 한 번의 공격으로 파괴됩니다.")]
    public int health = 1;

    // 주석: 청크가 파괴될 때 사용할 아이템 드랍 테이블
    [Tooltip("청크가 파괴될 때 사용할 아이템 드랍 테이블입니다.")]
    public LootTable lootTable;

    [Header("Unique Recipe Drop")]
    [Tooltip("이 청크가 낮은 확률로 드랍할 수 있는 고유 레시피")]
    public CraftingRecipe uniqueRecipeToUnlock;
    [Tooltip("위 '고유 레시피'의 아이템 프리팹")]
    public GameObject recipeItemPrefab;
    [Tooltip("고유 레시피가 드랍될 확률 (0.0 ~ 1.0)")]
    [Range(0f, 1f)]
    public float uniqueRecipeDropChance = 0.05f; // 5% 확률

    /// <summary>
    /// 외부에서 이 청크에 데미지를 주기 위해 호출하는 함수
    /// </summary>
    /// <param name="damageAmount">입힐 데미지의 양</param>
    public void TakeDamage(int damageAmount)
    {
        // 주석: 받은 데미지만큼 체력을 감소
        health -= damageAmount;

        // 주석: 체력이 0 이하로 떨어졌는지 확인
        if (health <= 0)
        {
            // 주석: 체력이 다 닳으면 Die 함수를 호출하여 청크를 파괴
            Die();
        }
    }

    /// <summary>
    /// 청크가 파괴될 때 호출되는 함수
    /// </summary>
    private void Die()
    {
        // --- 유니크 레시피 드랍 로직 ---
        // 월드 중복 체크 로직을 제거하여 더 직관적이고 효율적으로 변경합니다.
        // 조건 1: 유니크 레시피와 프리팹이 할당되어 있고,
        // 조건 2: 아직 발견되지 않은 레시피이며,
        // 조건 3: 드랍 확률을 통과했을 때
        if (uniqueRecipeToUnlock != null &&
            recipeItemPrefab != null &&
            !uniqueRecipeToUnlock.isDiscovered &&
            Random.value < uniqueRecipeDropChance)
        {
            // 위 조건을 통과하면 바로 아이템을 생성합니다.
            Instantiate(recipeItemPrefab, transform.position, Quaternion.identity);
        }
        
        // --- 기존 재료 드랍 로직 (유니크 드랍과 별개로 실행되도록 수정) ---
        if (lootTable != null)
        {
            // 주석: 현재 청크의 위치에 아이템을 드랍
            lootTable.SpawnLoot(transform.position);
        }

        // 주석: 이 게임 오브젝트(청크)를 씬에서 파괴
        Destroy(gameObject);
    }
}
