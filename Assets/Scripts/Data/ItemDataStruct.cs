public struct ItemDataStruct
{
    private int _itemId;
    private string _name;
    private string _description;
    private ItemCategory _itemCategory;
    private int _price;

    public int ItemId { get => _itemId; }
    public string Name { get => _name; }
    public string Description { get => _description; }
    public ItemCategory ItemCategory { get => _itemCategory; }
    public int Price { get => _price; }
    public void ItemDataSet(int itemId, string name, string description, ItemCategory category, int price)
    {
        _itemId = itemId;
        _name = name;
        _description = description;
        _itemCategory = category;
        _price = price;
    }
}
