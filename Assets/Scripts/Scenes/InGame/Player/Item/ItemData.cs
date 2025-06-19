using Scenes.Ingame.Player;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("�A�C�e���̌ŗL�l")]
    public int itemID;
    public string itemName;
    public Sprite itemSprite;

    [Header("�V���b�v�֘A�̏��")]
    public int price;
    public string itemInfo;
    public ItemType type;

    [Header("�C���Q�[���̐ݒ�")]
    public string useMethod;
    [Tooltip("�g���؂�̃A�C�e���Ȃ�true��")]
    public bool isSingleUse;
    [Tooltip("���̃A�C�e���̃v���n�u")] public GameObject prefab;

    /// <summary>
    /// �R���X�g���N�^
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

