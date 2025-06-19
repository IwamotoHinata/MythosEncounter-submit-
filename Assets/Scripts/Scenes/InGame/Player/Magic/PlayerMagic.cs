using UnityEngine;
using UniRx;
using System;
using Fusion;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// �v���C���[�̖��@�֘A���Ǘ�����X�N���v�g
    /// </summary>
    public class PlayerMagic : NetworkBehaviour
    {
        private bool _isCanUseMagic = true;//���ݖ��@���g���邩�ۂ�
        private bool _isUsedMagic = false;//���@��1�x�g�������ۂ�
        [SerializeField] private Magic _myMagic;//�g�p�\�Ȗ��@
        [SerializeField] private PlayerSoundManager _playerSoundManager;//�g�p�\�Ȗ��@

        private Subject<Unit> _FinishUseMagic = new Subject<Unit>();//���@�̉r�����I���A���ʂ�����������C�x���g������.
        public IObserver<Unit> OnPlayerFinishUseMagic { get { return _FinishUseMagic; } }//�O����_FinishUseMagic��OnNext���ĂԂ��߂�IObserver�����J

        PlayerStatus _myPlayerStatus;
        private TickTimer _coolTimer = TickTimer.None;

        public override void Spawned()
        {
            //���g��PlayerStatus���擾
            _myPlayerStatus = this.GetComponent<PlayerStatus>();

            //_myMagic�̒��g�����g���ݒ肵�������ɐݒ肷�鏈��
            //���łł͖���(�C���Q�[���O���������ꂽ�����)
            //RPC_AddMagicScript();

            //�����X�N���v�g��PlayerStatus��PlayerMagic���擾������
            _myMagic.myPlayerStatus = _myPlayerStatus;
            _myMagic.myPlayerMagic = this;

            if (HasStateAuthority)
            {
                //�U����������Ƃ��̃C�x���g�����s���ꂽ�Ƃ��Ɏ����r���𒆒f(�g�p���̂�)
                _myPlayerStatus.OnEnemyAttackedMe
                    .Where(_ => _isCanUseMagic && _myPlayerStatus.nowPlayerUseMagic)
                    .Subscribe(_ =>
                    {
                        //�r�����̈ړ����x50%Down������
                        _myPlayerStatus.UseMagic(false);
                        _myPlayerStatus.ChangeSpeed();

                        //���@���g���������L�����Z��
                        _myMagic.cancelMagic = true;
                        Debug.Log("�U�����󂯂��̂ŉr�����~�I");
                    }).AddTo(this);


                //�����̉r�����I�������瑫�̒x�������ɖ߂��B
                _FinishUseMagic
                    .Subscribe(_ =>
                    {
                        //�r�����̈ړ����x50%Down������
                        _myPlayerStatus.UseMagic(false);
                        _myPlayerStatus.ChangeSpeed();
                        _isUsedMagic = true;
                    }).AddTo(this);
            }

        }

        public override void FixedUpdateNetwork()
        {
            /* --- �T�[�o(�z�X�g)���݂̂̏����� ---*/
            if (HasStateAuthority)
            {
                //���͂Ɋ�Â������������s
                var input = GetInput<GameplayInput>();
                ProcessInput(input.GetValueOrDefault());

            }
        }

        private void ProcessInput(GameplayInput input)
        {
            //�������g�p�E���f���鏈��
            if (input.Buttons.IsSet(EInputButton.UseMagicOrStopMagic) && _isCanUseMagic)
            {
                if (_myPlayerStatus.nowPlayerUseMagic && _coolTimer.Expired(Runner))//�������r�����n�߂�1�b�o���Ă��� // && _coolTimer.Expired(Runner)
                {
                    //�r�����̈ړ����x50%Down������
                    _myPlayerStatus.UseMagic(false);
                    _myPlayerStatus.ChangeSpeed();

                    //���@���g���������L�����Z��
                    _myMagic.cancelMagic = true;
                    Debug.Log("����ɂ��r�����~");

                    //PlayerUI�̕��Ŏ����̉r�����Ԃ�\�����I��
                    RPC_CastEventCall(false);

                    //������
                    _coolTimer = TickTimer.CreateFromSeconds(Runner, 1.0f);
                }
                else if (!_myPlayerStatus.nowPlayerUseMagic && _coolTimer.ExpiredOrNotRunning(Runner))//�������܂��r�����Ă��Ȃ��Ƃ�
                {
                    //San�l��10�ȉ��̂Ƃ��͉r���ł��Ȃ�
                    if (_myPlayerStatus.nowPlayerSanValue <= _myMagic.consumeSanValue)
                    {
                        Debug.Log("SAN�l������Ȃ��̂ŉr���ł��܂���");
                        return;
                    }

                    //�e�����ňꕔ�g�p���Ȃ��ėǂ��󋵂ł���Ύ������g�킹�Ȃ�
                    bool needMagic = true;//�������g���K�v�����邩�ۂ�
                    switch (_myMagic)
                    {
                        case SelfBrainwashMagic:
                            if (_myPlayerStatus.nowPlayerSanValue > 50)
                            {
                                needMagic = false;
                                Debug.Log("�������Ă��Ȃ��̂Ŏ������g���K�v������܂���");
                            }
                            break;
                        case RecoverMagic:
                            if (_myPlayerStatus.nowPlayerHealth == _myPlayerStatus.health_max)
                            {
                                needMagic = false;
                                Debug.Log("�̗͌����Ă��Ȃ��̂Ŏ������g���K�v������܂���");
                            }
                            break;
                        default:
                            break;
                    }

                    if (needMagic)
                    {
                        //�r�����͈ړ����x50%Down
                        _myPlayerStatus.UseMagic(true);
                        _myPlayerStatus.ChangeSpeed();

                        //���@���g������
                        _myMagic.MagicEffect();
                        Debug.Log("�����̉r���J�n");

                        //PlayerUI�̕��Ŏ����̉r�����Ԃ�\��������
                        RPC_CastEventCall(true);

                        //SE��炷
                        _playerSoundManager.PlayEffectClip(EffectClip.Cast);

                        //�L�����Z���p
                        _coolTimer = TickTimer.CreateFromSeconds(Runner, 1.0f);
                    }
                }
            }

        }

        public void ChangeCanUseMagicBool(bool value)
        {
            _isCanUseMagic = value;
        }

        /// <summary>
        /// ���ɂP�x�������g���������Ǘ����Ă���Bool�̒l���擾����֐�
        /// </summary>
        /// <returns>_isUsedMagic�̒l</returns>
        public bool GetUsedMagicBool()
        {
            return _isUsedMagic;
        }

        /// <summary>
        /// RPC��PlayerUI�̃L���X�g�Q�[�W��\���E��\��
        /// </summary>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
        public void RPC_CastEventCall(bool isView)
        {
            if (isView)
                _myPlayerStatus.OnCastEventCall.OnNext(_myMagic.chantTime);
            else
                _myPlayerStatus.OnCancelCastEventCall.OnNext(default);

        }

        /// <summary>
        /// RPC��magic�X�N���v�g���A�^�b�`
        /// </summary>
        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
        public void RPC_AddMagicScript()
        {
            //Todo:���g�Őݒ肵��magic�X�N���v�g���A�^�b�`���鏈��

            //�����X�N���v�g��PlayerStatus��PlayerMagic���擾������
            _myMagic.myPlayerStatus = _myPlayerStatus;
            _myMagic.myPlayerMagic = this;
        }
    }
}

