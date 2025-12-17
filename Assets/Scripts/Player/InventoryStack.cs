/// <summary>
/// 인벤토리 내의 한 아이템 스택을 나타내는 클래스.
/// 어떤 아이템(ItemData)이 몇 개(quantity) 있는지 저장한다.
/// </summary>
[System.Serializable]
public class InventoryStack
{
    public ItemData item;
    public int quantity;

    public InventoryStack(ItemData item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}
