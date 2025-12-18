using UnityEngine;

/// <summary>
/// 월드에 드랍된 '레시피 아이템'의 동작을 관리하는 클래스.
/// 플레이어가 이 아이템을 획득하면 RecipeBook에 해당 레시피를 등록한다.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class RecipePickupController : MonoBehaviour
{
    [Header("레시피 정보")]
    [Tooltip("이 아이템을 통해 얻게 될 조합법(레시피)")]
    public CraftingRecipe recipeToUnlock;

    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트가 'Player' 태그를 가지고 있는지 확인
        if (other.CompareTag("Player"))
        {
            // 해제할 레시피가 할당되어 있는지, RecipeBook 인스턴스가 존재하는지 확인
            if (recipeToUnlock != null && RecipeBook.Instance != null)
            {
                // RecipeBook에 레시피를 '발견' 상태로 등록 요청
                RecipeBook.Instance.DiscoverRecipe(recipeToUnlock);

                // 획득 효과음이나 이펙트 등을 여기에 추가할 수 있음

                // 레시피 아이템 오브젝트를 파괴
                Destroy(gameObject);
            }
            else
            {
                if (recipeToUnlock == null)
                {
                    Debug.LogError($"{gameObject.name}: recipeToUnlock이 할당되지 않았습니다!");
                }
                if (RecipeBook.Instance == null)
                {
                    Debug.LogError("RecipeBook의 인스턴스를 찾을 수 없습니다! 씬에 RecipeBook 오브젝트가 있는지 확인하세요.");
                }

                // 문제가 있어도 일단 아이템은 사라지게 처리
                Destroy(gameObject);
            }
        }
    }
}
