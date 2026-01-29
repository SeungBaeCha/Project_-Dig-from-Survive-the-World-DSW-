using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // UI 이벤트를 사용하기 위해 필요
using TMPro; // TextMeshProUGUI를 사용하기 위해 필요

/// <summary>
/// 인벤토리 UI의 각 슬롯.
/// 아이콘과 아이템 개수를 표시하고, 클릭 이벤트를 감지해서 InventoryUI에 알려주는 역할을 한다.
/// </summary>
public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    // 이 슬롯이 보여주고 있는 아이템 스택 정보
    public InventoryStack currentStack { get; private set; }
    
    [Header("UI 요소")]
    [Tooltip("아이콘을 표시할 이미지")]
    [SerializeField] private Image itemIcon;
    [Tooltip("아이템 개수를 표시할 텍스트")]
    [SerializeField] private TextMeshProUGUI quantityText;
    
    [Header("기본 값")]
    [Tooltip("슬롯이 비었을 때의 기본 스프라이트")]
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
    /// 슬롯에 아이템 스택 정보를 설정하고 UI를 갱신한다.
    /// </summary>
    public void SetStack(InventoryStack stack)
    {
        currentStack = stack;

        // 스택 정보가 유효한지 확인
        if (currentStack != null && currentStack.item != null)
        {
            // 아이콘 설정
            itemIcon.sprite = currentStack.item.icon;
            itemIcon.color = Color.white;

            // 개수 텍스트 설정
            if (currentStack.quantity > 1)
            {
                quantityText.gameObject.SetActive(true);
                quantityText.text = currentStack.quantity.ToString();
            }
            else
            {
                // 1개 이하면 굳이 숫자를 표시하지 않는다.
                quantityText.gameObject.SetActive(false);
            }
        }
        else
        {
            // 스택 정보가 없으면 슬롯을 비운다.
            ClearSlot();
        }
    }

    /// <summary>
    /// 슬롯을 비우고 기본 상태로 되돌린다.
    /// </summary>
    public void ClearSlot()
    {
        currentStack = null;
        itemIcon.sprite = emptySlotSprite;
        itemIcon.color = new Color(1, 1, 1, 0.5f); // 비어있을 땐 반투명하게
        
        // 개수 텍스트도 비활성화
        if (quantityText != null)
        {
            quantityText.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 이 UI 요소가 클릭되었을 때 자동으로 호출되는 함수 (IPointerClickHandler 덕분)
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // 슬롯이 비어있지 않은지 먼저 확인
        if (currentStack == null || currentStack.item == null) return;

        // 왼쪽 클릭: 아이템 사용 (레시피 북 등)
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 클릭 사운드 재생
            if (inventoryUI != null && inventoryUI.slotClickSound != null)
            {
                AudioSource.PlayClipAtPoint(inventoryUI.slotClickSound, transform.position, inventoryUI.slotClickVolume);
            }

            // InventoryUI에 '나(이 슬롯) 왼쪽 클릭됐어!' 라고 알린다.
            inventoryUI.OnSlotClicked(this);
        }
        // 오른쪽 클릭: 아이템 버리기
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // InventoryUI에 '나(이 슬롯) 오른쪽 클릭됐어!' 라고 알린다.
            inventoryUI.OnSlotRightClicked(this);
        }
    }
}