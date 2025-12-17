using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // UI 이벤트 사용을 위해 추가

/// <summary>
/// 조합 창의 각 슬롯(재료, 결과물)을 관리하는 스크립트.
/// 각 슬롯은 하나의 아이템 데이터를 가질 수 있으며, 아이콘을 표시하고 클릭 이벤트를 처리한다.
/// </summary>
[RequireComponent(typeof(Image))]
public class CraftingSlot : MonoBehaviour, IPointerClickHandler
{
    public ItemData currentItem { get; private set; }

    [Tooltip("아이템 아이콘을 표시할 이미지 컴포넌트. 보통 자기 자신을 넣는다.")]
    [SerializeField] private Image itemIcon;
    
    private CraftingWindow craftingWindow; // 나를 관리하는 부모 창

    /// <summary>
    /// 이 슬롯을 초기화하는 함수.
    /// </summary>
    public void Initialize(CraftingWindow parentWindow)
    {
        craftingWindow = parentWindow;
        ClearSlot();
    }

    /// <summary>
    /// 이 슬롯에 아이템을 설정하고 아이콘을 표시한다.
    /// </summary>
    public void SetItem(ItemData item)
    {
        currentItem = item;

        if (itemIcon == null) return; // 아이콘이 없으면 아무것도 안함

        if (currentItem != null)
        {
            itemIcon.sprite = currentItem.icon;
            itemIcon.color = new Color(1, 1, 1, 1);
        }
        else
        {
            ClearSlot();
        }
    }

    /// <summary>
    /// 이 슬롯을 비운다. 아이콘을 숨기고 데이터를 null로 만든다.
    /// </summary>
    public void ClearSlot()
    {
        currentItem = null;

        if (itemIcon == null) return; // 아이콘이 없으면 아무것도 안함

        itemIcon.sprite = null;
        itemIcon.color = new Color(1, 1, 1, 0);
    }

    /// <summary>
    /// 슬롯이 클릭되었을 때 호출된다.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 왼쪽 클릭이고, 슬롯에 아이템이 있을 때
        if (eventData.button == PointerEventData.InputButton.Left && currentItem != null)
        {
            // 부모인 조합 창에 '나 클릭됐어!' 라고 알린다.
            craftingWindow.OnCraftingSlotClicked(this);
        }
    }
}
