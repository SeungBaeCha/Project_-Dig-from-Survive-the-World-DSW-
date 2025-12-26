/// <summary>
/// ?몃깽?좊━ ?댁쓽 ???꾩씠???ㅽ깮???섑??대뒗 ?대옒??
/// ?대뼡 ?꾩씠??ItemData)??紐?媛?quantity) ?덈뒗吏 ??ν븳??
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




