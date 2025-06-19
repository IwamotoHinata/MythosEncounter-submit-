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
            //�U����������Ƃ�������Bool��True�ɂȂ����Ƃ��ɃA�C�e���g�p�𒆒f
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

            //�A�C�e���g�p����ɃX�e�[�^�X�ύX���s��
            ownerPlayerStatus.UseItem(true);
            ownerPlayerStatus.ChangeSpeed();
            ownerPlayerItem.ChangeCanChangeBringItem(false);
            ownerPlayerItem.ChangeCanUseItem(false);
            //ToDo:Player�ɕ�т������G�t�F�N�g�𔭐�������

            await UniTask.WaitForSeconds(3f, cancellationToken: token);
            ownerPlayerStatus.SetStaminaBuff(true);//�A�h���i������Ԃ�ω������邽�߂̃R�}���h
            ownerPlayerStatus.UseItem(false);
            ownerPlayerStatus.ChangeSpeed();
            ownerPlayerItem.ChangeCanChangeBringItem(true);
            ownerPlayerItem.ChangeCanUseItem(true);
            ownerPlayerItem.ConsumeItem(ownerPlayerItem.nowIndex);

            await UniTask.WaitForSeconds(15f, cancellationToken: token);
            if (!token.IsCancellationRequested)
            {
                ownerPlayerStatus.SetStaminaBuff(false);//�A�h���i������Ԃ�ω������邽�߂̃R�}���h
            }
        }
    }
}

