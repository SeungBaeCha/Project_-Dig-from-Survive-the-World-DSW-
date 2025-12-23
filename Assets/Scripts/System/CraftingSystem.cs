using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 아이템 제작 로직을 총괄하는 싱글턴 매니저.
/// </summary>
public class CraftingSystem : MonoBehaviour
{
    // 싱글턴 인스턴스
    public static CraftingSystem Instance { get; private set; }

    // 플레이어 인벤토리 참조
    private Inventory playerInventory;

    // 현재 활성화된(UI에 표시될) 레시피
    public CraftingRecipe ActiveRecipe { get; private set; }

    private void Awake()
    {
        // 싱글턴 패턴 구현
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // 게임 내에 계속 유지되도록 설정
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        // 플레이어 인벤토리 참조를 찾아서 할당
        // Player 태그를 가진 오브젝트에서 Inventory 컴포넌트를 찾는다.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInventory = player.GetComponent<Inventory>();
        }

        if (playerInventory == null)
        {
            Debug.LogError("CraftingSystem이 플레이어의 Inventory를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 제작 창에 표시할 레시피를 설정합니다.
    /// </summary>
    public void SetActiveRecipe(CraftingRecipe recipe)
    {
        ActiveRecipe = recipe;
        Debug.Log($"활성화된 레시피: {recipe.name}");
    }

    /// <summary>
    /// 특정 레시피로 아이템 제작을 시도하는 함수.
    /// </summary>
    /// <param name="recipeToCraft">제작할 레시피</param>
    /// <returns>제작 성공 시 true, 실패 시 false</returns>
    public bool CraftItem(CraftingRecipe recipeToCraft)
    {
        if (recipeToCraft == null)
        {
            Debug.LogError("제작하려는 레시피가 null입니다.");
            return false;
        }

        // 1. 재료 확인
        if (!playerInventory.HasMaterials(recipeToCraft.materials))
        {
            Debug.Log("제작 재료가 부족합니다.");
            return false;
        }

        // 2. 재료 소모
        playerInventory.RemoveMaterials(recipeToCraft.materials);

        // 3. 결과물 아이템을 월드에 생성 (인벤토리가 아닌 플레이어 근처에)
        if (recipeToCraft.result.itemPrefab != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // 플레이어의 위치를 기준으로 약간 앞에 생성
                Vector3 spawnPosition = player.transform.position + player.transform.forward * 1.5f;
                Instantiate(recipeToCraft.result.itemPrefab, spawnPosition, Quaternion.identity);
                Debug.Log($"{recipeToCraft.result.itemName} 제작 성공! 월드에 생성되었습니다.");
                return true;
            }
            else
            {
                Debug.LogError("플레이어 오브젝트를 찾을 수 없습니다. 아이템을 월드에 생성할 수 없습니다.");
                // TODO: 이 경우 재료를 다시 돌려주는 로직 고려
                return false;
            }
        }
        else
        {
            Debug.LogError($"{recipeToCraft.result.itemName}에 연결된 프리팹이 없습니다. 월드에 생성할 수 없습니다.");
            // TODO: 이 경우 재료를 다시 돌려주는 로직 고려
            return false;
        }
    }
}