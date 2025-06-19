using UnityEngine;
using Fusion;
/// <summary>
/// アイテムスロットが使用できるか否か
/// </summary>
public enum ItemSlotStatus
{ 
    available,
    unavailable
}

public struct ItemSlotStruct
{
    public ItemData myItemData;
    public ItemSlotStatus myItemSlotStatus;

    public ItemSlotStruct(ItemData data, ItemSlotStatus status)
    {
        this.myItemData = data;
        this.myItemSlotStatus = status;
    }

    /// <summary>
    /// 構造体にある変数を変更する関数.初期化したいときは引数なしでOK
    /// </summary>
    /// <param name="data">aアイテムのデータ</param>
    /// <param name="status">アイテムスロットが使用できるのかを決定</param>
    /// <param name="obj">アイテムのゲームオブジェクト</param>
    public void ChangeInfo(ItemData data = null, ItemSlotStatus status = ItemSlotStatus.available)
    { 
        this.myItemData = data;
        this.myItemSlotStatus = status;
    }
}
