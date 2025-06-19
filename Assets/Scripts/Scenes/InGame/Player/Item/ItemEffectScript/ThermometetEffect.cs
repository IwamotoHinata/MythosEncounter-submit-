using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

namespace Scenes.Ingame.Player
{
    public class ThermoeterEffect : ItemEffect
    {
        public override void OnPickUp()
        {
            //��ʏ�ɉ��x�v��\������
            ownerPlayerItem.RPC_ActiveThermometer(true);

            //�I���A�C�e����ʂ̂��̂ɂ����Ƃ��A�����ŉ��x�v���\���ɂ���
            ownerPlayerItem.OnNowIndexChange
            .Skip(1)
            .Where(x => ownerPlayerItem.ItemSlots[ownerPlayerItem.nowIndex].myItemData == null || ownerPlayerItem.ItemSlots[ownerPlayerItem.nowIndex].myItemData.itemID != 20)
            .Subscribe(_ =>
            {
                ownerPlayerItem.RPC_ActiveThermometer(false);
            }).AddTo(this);
        }

        public override void OnThrow()
        {
            //�p�����ɉ��x�v���\���ɂ���
            ownerPlayerItem.RPC_ActiveThermometer(false);
        }

        public override void Effect()
        {
            SoundManager.Instance.PlaySe("se_switch00", transform.position);
            ownerPlayerItem.RPC_UseThermometer();    
            ownerPlayerItem.ChangeCanUseItem(false);                   
        }

    }
}


