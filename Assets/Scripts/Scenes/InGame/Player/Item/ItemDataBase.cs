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
            Debug.LogError("�w�肳�ꂽItemID�͑z��O�̒l�ł�");
            return null;
        }

        if (itemDatas[itemID] == null)
        {
            Debug.LogError("�v�f������܂���");
            return null;
        }

        return itemDatas[itemID];
    }
}
                