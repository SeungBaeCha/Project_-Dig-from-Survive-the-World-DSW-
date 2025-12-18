using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 조합 창 UI를 관리하는 스크립트.
/// C 키를 눌러 창을 열고 닫는 역할과 조합 로직 실행을 담당한다.
/// </summary>
public class CraftingWindow : MonoBehaviour
{
    [Header("UI 오브젝트")]
    [Tooltip("활성화/비활성화 할 조합 창 Panel 오브젝트")]
    [SerializeField] private GameObject craftingWindowPanel;

    [Header("슬롯")]
    [Tooltip("재료를 넣는 슬롯들")]
    [SerializeField] private List<CraftingSlot> materialSlots;
    [Tooltip("결과물이 나오는 슬롯")]
    [SerializeField] private CraftingSlot resultSlot;

    [Header("시스템")]
    [Tooltip("모든 레시피 정보를 가지고 있는 CraftingSystem")]
    [SerializeField] private CraftingSystem craftingSystem;
    [Tooltip("플레이어의 인벤토리 데이터")]
    [SerializeField] private Inventory playerInventory;

    [Header("조합 버튼")]
    [SerializeField] private Button craftButton;

    private bool isOpen = false;

    void Start()
    {
        // 모든 슬롯들에게 내가 부모(CraftingWindow)라고 알려준다.
        foreach (var slot in materialSlots)
        {
            slot.Initialize(this);
        }
        resultSlot.Initialize(this);

        if (craftingWindowPanel != null)
        {
            craftingWindowPanel.SetActive(false);
            isOpen = false;
        }
        else
        {
            Debug.LogError("CraftingWindowPanel이 할당되지 않았습니다!");
        }

        if (craftButton != null)
        {
            craftButton.onClick.AddListener(CraftItem);
        }
    }

    public void ToggleWindow()
    {
        isOpen = !isOpen;
        craftingWindowPanel.SetActive(isOpen);
        
        // 창을 닫을 때 슬롯에 있던 아이템들을 모두 인벤토리로 돌려준다.
        if (!isOpen)
        {
            ClearAllSlots();
        }
    }
    
    /// <summary>
    /// 인벤토리에서 클릭한 아이템을 재료 슬롯에 추가한다.
    /// </summary>
    /// <returns>추가에 성공하면 true</returns>
    public bool AddMaterial(ItemData material)
    {
        // 비어있는 재료 슬롯을 찾는다.
        foreach (var slot in materialSlots)
        {
            if (slot.currentItem == null)
            {
                slot.SetItem(material);
                return true; // 추가 성공
            }
        }
        return false; // 모든 슬롯이 꽉 찼다.
    }

    /// <summary>
    /// 조합 슬롯(재료 또는 결과)이 클릭됐을 때 호출된다.
    /// </summary>
    public void OnCraftingSlotClicked(CraftingSlot clickedSlot)
    {
        // 클릭한 슬롯의 아이템을 인벤토리에 돌려주고, 슬롯을 비운다.
        playerInventory.AddItem(clickedSlot.currentItem);
        clickedSlot.ClearSlot();
        CheckForValidRecipe(); // 아이템을 뺐으니 레시피를 다시 확인
    }

    /// <summary>
    /// 조합 버튼을 눌렀을 때 실행될 함수.
    /// </summary>
    private void CraftItem()
    {
        // 결과물 슬롯에 아이템이 없으면 조합 불가
        if (resultSlot.currentItem == null)
        {
            Debug.Log("조합할 아이템이 없습니다.");
            return;
        }
        
        // 재료 슬롯들을 비운다. (재료 소모)
        foreach (var slot in materialSlots)
        {
            slot.ClearSlot();
        }
        
        // 결과물 슬롯에 있던 아이템을 인벤토리에 추가한다.
        playerInventory.AddItem(resultSlot.currentItem);
        
        // 결과물 슬롯을 비운다.
        resultSlot.ClearSlot();
    }

    /// <summary>
    /// 현재 재료 슬롯의 아이템들을 바탕으로 유효한 레시피가 있는지 확인하고, 결과 슬롯에 보여준다.
    /// </summary>
    private void CheckForValidRecipe()
    {
        List<ItemData> currentMaterials = new List<ItemData>();
        foreach (var slot in materialSlots)
        {
            if (slot.currentItem != null)
            {
                currentMaterials.Add(slot.currentItem);
            }
        }

        CraftingRecipe recipe = craftingSystem.FindRecipe(currentMaterials);

        // 레시피를 찾았고, '발견된' 레시피일 경우에만 결과 슬롯에 아이템을 표시한다.
        if (recipe != null && recipe.isDiscovered)
        {
            resultSlot.SetItem(recipe.result);
        }
        else
        {
            resultSlot.ClearSlot();
        }
    }
    
    /// <summary>
    /// 모든 슬롯(재료, 결과)의 아이템을 인벤토리로 되돌리고 비운다.
    /// </summary>
    private void ClearAllSlots()
    {
        foreach(var slot in materialSlots)
        {
            if(slot.currentItem != null)
            {
                playerInventory.AddItem(slot.currentItem);
                slot.ClearSlot();
            }
        }
        if(resultSlot.currentItem != null)
        {
            // 조합중이던 결과물은 그냥 파기하거나, 인벤토리에 돌려주거나 정책을 정해야 함. 여기선 파기.
            resultSlot.ClearSlot();
        }
    }
}
