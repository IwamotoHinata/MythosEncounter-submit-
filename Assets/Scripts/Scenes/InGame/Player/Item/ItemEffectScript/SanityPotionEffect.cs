using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Scenes.Ingame.Player
{
    public class SanityPotionEffect : ItemEffect
    {
        private bool _stopCoroutineBool = false;

        public override void OnPickUp()
        {
            //�U����������Ƃ�������Bool��True�ɂȂ����Ƃ��ɃA�C�e���g�p�𒆒f
            ownerPlayerStatus.OnEnemyAttackedMe
                .Subscribe(_ => _stopCoroutineBool = true).AddTo(this);
        }

        public override void OnThrow()
        {

        }

        public override void Effect()
        {
            if(ownerPlayerStatus.nowPlayerSanValue != 100)
            {
                StartCoroutine(UseSanityPotion());
            }
            else
            {
                Debug.Log("SAN�l���ő�Ȃ̂Ŏg�p�s��");
            }
        }

        public IEnumerator UseSanityPotion()
        {
            Debug.Log("SAN�l�񕜖�g��");
            SoundManager.Instance.PlaySe("se_tranquilizer00", transform.position);

            //�A�C�e���g�p����ɃX�e�[�^�X�ύX���s��
            ownerPlayerStatus.UseItem(true);
            ownerPlayerStatus.ChangeSpeed();
            ownerPlayerItem.ChangeCanChangeBringItem(false);

            int count = 0;

            while (true)
            {
                //�U����H������ۂɂ��̃R���[�`����j��              
                if (_stopCoroutineBool == true)
                {
                    Debug.Log("san�񕜖�g�p�̃R���[�`����j��");
                    _stopCoroutineBool = false;
                    ownerPlayerStatus.UseItem(false);
                    ownerPlayerStatus.ChangeSpeed();
                    ownerPlayerItem.ConsumeItem(ownerPlayerItem.nowIndex);
                    ownerPlayerItem.ChangeCanChangeBringItem(true);
                    yield break;
                }

                yield return new WaitForSeconds(0.5f);
                ownerPlayerStatus.ChangeSanValue(1, ChangeValueMode.Heal);
                count++;

                //SAN�l��10�񕜂܂��͍ő�ƂȂ����ꍇ�ɃA�C�e��������
                if (count >= 10 || ownerPlayerStatus.nowPlayerSanValue >= 100)
                {
                    ownerPlayerStatus.UseItem(false);
                    ownerPlayerStatus.ChangeSpeed();
                    ownerPlayerItem.ConsumeItem(ownerPlayerItem.nowIndex);
                    ownerPlayerItem.ChangeCanChangeBringItem(true);
                    yield break;
                }
            }
        }
    }
}

