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
    /// 인벤토리에서 특정 아이템을 1개 제거하는 함수.
    /// </summary>
    public void RemoveItem(ItemData itemToRemove)
    {
        // 제거할 아이템을 가진 스택을 찾는다.
        int index = _stacks.FindIndex(s => s.item == itemToRemove);

        if (index > -1)
        {
            // 아이템을 찾았으면 개수를 1 줄인다.
            _stacks[index].quantity--;
            
            // 만약 개수가 0이 되면, 리스트에서 스택 자체를 제거한다.
            if (_stacks[index].quantity <= 0)
            {
                _stacks.RemoveAt(index);
            }
            
            Debug.Log(itemToRemove.itemName + "을(를) 인벤토리에서 제거했습니다.");
            
            // 인벤토리가 변경되었다고 모두에게 알림!
            onInventoryChanged?.Invoke();
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
