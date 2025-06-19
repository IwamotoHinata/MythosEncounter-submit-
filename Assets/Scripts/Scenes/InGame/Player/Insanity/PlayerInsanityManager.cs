using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using Fusion;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// �v���C���[�̔����֌W���Ǘ�����X�N���v�g
    /// </summary>
    public class PlayerInsanityManager : NetworkBehaviour
    {
        private ReactiveCollection<IInsanity> _insanities = new ReactiveCollection<IInsanity>(); //���݂̔����X�N���v�g���܂Ƃ߂�List
        public IObservable<CollectionAddEvent<IInsanity>> OnInsanitiesAdd => _insanities.ObserveAdd();//�O����__insanities�̗v�f���ǉ����ꂽ�Ƃ��ɍs��������o�^�ł���悤�ɂ���
        public IObservable<CollectionRemoveEvent<IInsanity>> OnInsanitiesRemove => _insanities.ObserveRemove();//�O����__insanities�̗v�f���폜���ꂽ�Ƃ��ɍs��������o�^�ł���悤�ɂ���
        public List<IInsanity> Insanities { get { return _insanities.ToList(); } }//�O����_insanities�̓��e�����J����

        private List<int> _numbers = Enumerable.Range(0, 5).ToList();//0,1,2,3,4�̃��X�g�𐶐�(�����X�N���v�g���d�����Ȃ��ׂɗp����)
        /*
         �Ή��\
         0.EyeParalyze
         1.BodyParalyze
         2.IncreasePulsation
         3.Scream
         4.Hallucination
         */

        [SerializeField] private BoolReactiveProperty _isBrainwashed = new BoolReactiveProperty(false);//���]�����ۂ�
        public IObservable<bool> OnPlayerBrainwashedChange { get { return _isBrainwashed; } }//���]��Ԃ��ω������ۂɃC�x���g�����s

        private PlayerStatus _myPlayerStatus;
        IInsanity InsanityScript = null;

        private Animator _animator;

        public override void Spawned()
        {
            _myPlayerStatus = GetComponent<PlayerStatus>();
            TryGetComponent<Animator>(out _animator);
            if (!HasStateAuthority)
                return;

            //���݂�SAN�l��50�ȉ�����SAN�l�����������ɔ����X�N���v�g��t�^
            _myPlayerStatus.OnPlayerSanValueChange
                .Where(x => x <= 50 && x < _myPlayerStatus.lastSanValue && _myPlayerStatus.NetworkFinishInit)
                .Subscribe(x =>
                {
                    if (40 < x && x <= 50)
                        AddRandomInsanity(1 - _insanities.Count);

                    else if (30 < x && x <= 40)
                        AddRandomInsanity(2 - _insanities.Count);

                    else if (20 < x && x <= 30)
                        AddRandomInsanity(3 - _insanities.Count);

                    else if (10 < x && x <= 20)
                        AddRandomInsanity(4 - _insanities.Count);

                    else if (0 < x && x <= 10)
                        AddRandomInsanity(5 - _insanities.Count);

                }).AddTo(this);

            _myPlayerStatus.OnPlayerSanValueChange
                .Subscribe(x =>
                {
                    Debug.Log("NowSanValue�F" + x + "\nlastSanValue�F" + _myPlayerStatus.lastSanValue);
                }).AddTo(this);

            //�ύX�O��SAN�l��50�ȉ�����SAN�l���񕜂����Ƃ��ɔ�������
            _myPlayerStatus.OnPlayerSanValueChange
                .Where(x => _myPlayerStatus.lastSanValue <= 50 && x > _myPlayerStatus.lastSanValue && _myPlayerStatus.NetworkFinishInit)
                .Subscribe(x => RecoverInsanity(x / 10 - _myPlayerStatus.lastSanValue / 10))
                .AddTo(this);
        }

        public override void FixedUpdateNetwork()
        {

        }

        /// <summary>
        /// �����_���Ŕ����X�N���v�g��t�^������ 
        /// </summary>
        /// /// <param name="times">�֐���@����</param>
        private void AddRandomInsanity(int times)
        {
            if (times == 0 || !HasStateAuthority)
                return;

            for (int i = 0; i < times; i++)
            {
                int random = _numbers[UnityEngine.Random.Range(0, _numbers.Count)];
                //�C�ӂ�IInsanity�֘A�̃X�N���v�g���A�^�b�`
                InsanityScript = null;
                RPC_AddInsanityScript(random);

                //���]��ԂŖ�����Α����ɔ������ʂ𔭊�
                //�z�X�g���ł̂݌��ʂ𔭓�������B���ʂ��N���C�A���g���ɕԂ�
                if (InsanityScript != null && !_isBrainwashed.Value)
                {
                    //EyeParalyze��Hallucination�ȊO�̔������ʂ̂�Active��
                    if (!InsanityScript.GetType().Equals(typeof(EyeParalyze)) && !InsanityScript.GetType().Equals(typeof(Hallucination)))
                        InsanityScript.Active();
                }
                
            }           
        }

        
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_AddInsanityScript(int number)
        {
            switch (number)
            {
                case 0:
                    InsanityScript = this.AddComponent<EyeParalyze>();
                    _insanities.Add(InsanityScript);
                    break;
                case 1:
                    InsanityScript = this.AddComponent<BodyParalyze>();
                    _insanities.Add(InsanityScript);
                    break;
                case 2:
                    InsanityScript = this.AddComponent<IncreasePulsation>();
                    _insanities.Add(InsanityScript);
                    break;
                case 3:
                    InsanityScript = this.AddComponent<Scream>();
                    _insanities.Add(InsanityScript);
                    break;
                case 4:
                    InsanityScript = this.AddComponent<Hallucination>();
                    _insanities.Add(InsanityScript);
                    break;
                default:
                    Debug.Log("�z��O�̒l�ł��B");
                    break;
            }

            //�t�^���ꂽ�����X�N���v�g���������������폜
            _numbers.Remove(number);
            InsanityScript.Setup();
            Debug.Log("�����X�N���v�g���s");

            //�l�̉�ʏ�ɂ������ʂ̂Ȃ����iFusion��������x���K�v�Ȃ���j��Active�֐������s
            if (InsanityScript != null && !_isBrainwashed.Value && HasInputAuthority)
            {
                if (InsanityScript.GetType().Equals(typeof(EyeParalyze))|| InsanityScript.GetType().Equals(typeof(Hallucination)))
                    InsanityScript.Active();
                else
                    return;
            }
        }
        

        /// <summary>
        /// �Ō�ɕt�^���ꂽ�����X�N���v�g����菜��
        /// </summary>
        /// /// /// <param name="times">�֐���@����</param>
        private void RecoverInsanity(int times)
        {
            if (times == 0 || !HasStateAuthority)
                return;

            int number = 0;

            for (int i = 0; i < times; i++)
            {
                switch (_insanities.Last())
                {
                    case EyeParalyze:
                        number = 0;
                        break;
                    case BodyParalyze:
                        number = 1;
                        break;
                    case IncreasePulsation:
                        number = 2;
                        break;
                    case Scream:
                        number = 3;
                        break;
                    case Hallucination:
                        number = 4;
                        break;
                    default:
                        number = -1;
                        break;
                }

                _insanities.Last().Hide();
                RPC_RemoveInsanityScript(number);

                //��������ȏ�񕜂���K�v���Ȃ��Ƃ��͏I��
                if (_insanities.Count == 0)
                    break;
            }
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_RemoveInsanityScript(int number)
        {
            if (number == -1)
            {
                Debug.LogError("�z��O�̒l�ł��B");
                return;
            }

            //�A�^�b�`����Ă��Ȃ������X�N���v�g�̃��X�g�X�V
            _numbers.Add(number);
            _numbers.Sort();

            //�����X�N���v�g�̍폜, �����X�N���v�g�̃��X�g�X�V
            Destroy((UnityEngine.Object)_insanities.Last());//�����X�N���v�g���폜
            _insanities.Remove(_insanities.Last());

        }

        /// <summary>
        /// ���]��ԂɂȂ����ۂɍs���������܂Ƃ߂��R���[�`��
        /// </summary>
        /// <returns></returns>
        public IEnumerator SelfBrainwash()
        {
            //�S�Ă̔����X�N���v�g�𖳌���
            for (int i = 0; i < _insanities.Count; i++)
            {
                _insanities[i].Hide();
            }
            _isBrainwashed.Value = true;

            Debug.Log("�S�Ă̔������ʂ𖳌������܂���");
            //���]���ʂ�60�b����
            yield return new WaitForSeconds(60f);

            //�S�Ă̔����X�N���v�g��L���ɂ���
            for (int i = 0; i < _insanities.Count; i++)
            {
                _insanities[i].Active();
            }
            _isBrainwashed.Value = false;

            Debug.Log("�S�Ă̔������ʂ�L�������܂���");
        }

        /// <summary>
        /// ���ݕt�^����Ă��锭���X�N���v�g�̔ԍ����܂Ƃ߂�List���擾�ł���֐�(���Ԃ��ۑ��\)
        /// </summary>
        /// <returns></returns>
        public List<int> GetMyNumbers()
        {
            List<int> numbers = new List<int>();
            for (int i = 0; i < _insanities.Count; i++)
            {
                switch (_insanities[i])
                {
                    case EyeParalyze:
                        numbers.Add(0);
                        break;
                    case BodyParalyze:
                        numbers.Add(1);
                        break;
                    case IncreasePulsation:
                        numbers.Add(2);
                        break;
                    case Scream:
                        numbers.Add(3);
                        break;
                    case Hallucination:
                        numbers.Add(4);
                        break;
                    default:
                        break;
                }
            }

            return numbers;
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_SetAnimBool(string animName, bool value)
        {
            _animator.SetBool(animName, value);
        }

#if UNITY_EDITOR
        private void Update()
        {
            //�m�F�p
            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (_insanities.Count == 0)
                {
                    Debug.Log("�������Ă܂���");
                }

                Debug.Log(this.gameObject.name + "�F���݂��Ă��锭���X�N���v�g�̗�");
                for (int i = 0; i < _insanities.Count; i++)
                {
                    Debug.Log(_insanities[i]);
                }
            }
        }
#endif
    }
}

