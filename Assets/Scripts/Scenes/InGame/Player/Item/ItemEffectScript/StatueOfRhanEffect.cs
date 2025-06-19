using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Scenes.Ingame.Player
{
    public class StatueOfRhanEffect : ItemEffect
    {
        private bool _isHaveStatue = false;
        private static CancellationTokenSource _cts;
        public override void OnPickUp()
        {
            if (_cts == null)
            {
                _cts = new CancellationTokenSource();
                //外部からの停止とplayer破壊時の自己停止を合体
                CancellationToken _linkedToken = CancellationTokenSource.CreateLinkedTokenSource(
                    _cts.Token,
                    ownerPlayerStatus.GetCancellationTokenOnDestroy()
                ).Token;
                StatueEffect(_linkedToken).Forget();
            }

        }

        public override void OnThrow()
        {
            //　アイテムスロット内に石像が残っているかどうか判定
            for (int i = 0 ; i < 7 ; i++) {
                var item = ownerPlayerItem.ItemSlots[i];
                if ( i != ownerPlayerItem.nowIndex && item.myItemData != null && item.myItemData.itemID == 23) 
                {
                     _isHaveStatue = true;
                }
            }

            if (!_isHaveStatue) 
            {
                if(_cts != null)
                {
                    _cts.Cancel();
                    _cts.Dispose();
                    _cts = null;
                }
            }
        }

        public override void Effect()
        {

        }

        private async UniTask StatueEffect(CancellationToken token)
        {
            int _count = 0;

            while (!token.IsCancellationRequested)
            {
                _count++;

                if (_count % 10 == 0)
                {
                    ownerPlayerStatus.ChangeHealth(1, ChangeValueMode.Heal);
                    Debug.Log("HP1回復");
                }

                if (_count >= 100)
                {
                    ownerPlayerStatus.ChangeSanValue(1, ChangeValueMode.Damage);
                    _count = 0;
                    Debug.Log("SAN値ダメージ");
                }

                await UniTask.WaitForSeconds(1f, cancellationToken: token);
            }
            _cts.Dispose();
            _cts = null;

        }
    }

}

