using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// LootBoxUI는 게임 내에 하나만 존재해야 하므로 싱글톤으로 만든다.
public class LootBoxUI : MonoBehaviour
{
    public static LootBoxUI Instance { get; private set; }

    [Tooltip("LootSlot들이 자식으로 있는 부모 오브젝트의 Transform")]
    public Transform slotsParent; // 슬롯들의 부모가 될 Transform
    [Tooltip("LootBox UI 전체 창")]
    public GameObject lootBoxWindow;
    [Tooltip("상호작용 거리보다 멀어지면 자동으로 UI가 닫힐 거리")]
    public float autoCloseDistance = 3f;
    
    [Header("슬롯 설정")]
    [Tooltip("생성할 슬롯 프리팹")]
    [SerializeField] private GameObject lootSlotPrefab;
    [Tooltip("생성할 최대 슬롯 개수")]
    [SerializeField] private int maxLootCapacity = 16;

    [Header("플레이어 연결")]
    [Tooltip("플레이어의 인벤토리. Inspector에서 직접 연결해야 함.")]
    [SerializeField] private Inventory playerInventory;

    private LootBox currentLootBox; // 현재 열려있는 LootBox
    private List<LootSlot> slots = new List<LootSlot>(); // 자식으로 있는 모든 LootSlot 스크립트들을 담을 리스트

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 최대 슬롯 개수만큼 미리 슬롯을 생성한다.
        for (int i = 0; i < maxLootCapacity; i++)
        {
            GameObject slotGO = Instantiate(lootSlotPrefab, slotsParent);
            LootSlot slot = slotGO.GetComponent<LootSlot>();
            if (slot != null)
            {
                slots.Add(slot);
            }
        }
        
        // UI 시작 시 비활성화
        lootBoxWindow.SetActive(false);
    }

    private void Update()
    {
        // UI가 열려있고, 현재 상자 및 플레이어 인벤토리가 지정되어 있을 때만 거리 체크
        if (lootBoxWindow.activeSelf && currentLootBox != null && playerInventory != null)
        {
            // 플레이어와 상자 사이의 거리를 계산
            float distance = Vector3.Distance(playerInventory.transform.position, currentLootBox.transform.position);

            // 거리가 자동 닫기 설정값보다 멀어지면 UI를 닫는다.
            if (distance > autoCloseDistance)
            {
                Close();
            }
        }
    }

    /// <summary>
    /// LootBox를 열고 해당 상자의 내용물을 UI에 표시한다.
    /// </summary>
    /// <param name="lootBox">열 상자</param>
    public void Open(LootBox lootBox)
    {
        currentLootBox = lootBox;
        lootBoxWindow.SetActive(true);
        
        // UIManager에게 UI 상태가 변경되었음을 알려 커서 및 게임 상태를 업데이트하도록 한다.
        UIManager.Instance.UpdateCursorAndGameState();

        UpdateUI();
    }

    /// <summary>
    /// LootBox UI를 닫는다.
    /// </summary>
    public void Close()
    {
        if (!lootBoxWindow.activeSelf) return; // 이미 닫혀있으면 중복 실행 방지

        lootBoxWindow.SetActive(false);
        currentLootBox = null;

        // UIManager에게 UI 상태가 변경되었음을 알려 커서 및 게임 상태를 업데이트하도록 한다.
        UIManager.Instance.UpdateCursorAndGameState();
    }

    /// <summary>
    /// currentLootBox의 아이템 목록을 기반으로 UI 슬롯들을 업데이트한다.
    /// </summary>
    private void UpdateUI()
    {
        if (currentLootBox == null) return;

        // 1. 상자의 아이템 목록을 스택(아이템 종류 + 개수)으로 변환한다.
        var itemStacks = new Dictionary<ItemData, int>();
        foreach (var item in currentLootBox.currentLoot)
        {
            if (itemStacks.ContainsKey(item))
            {
                itemStacks[item]++;
            }
            else
            {
                itemStacks[item] = 1;
            }
        }

        // Dictionary를 List<InventoryStack>으로 변환 (표시하기 쉽게)
        var displayStacks = new List<InventoryStack>();
        foreach (var pair in itemStacks)
        {
            displayStacks.Add(new InventoryStack(pair.Key, pair.Value));
        }

        // 2. 변환된 스택 정보를 기반으로 UI 슬롯을 업데이트한다.
        for (int i = 0; i < slots.Count; i++)
        {
            // 표시할 스택이 남아있다면
            if (i < displayStacks.Count)
            {
                // 해당 슬롯에 아이템과 개수 정보를 할당
                slots[i].AddItem(displayStacks[i].item, displayStacks[i].quantity, currentLootBox);
            }
            else
            {
                // 표시할 스택이 없다면 슬롯을 비운다.
                slots[i].ClearSlot();
            }
        }
    }

    /// <summary>
    /// 슬롯에서 요청한 아이템을 플레이어 인벤토리로 옮기는 것을 시도한다.
    /// </summary>
    /// <param name="item">옮길 아이템</param>
    /// <param name="sourceBox">아이템의 출처가 되는 상자</param>
    public void AttemptTransferItem(ItemData item, LootBox sourceBox)
    {
        if (playerInventory == null)
        {
            Debug.LogError("LootBoxUI에 PlayerInventory가 연결되지 않았습니다!");
            return;
        }

        bool wasAdded = playerInventory.AddItem(item);

        if (wasAdded)
        {
            sourceBox.RemoveItem(item);
            Refresh();
        }
        else
        {
            Debug.Log("인벤토리가 가득 찼습니다!");
            // TODO: UIManager를 통해 알림 메시지 표시
        }
    }
    
    // 아이템 획득 후 UI를 새로고침하기 위한 공용 함수
    public void Refresh()
    {
        UpdateUI();
    }
}
