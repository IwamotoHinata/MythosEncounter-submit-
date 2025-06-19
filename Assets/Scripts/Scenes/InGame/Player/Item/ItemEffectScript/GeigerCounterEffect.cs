using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

namespace Scenes.Ingame.Player
{
    public class GeigerCounterEffect : ItemEffect
    {
        public override void OnPickUp()
        {
            //��ʏ�ɕ��ː�������\������
            SoundManager.Instance.PlaySe("se_geigercounter00", transform.position);
            ownerPlayerItem.RPC_ActiveGeigerCounter(true);
            //ownerPlayerItem.RPC_UseGeigerCounter(false);
            if (ownerPlayerItem.SwitchGeigerCounter[ownerPlayerItem.nowIndex] == true)// �d����on�ł��鎞
            {
                ownerPlayerItem.RPC_UseGeigerCounter(true);
            }

            //�I���A�C�e����ʂ̂��̂ɂ����Ƃ��A�����ő���@���\���ɂ���
            ownerPlayerItem.OnNowIndexChange
                .Skip(1)
                .Where(x => ownerPlayerItem.ItemSlots[ownerPlayerItem.nowIndex].myItemData == null || ownerPlayerItem.ItemSlots[ownerPlayerItem.nowIndex].myItemData.itemID != 19)
                .Subscribe(_ =>
                {
                    ownerPlayerItem.RPC_ActiveGeigerCounter(false);
                }).AddTo(this);
        }

        public override void OnThrow()
        {
            //�p�����ɑ������\���ɂ���
            ownerPlayerItem.RPC_ChangeSwitchGeigerCounter(false);
            ownerPlayerItem.RPC_ActiveGeigerCounter(false);
        }

        public override void Effect()
        {
            if (ownerPlayerItem.SwitchGeigerCounter[ownerPlayerItem.nowIndex] == true)// ����킪���蒆�̏ꍇ
            {
                ownerPlayerItem.RPC_UseGeigerCounter(false);
                ownerPlayerItem.RPC_ChangeSwitchGeigerCounter(false);
            }
            else
            {
                ownerPlayerItem.RPC_UseGeigerCounter(true);
                ownerPlayerItem.RPC_ChangeSwitchGeigerCounter(true);
            }
        }

    }
}


