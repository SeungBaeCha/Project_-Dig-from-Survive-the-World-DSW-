using System.Collections.Generic;
using UnityEngine;
using System; // Action을 사용하기 위해 필요

/// <summary>
/// 플레이어의 인벤토리를 관리하는 스크립트.
/// 아이템 데이터를 리스트 형태로 저장한다.
/// </summary>
public class Inventory : MonoBehaviour
{
    // 인벤토리 내용이 변경되었을 때 다른 스크립트에게 알려주기 위한 이벤트
    public event Action onInventoryChanged;

    [SerializeField]
    private List<ItemData> items = new List<ItemData>();

    /// <summary>
    /// 인벤토리에 아이템을 추가하는 함수.
    /// </summary>
    public void AddItem(ItemData itemToAdd)
    {
        items.Add(itemToAdd);
        Debug.Log(itemToAdd.itemName + "을(를) 인벤토리에 추가했습니다. 현재 아이템 수: " + items.Count);
        
        // 인벤토리가 변경되었다고 모두에게 알림!
        onInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 인벤토리에서 특정 아이템을 제거하는 함수.
    /// </summary>
    public void RemoveItem(ItemData itemToRemove)
    {
        if (items.Remove(itemToRemove))
        {
            Debug.Log(itemToRemove.itemName + "을(를) 인벤토리에서 제거했습니다. 현재 아이템 수: " + items.Count);
            
            // 인벤토리가 변경되었다고 모두에게 알림!
            onInventoryChanged?.Invoke();
        }
    }
    
    /// <summary>
    /// 현재 인벤토리에 있는 모든 아이템 목록을 (복사해서) 반환한다.
    /// </summary>
    public List<ItemData> GetItems()
    {
        // 원본 리스트를 직접 주지 않고, 복사본을 만들어서 주면
        // 다른 스크립트에서 실수로 인벤토리를 망가뜨리는 것을 방지할 수 있어.
        return new List<ItemData>(items);
    }
}
