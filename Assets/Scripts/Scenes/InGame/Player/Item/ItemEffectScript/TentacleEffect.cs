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
            ownerPlayerStatus.ChangeBleedingBool(false);//»έ ιoͺ~άι
            ownerPlayerItem.ConsumeItem(ownerPlayerItem.nowIndex);//ACeπ³­·
        }

        private async UniTaskVoid TantacleEffect()
        {
            var token = ownerPlayerStatus.GetCancellationTokenOnDestroy();
            SoundManager.Instance.PlaySe("se_tentacle00", transform.position);
            while (true)
            {
                Debug.Log("Tentacle");
                ownerPlayerStatus.SetStaminaBuff(true);//X^~iΈ­Κͺ50%Έ­
                ownerPlayerStatus.SetStaminaHealBuff(true);//X^~iρΚͺ50%γΈ
                ownerPlayerStatus.ChangeHealth(1, ChangeValueMode.Heal);//ΜΝͺb1ρ
                ownerPlayerStatus.ChangeSanValue(1, ChangeValueMode.Damage);//SANlͺb1Έ­
                await UniTask.WaitForSeconds(1f, cancellationToken: token);
                Debug.Log("Tentacle2");
            }
        }


    }
}