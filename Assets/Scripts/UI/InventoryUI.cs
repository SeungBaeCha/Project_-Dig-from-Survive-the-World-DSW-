using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 인벤토리를 시각적으로 보여주고 상호작용하는 UI.
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
        // playerInventory가 할당되었다면, 인벤토리 변경 이벤트가 발생할 때마다 RefreshUI가 호출되도록 등록
        if (playerInventory != null)
        {
            playerInventory.onInventoryChanged += RefreshUI;
        }

        // 초기 UI 상태를 한번 그려준다.
        RefreshUI();

        // 시작할 땐 닫아둔다.
        isOpen = false;
        inventoryPanel.SetActive(false);
    }
    
    // 오브젝트가 파괴될 때 이벤트 구독을 취소해야 메모리 누수를 막을 수 있어.
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
    /// 인벤토리 데이터를 기반으로 UI를 다시 그린다.
    /// </summary>
    private void RefreshUI()
    {
        // playerInventory가 연결 안되어 있으면 아무것도 하지 않는다.
        if (playerInventory == null) return;

        // 실제 인벤토리의 아이템 목록을 가져온다.
        List<ItemData> items = playerInventory.GetItems();

        // 슬롯 개수 맞추기: 현재 아이템 수보다 슬롯이 적으면 더 만들고, 많으면 일단 냅둔다.
        while (inventorySlots.Count < items.Count)
        {
            GameObject newSlotGO = Instantiate(slotPrefab, slotsParent);
            InventorySlot newSlot = newSlotGO.GetComponent<InventorySlot>();
            newSlot.Initialize(this);
            inventorySlots.Add(newSlot);
        }

        // 모든 슬롯에 아이템을 채워넣거나, 비운다.
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (i < items.Count)
            {
                inventorySlots[i].SetItem(items[i]);
            }
            else
            {
                inventorySlots[i].ClearSlot();
            }
        }
    }

    /// <summary>
    /// 인벤토리 슬롯이 클릭되었을 때 호출되는 함수 (InventorySlot이 호출)
    /// </summary>
    public void OnSlotClicked(InventorySlot clickedSlot)
    {
        if (craftingWindow != null && craftingWindow.gameObject.activeInHierarchy)
        {
            // 클릭된 슬롯의 아이템을 조합창의 재료 슬롯에 추가 시도
            bool added = craftingWindow.AddMaterial(clickedSlot.currentItem);

            // 조합창에 재료가 성공적으로 추가되었다면,
            // 플레이어의 인벤토리에서 해당 아이템을 제거한다. (UI는 이벤트로 자동 갱신됨)
            if (added)
            {
                playerInventory.RemoveItem(clickedSlot.currentItem);
            }
        }
    }
}
