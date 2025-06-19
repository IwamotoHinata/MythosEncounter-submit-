using UnityEngine;
using Fusion;
/// <summary>
/// �A�C�e���X���b�g���g�p�ł��邩�ۂ�
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
    /// �\���̂ɂ���ϐ���ύX����֐�.�������������Ƃ��͈����Ȃ���OK
    /// </summary>
    /// <param name="data">a�A�C�e���̃f�[�^</param>
    /// <param name="status">�A�C�e���X���b�g���g�p�ł���̂�������</param>
    /// <param name="obj">�A�C�e���̃Q�[���I�u�W�F�N�g</param>
    public void ChangeInfo(ItemData data = null, ItemSlotStatus status = ItemSlotStatus.available)
    { 
        this.myItemData = data;
        this.myItemSlotStatus = status;
    }
}
