using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Scenes.Ingame.Player
{
    public class CompassEffect : ItemEffect
    {

        public override void OnPickUp()
        {
            //��ʏ�ɃR���p�X��\������
            ownerPlayerItem.RPC_ActiveCompass(true);

            //�I���A�C�e����ʂ̂��̂ɂ����Ƃ��A�����ŃR���p�X���\���ɂ���
            ownerPlayerItem.OnNowIndexChange
            .Skip(1)
            .Where(x => ownerPlayerItem.ItemSlots[ownerPlayerItem.nowIndex].myItemData == null || ownerPlayerItem.ItemSlots[ownerPlayerItem.nowIndex].myItemData.itemID != 4)
            .Subscribe(_ =>
            {
                ownerPlayerItem.RPC_ActiveCompass(false);
            }).AddTo(this);
        }

        public override void OnThrow()
        {
            //�p�����ɃR���p�X���\���ɂ���
            ownerPlayerItem.RPC_ActiveCompass(false);
        }

        public override void Effect()
        {

        }


    }
}


