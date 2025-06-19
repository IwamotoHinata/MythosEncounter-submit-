using Scenes.Ingame.Player;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("アイテムの固有値")]
    public int itemID;
    public string itemName;
    public Sprite itemSprite;

    [Header("ショップ関連の情報")]
    public int price;
    public string itemInfo;
    public ItemType type;

    [Header("インゲームの設定")]
    public string useMethod;
    [Tooltip("使い切りのアイテムならtrueに")]
    public bool isSingleUse;
    [Tooltip("このアイテムのプレハブ")] public GameObject prefab;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="itemName"></param>
    /// <param name="spritePath"></param>
    /// <param name="prace"></param>
    /// <param name="itemInfo"></param>
    /// <param name="type"></param>
    /// <param name="isSingleUse"></param>
    /// <param name="prefabPath"></param>
    public ItemData(int itemID, string itemName, string spritePath, int prace, string itemInfo, ItemType type, bool isSingleUse, string prefabPath,string useMethod)
    {
        this.itemID = itemID;
        this.itemName = itemName;
        this.itemSprite = (Sprite)Resources.Load(spritePath);
        this.price = prace;
        this.itemInfo = itemInfo;
        this.type = type;
        this.isSingleUse = isSingleUse;
        this.prefab = (GameObject)Resources.Load(prefabPath);
        this.useMethod = useMethod;
    }
}

