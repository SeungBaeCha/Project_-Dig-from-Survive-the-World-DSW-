using UnityEngine;

/// <summary>
/// 파괴 가능한 지형의 기본 단위인 '청크'를 관리하는 스크립트.
/// 이 스크립트는 Chunk 프리팹에 부착되어야 한다.
/// </summary>
public class Chunk : MonoBehaviour
{
    // 주석: 청크의 체력. 1이면 한 번의 공격으로 파괴
    [Tooltip("청크의 체력. 1이면 한 번의 공격으로 파괴된다.")]
    public int health = 1;

    // 주석: 청크가 파괴될 때 사용할 아이템 드랍 테이블
    [Tooltip("청크가 파괴될 때 사용할 아이템 드랍 테이블이다.")]
    public LootTable lootTable;

    [Tooltip("청크가 파괴될 때 생성될 파티클 이펙트")]
    public GameObject digEffectPrefab;

    [Tooltip("땅을 팔 때 재생될 음향 효과")]
    public AudioClip digSound;
    [Tooltip("땅 파는 소리의 볼륨 (0.0 ~ 1.0)")]
    [Range(0f, 1f)]
    public float digVolume = 1.0f; // 기본 볼륨은 1.0f

    [Header("Unique Recipe Drop")]
    [Tooltip("이 청크가 낮은 확률로 드랍할 수 있는 고유 레시피")]
    public CraftingRecipe uniqueRecipeToUnlock;
    [Tooltip("위 '고유 레시피'의 아이템 프리팹")]
    public GameObject recipeItemPrefab;
    [Tooltip("고유 레시피가 드랍될 확률 (0.0 ~ 1.0)")]
    [Range(0f, 1f)]
    public float uniqueRecipeDropChance = 0.05f; // 5% 확률

    /// <summary>
    /// 외부에서 이 청크에 데미지를 주기 위해 호출되는 함수
    /// </summary>
    /// <param name="damageAmount">입힐 데미지 양</param>
    public void TakeDamage(int damageAmount)
    {
        // 땅 파는 소리 재생
        if (digSound != null)
        {
            AudioSource.PlayClipAtPoint(digSound, transform.position, digVolume);
        }

        // 데미지를 받으면 파티클 이펙트를 생성한다.
        if (digEffectPrefab != null && DiggableGrid.Instance != null)
        {
            DiggableGrid.Instance.GetPooledEffect(digEffectPrefab, transform.position, Quaternion.identity);
        }
        else if (digEffectPrefab == null)
        {
            Debug.LogError("Chunk: 이펙트 프리팹이 할당되지 않았습니다! Chunk 프리팹의 Inspector를 확인해주세요!");
        }
        
        // 주석: 받은 데미지만큼 체력을 감소
        health -= damageAmount;

        // 주석: 체력이 0 이하로 떨어졌는지 확인
        if (health <= 0)
        {
            // 주석: 체력이 다 닳았으면 Die 함수를 호출하여 청크를 파괴
            Die();
        }
    }

    /// <summary>
    /// 청크가 파괴될 때 호출되는 함수
    /// </summary>
    private void Die()
    {
        // --- 유니크 레시피 드랍 로직 ---
        if (uniqueRecipeToUnlock != null &&
            recipeItemPrefab != null &&
            !uniqueRecipeToUnlock.isDiscovered &&
            Random.value < uniqueRecipeDropChance)
        {
            Instantiate(recipeItemPrefab, transform.position, Quaternion.identity);
        }

        // --- 기본 재료 드랍 로직 (유니크 드랍과 별개로 실행) ---
        if (lootTable != null)
        {
            lootTable.SpawnLoot(transform.position);
        }

        // --- NavMesh 업데이트 요청 및 입구 등록 로직 ---
        // 핵심: 파괴된 위치를 부모 Grid에 '입구'로 등록하여 적들이 우회 경로로 사용하게 함
        DiggableGrid grid = GetComponentInParent<DiggableGrid>();
        if (grid != null)
        {
            grid.RegisterEntrance(transform.position);
            // grid.RequestNavMeshUpdate(); // NavMesh 업데이트는 이제 GameManager의 낮/밤 이벤트에 따라 자동 관리된다.
        }

        // 이 게임 오브젝트(청크)를 씬에서 파괴
        Destroy(gameObject);
    }
}
