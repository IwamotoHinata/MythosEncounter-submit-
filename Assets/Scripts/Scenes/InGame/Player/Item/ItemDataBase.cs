using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataBase", menuName = "ScriptableObjects/ItemDataBase")]
public class ItemDataBase : ScriptableObject
{
    public List<ItemData> itemDatas = new List<ItemData>();

    public ItemData GetItemData(int itemID)
    {
        if (itemID < 0 || itemID >= itemDatas.Count)
        {
            Debug.LogError("指定されたItemIDは想定外の値です");
            return null;
        }

        if (itemDatas[itemID] == null)
        {
            Debug.LogError("要素がありません");
            return null;
        }

        return itemDatas[itemID];
    }
}
                