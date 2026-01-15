using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 인벤토리를 시각적으로 보여주고 상호작용하는 UI.
/// 고정된 개수의 슬롯을 미리 생성하고 관리한다.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("UI 요소")]
    [Tooltip("인벤토리 UI의 최상위 Panel 오브젝트")]
    [SerializeField] private GameObject inventoryPanel;
    [Tooltip("인벤토리 슬롯들이 자식으로 들어갈 부모 Transform")]
    [SerializeField] private Transform slotsParent;
    [Tooltip("인벤토리 슬롯 프리팹")]
    [SerializeField] private GameObject slotPrefab;

    [Header("연결될 시스템")]
    [Tooltip("플레이어의 실제 인벤토리 데이터")]
    [SerializeField] public Inventory playerInventory;
    [Tooltip("아이템을 전달할 조합 창")]
    [SerializeField] private CraftingWindow craftingWindow;
    
    // 관리하는 모든 인벤토리 슬롯들
    private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    private bool isOpen = false;

    void Start()
    {
        if (playerInventory != null)
        {
            // 인벤토리 최대 용량만큼 슬롯을 미리 생성한다.
            for (int i = 0; i < playerInventory.MaxCapacity; i++)
            {
                GameObject newSlotGO = Instantiate(slotPrefab, slotsParent);
                InventorySlot newSlot = newSlotGO.GetComponent<InventorySlot>();
                newSlot.Initialize(this);
                inventorySlots.Add(newSlot);
            }
            
            // 인벤토리 변경 이벤트가 발생할 때마다 UI를 새로고침하도록 등록
            playerInventory.onInventoryChanged += RefreshUI;
        }

        // 초기 UI 상태를 한번 그려준다.
        RefreshUI();

        // 시작할 땐 닫아둔다.
        isOpen = false;
        inventoryPanel.SetActive(false);
    }
    
    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.onInventoryChanged -= RefreshUI;
        }
    }

    /// <summary>
    /// 인벤토리 UI 패널의 활성화 상태를 직접 설정한다.
    /// </summary>
    public void ToggleWindow(bool state)
    {
        isOpen = state;
        inventoryPanel.SetActive(isOpen);
    }

    /// <summary>
    /// 인벤토리 UI 패널의 활성화 상태를 토글한다.
    /// </summary>
    public void ToggleWindow()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);
    }

    /// <summary>
    /// 인벤토리 창이 현재 열려있는지 여부를 반환한다.
    /// </summary>
    public bool IsOpen()
    {
        return isOpen;
    }

    /// <summary>
    /// 인벤토리 데이터를 기반으로 UI를 다시 그린다.
    /// </summary>
    private void RefreshUI()
    {
        if (playerInventory == null) return;

        // 실제 인벤토리의 아이템 스택 목록을 가져온다.
        List<InventoryStack> stacks = playerInventory.GetStacks();

        // 모든 슬롯을 순회하며 내용을 업데이트한다.
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < stacks.Count)
            {
                // 아이템이 있는 경우: 슬롯에 스택 정보 설정
                inventorySlots[i].SetStack(stacks[i]);
            }
            else
            {
                // 아이템이 없는 경우: 슬롯을 비운다.
                inventorySlots[i].ClearSlot();
            }
        }
    }

    /// <summary>
    /// 인벤토리 슬롯이 클릭되었을 때 호출되는 함수 (InventorySlot이 호출)
    /// </summary>
    public void OnSlotClicked(InventorySlot clickedSlot)
    {
        // 클릭된 슬롯이 비어있거나, 아이템이 없으면 아무것도 하지 않는다.
        if (clickedSlot.currentStack == null || clickedSlot.currentStack.item == null)
        {
            return;
        }

        ItemData clickedItem = clickedSlot.currentStack.item;

        // 클릭된 아이템이 '해금할 레시피(레시피 북)'를 가진 경우,
        // Inventory 스크립트의 UseItem 메서드를 호출하여 아이템 사용 로직을 처리한다.
        if (clickedItem.recipeToUnlock != null)
        {
            playerInventory.UseItem(clickedItem); 
        }
        // 아이템이 소모품(ConsumableData)인 경우 (왼쪽 클릭 시 사용)
        else if (clickedItem is ConsumableData consumable)
        {
            // PlayerHealth 컴포넌트가 Inventory와 같은 GameObject에 있다고 가정
            // (UIManager에 playerInventory를 연결할 때 Inventory 컴포넌트가 Player GameObject에 있으므로,
            // GetComponent<PlayerHealth>()는 Player GameObject에서 PlayerHealth를 찾을 것이다.)
            PlayerHealth playerHealth = playerInventory.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.UseConsumable(consumable);
                playerInventory.RemoveItem(clickedItem, 1); // 사용했으니 인벤토리에서 1개 제거
            }
            else
            {
                Debug.LogWarning("플레이어의 PlayerHealth 컴포넌트를 찾을 수 없습니다. 소모품을 사용할 수 없습니다.");
            }
        }
    }

    /// <summary>
    /// 인벤토리 슬롯이 오른쪽 클릭되었을 때 아이템을 사용하거나 버리는 함수.
    /// </summary>
    public void OnSlotRightClicked(InventorySlot clickedSlot)
    {
        // 클릭된 슬롯이나 아이템이 유효하지 않으면 아무것도 하지 않음
        if (clickedSlot.currentStack == null || clickedSlot.currentStack.item == null) return;
        
        ItemData clickedItem = clickedSlot.currentStack.item;

        // 1. 아이템이 소모품(ConsumableData)인지 확인
        if (clickedItem is ConsumableData consumable)
        {
            // 플레이어의 PlayerHealth 컴포넌트를 찾아서 아이템 사용 함수 호출
            PlayerHealth playerHealth = playerInventory.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.UseConsumable(consumable);
                // 인벤토리에서 아이템 1개 제거
                playerInventory.RemoveItem(clickedItem, 1);
            }
        }
        // 2. 소모품이 아닐 경우, 기존의 버리기 로직 실행
        else
        {
            if (clickedItem.itemPrefab == null)
            {
                Debug.LogWarning($"'{clickedItem.itemName}' 아이템은 월드에 버릴 수 있는 프리팹이 없습니다.");
                return;
            }

            // --- 아이템을 월드에 생성 ---
            Transform playerTransform = playerInventory.transform;
            Vector3 spawnPosition = playerTransform.position + playerTransform.forward * 1.5f;

            GameObject droppedItemGO = Instantiate(clickedItem.itemPrefab, spawnPosition, Quaternion.identity);
            
            Billboard billboard = droppedItemGO.GetComponentInChildren<Billboard>(true);
            if (billboard != null)
            {
                billboard.gameObject.SetActive(true);
            }

            ItemRotator rotator = droppedItemGO.GetComponent<ItemRotator>();
            if (rotator != null)
            {
                rotator.enabled = true;
            }

            // --- 인벤토리에서 아이템 제거 ---
            playerInventory.RemoveItem(clickedItem, 1);
        }
    }
}
