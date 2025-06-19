using UnityEngine;
using Cysharp.Threading.Tasks;
using Fusion;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// �����̎�ނ�1��
    /// 1.�v���C���[��5�b�Ԑ����グ�Ĕ�������B���̂Ƃ����������A�j���[�V�����Ɉڍs
    /// 2.��؂̍s�����s��
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
            //�������͍s���s�\�ɂȂ�
            _myMultiPlayerMove.MoveControl(false);
            _myPlayerItem.ChangeCanUseItem(false);
            _myPlayerItem.ChangeCanChangeBringItem(false);

            //���ѐ����グ��
            _myPlayerSoundManager.RPC_SetScreamClip("Male");

            //���������A�j���[�V�����ɑJ��
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
        /// ���Ԃ��Ƃ��I���΍s���s�\������
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
