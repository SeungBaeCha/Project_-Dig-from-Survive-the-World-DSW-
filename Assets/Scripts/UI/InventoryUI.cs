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
    [SerializeField] private Inventory playerInventory;
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

        // 클릭된 아이템이 '해금할 레시피(레시피 북)'를 가지고 있는 경우,
        // 해당 레시피를 CraftingSystem의 '활성 레시피'로 설정한다.
        if (clickedItem.recipeToUnlock != null)
        {
            CraftingSystem.Instance.SetActiveRecipe(clickedItem.recipeToUnlock);
            Debug.Log($"'{clickedItem.itemName}' 아이템 클릭. '{clickedItem.recipeToUnlock.name}' 레시피를 활성화합니다.");
            
            // 참고: 기존의 아이템 사용(소모) 로직은 제거됨.
            // playerInventory.UseItem(clickedItem); 
        }
    }
}
