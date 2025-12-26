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
    private Inventory _playerInventory;

    public Inventory PlayerInventory => _playerInventory;

    // 플레이어 Transform 참조 (효율성을 위해 캐싱)
    private Transform playerTransform;

    // 메인 카메라 참조 (효율성을 위해 캐싱)
    private Camera mainCamera;

    [Header("Recipe Management")]
    [Tooltip("게임에 존재하는 모든 레시피 목록. 게임 시작 시 '미발견' 상태로 초기화된다.")]
    [SerializeField] private List<CraftingRecipe> allRecipes;

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

        // --- 모든 레시피의 '발견' 상태를 '미발견'으로 초기화 ---
        // ScriptableObject는 에디터에서 플레이 세션 간의 변경사항을 저장하기 때문이다.
        // 게임을 시작할 때마다 이 초기화 과정이 필요하다.
        foreach (var recipe in allRecipes)
        {
            if (recipe != null)
            {
                recipe.ResetDiscovery();
            }
        }
    }

    private void Start()
    {
        // 플레이어 참조를 찾아서 할당 (한 번만)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerInventory = player.GetComponent<Inventory>();
            playerTransform = player.transform;
        }

        if (_playerInventory == null)
        {
            Debug.LogError("CraftingSystem이 플레이어의 Inventory를 찾을 수 없다!");
        }
        if (playerTransform == null)
        {
            Debug.LogError("CraftingSystem이 플레이어의 Transform을 찾을 수 없다!");
        }

        // 메인 카메라 참조 (한 번만)
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("CraftingSystem: Main Camera를 찾을 수 없다! 'MainCamera' 태그가 설정되어 있는지 확인해줘.");
        }
    }

    /// <summary>
    /// 제작 창에 표시할 레시피를 설정한다.
    /// </summary>
    public void SetActiveRecipe(CraftingRecipe recipe)
    {
        ActiveRecipe = recipe;
        // Debug.Log($"활성화된 레시피: {recipe.name}"); // 너무 자주 호출될 수 있으므로 주석 처리
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
            Debug.LogError("제작하려는 레시피가 null이다.");
            return false;
        }

        // 1. 재료 확인
        if (!_playerInventory.HasMaterials(recipeToCraft.materials))
        {
            UIManager.Instance.ShowNotification("재료가 부족하다!", 1.5f);
            return false;
        }

        // 2. 재료 소모
        _playerInventory.RemoveMaterials(recipeToCraft.materials);

        // 3. 결과물 아이템을 월드에 생성 (인벤토리가 아닌 플레이어 근처에)
        if (recipeToCraft.result.itemPrefab != null)
        {
            if (mainCamera != null)
            {
                // 카메라의 위치와 바라보는 방향을 기준으로 1단위 앞, 그리고 0.2단위 위에 생성
                // (카메라 또는 플레이어 모델에 끼는 것을 방지)
                Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * 1f + Vector3.up * 0.2f;

                // --- 아이템 생성 및 이름표(Billboard) 활성화 ---
                // 1. 프리팹으로부터 아이템 게임오브젝트를 생성한다.
                GameObject craftedItemGO = Instantiate(recipeToCraft.result.itemPrefab, spawnPosition, Quaternion.identity);

                // 2. 생성된 아이템의 자식 중 'Billboard' 컴포넌트를 찾는다.
                //    true 파라미터는 비활성화된 자식 오브젝트에서도 컴포넌트를 찾도록 한다.
                Billboard billboard = craftedItemGO.GetComponentInChildren<Billboard>(true);

                // 3. Billboard 컴포넌트를 찾았다면, 해당 게임오브젝트(이름표 UI)를 활성화시킨다.
                if (billboard != null)
                {
                    billboard.gameObject.SetActive(true);
                }

                // 4. 생성된 아이템의 'ItemRotator' 컴포넌트를 찾아 활성화한다.
                //    아이템이 월드에 놓여있을 때 시각적 효과를 주기 위함이다.
                ItemRotator rotator = craftedItemGO.GetComponent<ItemRotator>();
                if (rotator != null)
                {
                    rotator.enabled = true;
                }
                
                UIManager.Instance.ShowNotification($"'{recipeToCraft.result.itemName}' 제작 성공!");
                return true;
            }
            else
            {
                Debug.LogError("메인 카메라 참조가 없다. 아이템을 월드에 생성할 수 없다.");
                // TODO: 이 경우 재료를 다시 돌려주는 로직 고려
                return false;
            }
        }
        else
        {
            Debug.LogError($"'{recipeToCraft.result.itemName}'에 연결된 프리팹이 없다. 월드에 생성할 수 없다.");
            // TODO: 이 경우 재료를 다시 돌려주는 로직 고려
            return false;
        }
    }
}