using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Scenes.Ingame.Player
{
    public class TentacleEffect : ItemEffect
    {
        public override void OnPickUp()
        {

        }

        public override void OnThrow()
        {

        }

        public override void Effect()
        {
            TantacleEffect().Forget();
            ownerPlayerStatus.ChangeBleedingBool(false);//���݂���o�����~�܂�
            ownerPlayerItem.ConsumeItem(ownerPlayerItem.nowIndex);//�A�C�e���𖳂���
        }

        private async UniTaskVoid TantacleEffect()
        {
            var token = ownerPlayerStatus.GetCancellationTokenOnDestroy();
            SoundManager.Instance.PlaySe("se_tentacle00", transform.position);
            while (true)
            {
                Debug.Log("Tentacle");
                ownerPlayerStatus.SetStaminaBuff(true);//�X�^�~�i�����ʂ�50%����
                ownerPlayerStatus.SetStaminaHealBuff(true);//�X�^�~�i�񕜗ʂ�50%�㏸
                ownerPlayerStatus.ChangeHealth(1, ChangeValueMode.Heal);//�̗͂����b1��
                ownerPlayerStatus.ChangeSanValue(1, ChangeValueMode.Damage);//SAN�l�����b1����
                await UniTask.WaitForSeconds(1f, cancellationToken: token);
                Debug.Log("Tentacle2");
            }
        }


    }
}