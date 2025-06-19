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
            ownerPlayerStatus.ChangeBleedingBool(false);//現在ある出血が止まる
            ownerPlayerItem.ConsumeItem(ownerPlayerItem.nowIndex);//アイテムを無くす
        }

        private async UniTaskVoid TantacleEffect()
        {
            var token = ownerPlayerStatus.GetCancellationTokenOnDestroy();
            SoundManager.Instance.PlaySe("se_tentacle00", transform.position);
            while (true)
            {
                Debug.Log("Tentacle");
                ownerPlayerStatus.SetStaminaBuff(true);//スタミナ減少量が50%減少
                ownerPlayerStatus.SetStaminaHealBuff(true);//スタミナ回復量が50%上昇
                ownerPlayerStatus.ChangeHealth(1, ChangeValueMode.Heal);//体力が毎秒1回復
                ownerPlayerStatus.ChangeSanValue(1, ChangeValueMode.Damage);//SAN値が毎秒1減少
                await UniTask.WaitForSeconds(1f, cancellationToken: token);
                Debug.Log("Tentacle2");
            }
        }


    }
}