using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // TextMeshPro를 사용하기 위해 네임스페이스 추가

public class LootSlot : MonoBehaviour, IPointerClickHandler
{
    public Image itemIcon; // 아이템 아이콘을 표시할 UI Image
    public TextMeshProUGUI quantityText; // 아이템 개수를 표시할 TextMeshProUGUI
    
    private ItemData item; // 이 슬롯에 할당된 아이템 데이터
    private LootBox sourceLootBox; // 이 슬롯이 내용물을 보여주고 있는 원본 LootBox

    /// <summary>
    /// 슬롯에 아이템을 추가하고 UI를 업데이트한다.
    /// </summary>
    /// <param name="newItem">표시할 아이템</param>
    /// <param name="quantity">표시할 아이템의 개수</param>
    /// <param name="sourceBox">아이템의 출처가 되는 LootBox</param>
    public void AddItem(ItemData newItem, int quantity, LootBox sourceBox)
    {
        item = newItem;
        sourceLootBox = sourceBox;

        // 아이콘 설정
        itemIcon.sprite = item.icon;
        itemIcon.enabled = true;

        // 개수 텍스트 설정
        if (quantity > 1)
        {
            quantityText.text = quantity.ToString();
            quantityText.enabled = true;
        }
        else
        {
            quantityText.enabled = false;
        }
    }

    /// <summary>
    /// 슬롯을 비우고 아이콘과 텍스트를 숨긴다.
    /// </summary>
    public void ClearSlot()
    {
        item = null;
        sourceLootBox = null;

        itemIcon.sprite = null;
        itemIcon.enabled = false;
        
        quantityText.text = "";
        quantityText.enabled = false;
    }

    /// <summary>
    /// 이 슬롯이 클릭되었을 때 호출된다.
    /// </summary>
    /// <param name="eventData">클릭 이벤트 데이터</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 더블클릭인지 확인하고, 슬롯에 아이템이 있는지 확인
        if (eventData.clickCount == 2 && item != null)
        {
            // 실제 아이템 이동 로직은 LootBoxUI에 위임한다.
            LootBoxUI.Instance.AttemptTransferItem(item, sourceLootBox);
        }
    }
}