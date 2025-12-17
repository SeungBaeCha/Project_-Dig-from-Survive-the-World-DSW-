using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // UI 이벤트를 사용하기 위해 필요

/// <summary>
/// 인벤토리 UI의 각 슬롯.
/// 클릭 이벤트를 감지해서 InventoryUI에 알려주는 역할을 한다.
/// </summary>
public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    // 이 슬롯이 보여주고 있는 아이템 데이터
    public ItemData currentItem { get; private set; }
    
    // 아이콘을 표시할 이미지와, 슬롯이 비었을 때의 기본 스프라이트
    [SerializeField] private Image itemIcon;
    [SerializeField] private Sprite emptySlotSprite;

    // 이 슬롯을 관리하는 부모 InventoryUI
    private InventoryUI inventoryUI;

    /// <summary>
    /// 슬롯 초기화 (InventoryUI가 호출해준다)
    /// </summary>
    public void Initialize(InventoryUI parentUI)
    {
        inventoryUI = parentUI;
        ClearSlot();
    }

    /// <summary>
    /// 슬롯에 아이템을 설정하고 아이콘을 표시한다.
    /// </summary>
    public void SetItem(ItemData item)
    {
        currentItem = item;
        if (currentItem != null)
        {
            itemIcon.sprite = currentItem.icon;
            itemIcon.color = Color.white;
        }
        else
        {
            ClearSlot();
        }
    }

    /// <summary>
    /// 슬롯을 비운다.
    /// </summary>
    public void ClearSlot()
    {
        currentItem = null;
        itemIcon.sprite = emptySlotSprite;
        itemIcon.color = new Color(1, 1, 1, 0.5f); // 비어있을 땐 반투명하게
    }
    
    /// <summary>
    /// 이 UI 요소가 클릭되었을 때 자동으로 호출되는 함수 (IPointerClickHandler 덕분)
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 슬롯이 비어있지 않고, 왼쪽 클릭을 했을 때
        if (currentItem != null && eventData.button == PointerEventData.InputButton.Left)
        {
            // InventoryUI에 '나(이 슬롯) 클릭됐어!' 라고 알린다.
            inventoryUI.OnSlotClicked(this);
        }
    }
}
