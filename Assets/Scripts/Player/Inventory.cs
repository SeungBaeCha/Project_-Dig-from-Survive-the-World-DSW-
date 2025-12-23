using System.Collections.Generic;
using UnityEngine;
using System; // Action을 사용하기 위해 필요
using System.Linq; // FindIndex 사용을 위해 필요

/// <summary>
/// 플레이어의 인벤토리를 관리하는 스크립트.
/// 아이템을 스택(stack)과 수량(quantity)으로 저장하고, 용량 제한을 가진다.
/// </summary>
public class Inventory : MonoBehaviour
{
    // 인벤토리 내용이 변경되었을 때 다른 스크립트에게 알려주기 위한 이벤트
    public event Action onInventoryChanged;

    [Tooltip("인벤토리의 최대 슬롯 개수 (고유 아이템 종류 수)")]
    [SerializeField] private int _maxCapacity = 24;
    public int MaxCapacity => _maxCapacity;

    // 실제 아이템 데이터를 저장하는 리스트. 이제 아이템과 개수를 함께 저장.
    private List<InventoryStack> _stacks = new List<InventoryStack>();

    /// <summary>
    /// 인벤토리에 아이템을 추가하는 함수.
    /// </summary>
    /// <param name="itemToAdd">추가할 아이템 데이터</param>
    /// <returns>추가에 성공하면 true, 인벤토리가 꽉 찼으면 false</returns>
    public bool AddItem(ItemData itemToAdd)
    {
        // 1. 이미 같은 종류의 아이템이 인벤토리에 있는지 확인
        int index = _stacks.FindIndex(s => s.item == itemToAdd);

        if (index > -1)
        {
            // 2. 있으면 해당 스택의 개수만 늘린다.
            _stacks[index].quantity++;
        }
        else
        {
            // 3. 없다면, 인벤토리가 꽉 찼는지 확인
            if (_stacks.Count >= _maxCapacity)
            {
                Debug.Log("인벤토리가 꽉 찼습니다.");
                return false; // 추가 실패
            }
            
            // 4. 공간이 있으면 새로운 스택을 추가한다.
            _stacks.Add(new InventoryStack(itemToAdd, 1));
        }

        Debug.Log(itemToAdd.itemName + "을(를) 인벤토리에 추가했습니다.");
        
        // 인벤토리가 변경되었다고 모두에게 알림!
        onInventoryChanged?.Invoke();
        return true; // 추가 성공
    }

    /// <summary>
    /// 인벤토리에 있는 아이템을 '사용'하는 함수.
    /// </summary>
    /// <param name="itemToUse">사용할 아이템 데이터</param>
    public void UseItem(ItemData itemToUse)
    {
        // 사용할 아이템이 레시피를 해금하는 종류인지 확인
        if (itemToUse != null && itemToUse.recipeToUnlock != null)
        {
            // 레시피를 '발견' 상태로 변경
            itemToUse.recipeToUnlock.Discover();
            
            // CraftingSystem에 이 레시피를 활성 레시피로 설정하도록 알린다.
            CraftingSystem.Instance.SetActiveRecipe(itemToUse.recipeToUnlock);
            
            // 사용한 레시피 아이템을 인벤토리에서 제거
            RemoveItem(itemToUse);
            
            // 유저가 요청한 메시지 표시
            Debug.Log("조합이 활성화되었습니다!");
        }
        else
        {
            Debug.LogWarning($"{itemToUse.name}은(는) 사용할 수 없는 아이템입니다.");
        }
    }

    /// <summary>
    /// 인벤토리에서 특정 아이템을 지정된 개수만큼 제거하는 함수.
    /// </summary>
    /// <param name="itemToRemove">제거할 아이템</param>
    /// <param name="quantityToRemove">제거할 개수</param>
    public void RemoveItem(ItemData itemToRemove, int quantityToRemove = 1)
    {
        int index = _stacks.FindIndex(s => s.item == itemToRemove);

        if (index > -1)
        {
            _stacks[index].quantity -= quantityToRemove;
            
            if (_stacks[index].quantity <= 0)
            {
                _stacks.RemoveAt(index);
            }
            
            Debug.Log($"{itemToRemove.itemName} {quantityToRemove}개를 인벤토리에서 제거했습니다.");
            onInventoryChanged?.Invoke();
        }
    }

    /// <summary>
    /// 제작에 필요한 재료들이 인벤토리에 충분히 있는지 확인하는 함수.
    /// </summary>
    /// <param name="requiredMaterials">필요한 재료 목록</param>
    /// <returns>재료가 모두 충분하면 true, 아니면 false</returns>
    public bool HasMaterials(List<RequiredMaterial> requiredMaterials)
    {
        foreach (var requiredMaterial in requiredMaterials)
        {
            int index = _stacks.FindIndex(s => s.item == requiredMaterial.item);

            // 재료가 인벤토리에 아예 없거나, 있어도 개수가 부족한 경우
            if (index == -1 || _stacks[index].quantity < requiredMaterial.quantity)
            {
                return false; // 재료 부족
            }
        }
        return true; // 모든 재료가 충분함
    }

    /// <summary>
    /// 제작에 사용된 재료들을 인벤토리에서 제거하는 함수.
    /// </summary>
    /// <param name="requiredMaterials">제거할 재료 목록</param>
    public void RemoveMaterials(List<RequiredMaterial> requiredMaterials)
    {
        foreach (var requiredMaterial in requiredMaterials)
        {
            RemoveItem(requiredMaterial.item, requiredMaterial.quantity);
        }
    }
    
    /// <summary>
    /// 현재 인벤토리에 있는 모든 아이템 스택 목록을 (복사해서) 반환한다.
    /// </summary>
    public List<InventoryStack> GetStacks()
    {
        // 원본 리스트를 직접 주지 않고, 복사본을 만들어서 주면
        // 다른 스크립트에서 실수로 인벤토리를 망가뜨리는 것을 방지할 수 있어.
        return new List<InventoryStack>(_stacks);
    }
}
