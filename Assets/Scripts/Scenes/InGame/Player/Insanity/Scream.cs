using UnityEngine;
using Cysharp.Threading.Tasks;
using Fusion;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// 発狂の種類の1つ
    /// 1.プレイヤーは5秒間声を上げて発狂する。このとき頭を抱えるアニメーションに移行
    /// 2.一切の行動が不可
    /// </summary>
    public class Scream : MonoBehaviour, IInsanity
    {
        private PlayerSoundManager _myPlayerSoundManager;
        private MultiPlayerMove _myMultiPlayerMove;
        private PlayerItem _myPlayerItem;
        private PlayerInsanityManager _myPlayerInsanityManager;
        private Animator _animator;

        public void Setup()
        {
            _myPlayerSoundManager = GetComponent<PlayerSoundManager>();
            _myMultiPlayerMove = GetComponent<MultiPlayerMove>();
            _myPlayerItem = GetComponent<PlayerItem>();
            _myPlayerInsanityManager = GetComponent<PlayerInsanityManager>();
            TryGetComponent<Animator>(out _animator);
        }

        public void Active()
        {
            //発狂中は行動不能になる
            _myMultiPlayerMove.MoveControl(false);
            _myPlayerItem.ChangeCanUseItem(false);
            _myPlayerItem.ChangeCanChangeBringItem(false);

            //叫び声を上げる
            _myPlayerSoundManager.RPC_SetScreamClip("Male");

            //頭を抱えるアニメーションに遷移
            if (_animator != null)
            {
                _myPlayerInsanityManager.RPC_SetAnimBool("Scream", true);
                ScreamAnimation().Forget();
            }
        }

        public void Hide()
        {

        }

        /// <summary>
        /// 叫ぶことが終われば行動不能を解除
        /// </summary>
        /// <returns></returns>
        public async UniTaskVoid ScreamAnimation()
        {
            var token = _myMultiPlayerMove.GetCancellationTokenOnDestroy();
            await UniTask.WaitForEndOfFrame(_myMultiPlayerMove);

            float animClipLength = _animator.GetCurrentAnimatorStateInfo(0).length;
            await UniTask.WaitForSeconds(animClipLength, cancellationToken: token);

            _myMultiPlayerMove.MoveControl(true);
            _myPlayerItem.ChangeCanUseItem(true);
            _myPlayerItem.ChangeCanChangeBringItem(true);

            _myPlayerInsanityManager.RPC_SetAnimBool("Scream", false);
        }
    }
}
