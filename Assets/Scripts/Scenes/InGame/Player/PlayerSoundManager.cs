using UnityEngine;
using Fusion;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// �v���C���[�̉e���ŏo�鉹���Ǘ�����B
    /// ����,�A�C�e���̉��Ȃ�
    /// </summary>
    public class PlayerSoundManager : NetworkBehaviour
    {
        [SerializeField] private Animator _animator;

        [SerializeField] private AudioSource _audio;
        [SerializeField] private AudioClip[] _footClips;//������Clip
        [SerializeField] private AudioClip[] _screamClips;//�ߖ�Clip
        [SerializeField] private AudioClip[] _itemClips;//�A�C�e����Clip
        [SerializeField] private AudioClip[] _effectClips;//���ʉ���Clip

        private PlayerStatus _myPlayerStatus;
        // Start is called before the first frame update
        void Start()
        {
            TryGetComponent<PlayerStatus>(out _myPlayerStatus);
        }

        public void StopSound()
        {
            _audio.loop = false;
            _audio.Stop();
        }


        /// <summary>
        /// Scream.Cs�ɂāA�l�����Ԏ��̌��ʉ��𔭐�������֐��B
        /// </summary>
        /// <param name="gender">����.�j�FMale , �����FFemale</param>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_SetScreamClip(string gender)
        {
            _audio.volume = 0.5f;

            if (gender == "Male")
            {
                _audio.PlayOneShot(_screamClips[0]);
            }
            else if(gender == "Female")
            {
                _audio.PlayOneShot(_screamClips[1]);
            }
        }

        /*
         �v���C���[�̑����i�A�j���[�V�����C�x���g�j
         */
        private void SneakingFootSound()
        {
            if (_myPlayerStatus.nowPlayerActionState != PlayerActionState.Sneak)
                return;

            _audio.volume = 0.1f;
            _audio.PlayOneShot(_footClips[0]);
            Debug.Log("sneak");
        }

        /// <summary>
        /// ���s���̑�����炷�A�j���[�V�����C�x���g�֐�
        /// </summary>
        /// <param name="footInfo">���̏��ԂƉ���炷�����𖳎����邩�ۂ��i�d���΍�j</param>
        private void WalkingFootSound(string footInfo)
        {
            if (_myPlayerStatus.nowPlayerActionState != PlayerActionState.Walk)
                return;


            _audio.volume = 0.5f;
            //�A�j���[�V�������̃C�x���g�̏��Ԃɉ����đ�����ς���
            //�������E��
            if (footInfo == "LeftFoot")//��
            {
                _audio.PlayOneShot(_footClips[1]);
                //Debug.Log("�����̉��Ȃ��");
                return;
            }
            
            else if (footInfo == "RightFoot")//�E
            {
                _audio.PlayOneShot(_footClips[2]);
                //Debug.Log("�E���̉��Ȃ��");
                return;
            }
            
        }

        private void RunningFootSound(int order)
        {
            if (_myPlayerStatus.nowPlayerActionState != PlayerActionState.Dash)
                return;

            _audio.volume = 0.75f;
            //�A�j���[�V�������̃C�x���g�̏��Ԃɉ����đ�����ς���
            //�������E��
            if (order == 0)//��
            {
                _audio.PlayOneShot(_footClips[3]);
            }
            else//�E
            {
                _audio.PlayOneShot(_footClips[4]);
            }
        }

        /// <summary>
        /// �A�C�e���Ɋւ���N���b�v�𗬂��֐�
        /// </summary>
        /// <param name="clip">�N���b�v�̎��</param>
        /// <param name="volume">����</param>
        public void PlayItemClip(ItemClip clip, float volume = 1.0f)
        {
            _audio.volume = volume;
            _audio.PlayOneShot(_itemClips[(int)clip]);
        }

        /// <summary>
        /// ���ʉ��̃N���b�v�𗬂��֐�
        /// </summary>
        /// <param name="clip">�N���b�v�̎��</param>
        /// <param name="volume">����</param>
        public void PlayEffectClip(EffectClip clip, float volume = 1.0f)
        {
            _audio.volume = volume;
            _audio.PlayOneShot(_effectClips[(int)clip]);
        }
    }
}

