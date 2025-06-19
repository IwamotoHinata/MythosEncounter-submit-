using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Scenes.Ingame.Player
{
    public class BandageEffect : ItemEffect
    {
        private float _startTime;
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
            if (ownerPlayerStatus.nowBleedingValue)
                StartCoroutine(UseBandage());
            else
                Debug.Log("�o����Ԃł͂Ȃ��̂Ŏg�p���܂���ł����B");
        }

        public IEnumerator UseBandage()
        {
            Debug.Log("��юg��");
            SoundManager.Instance.PlaySe("se_bandage00", transform.position);

            //�A�C�e���g�p����ɃX�e�[�^�X�ύX���s��
            ownerPlayerStatus.UseItem(true);
            ownerPlayerStatus.ChangeSpeed();
            ownerPlayerItem.ChangeCanChangeBringItem(false);
            //ToDo:Player�ɕ�т������G�t�F�N�g�𔭐�������

            _startTime = Time.time;
            Debug.Log(_startTime);

            while (true)
            {
                yield return null;
                //�U����H������ۂɂ��̃R���[�`����j��              
                if (_stopCoroutineBool == true)
                {
                    Debug.Log("��юg�p�̃R���[�`����j��");
                    _stopCoroutineBool = false;
                    ownerPlayerStatus.UseItem(false);
                    ownerPlayerStatus.ChangeSpeed();
                    ownerPlayerItem.ChangeCanChangeBringItem(true);
                    yield break;
                }

                if (Time.time - _startTime >= 5.0f)
                {
                    ownerPlayerStatus.ChangeBleedingBool(false);
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

