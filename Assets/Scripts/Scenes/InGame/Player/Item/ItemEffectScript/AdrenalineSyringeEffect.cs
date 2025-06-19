using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Scenes.Ingame.Player
{
    public class AdrenalineSyringeEffect : ItemEffect
    {
        [SerializeField] private float _animationTime;
        CancellationTokenSource source = new CancellationTokenSource();

        public override void OnPickUp()
        {
            //攻撃くらったときを示すBoolがTrueになったときにアイテム使用を中断
            ownerPlayerStatus.OnEnemyAttackedMe
                .Subscribe(_ =>
                { 
                    source?.Cancel();
                    source?.Dispose();
                }).AddTo(this);
        }

        public override void OnThrow()
        {

        }

        public override void Effect()
        {
            SyringeEffect().Forget();
        }
        
        public async UniTaskVoid SyringeEffect()
        {
            var token = ownerPlayerStatus.GetCancellationTokenOnDestroy();
            SoundManager.Instance.PlaySe("se_adrenaline00", transform.position);

            //アイテム使用直後にステータス変更を行う
            ownerPlayerStatus.UseItem(true);
            ownerPlayerStatus.ChangeSpeed();
            ownerPlayerItem.ChangeCanChangeBringItem(false);
            ownerPlayerItem.ChangeCanUseItem(false);
            //ToDo:Playerに包帯を巻くエフェクトを発生させる

            await UniTask.WaitForSeconds(3f, cancellationToken: token);
            ownerPlayerStatus.SetStaminaBuff(true);//アドレナリン状態を変化させるためのコマンド
            ownerPlayerStatus.UseItem(false);
            ownerPlayerStatus.ChangeSpeed();
            ownerPlayerItem.ChangeCanChangeBringItem(true);
            ownerPlayerItem.ChangeCanUseItem(true);
            ownerPlayerItem.ConsumeItem(ownerPlayerItem.nowIndex);

            await UniTask.WaitForSeconds(15f, cancellationToken: token);
            if (!token.IsCancellationRequested)
            {
                ownerPlayerStatus.SetStaminaBuff(false);//アドレナリン状態を変化させるためのコマンド
            }
        }
    }
}

