using Scenes.Ingame.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

namespace Scenes.Ingame.Player
{
    public class PlayerMove : MonoBehaviour
    {
        [SerializeField] CharacterController _characterController;
        [SerializeField] private PlayerStatus _myPlayerStatus;
        [SerializeField] private PlayerSoundManager _myPlayerSoundManager;
        Vector3 _moveVelocity;
        private float _moveAdjustValue;

        //�L�[�o�C���h�̐ݒ�p
        KeyCode dash = KeyCode.LeftShift;
        KeyCode sneak = KeyCode.LeftControl;

        [Header("�J�����֌W")]
        [SerializeField] private GameObject _camera;
        private Vector3 _nowCameraAngle;
        public Vector3 NowCameraAngle { get { return _nowCameraAngle; } }

        [Header("�A�j���[�V�����֌W")]
        [SerializeField]private Animator _animator;

        [SerializeField] private float moveSpeed;
        [Tooltip("�X�^�~�i�̉񕜗�(per 1sec)")][SerializeField] private int _recoverStamina;
        [Tooltip("�X�^�~�i�̉񕜗�[�X�^�~�i�؂ꎞ](per 1sec)")][SerializeField] private int _recoverStaminaOnlyTired;
        [Tooltip("�X�^�~�i�̏����(per 1sec)")][SerializeField] private int _expandStamina;

        private bool _isTiredPenalty = false;
        private bool _isCanMove = true;
        private bool _isCanRotate = true;//UI���쓙���s���Ƃ��p�B��y�ɉ�]�������邩���߂���
        private bool _isCannotMoveByParalyze = false;
        private PlayerActionState _lastPlayerAction = PlayerActionState.Idle;

        //��ɊO���X�N���v�g�ň����t�B�[���h
        private bool _isParalyzed = false;//�g�̖̂��.BodyParalyze.Cs�Ŏg�p
        private bool _isPulsation = false;//�S��������.IncreasePulsation.Cs�Ŏg�p

        void Start()
        {
            _nowCameraAngle = _camera.transform.localEulerAngles;

            //�L�[�o�C���h�̐ݒ�
            KeyCode dash = KeyCode.LeftShift;
            KeyCode sneak = KeyCode.LeftControl;

            #region Subscribes
            //�v���C���[�̊�b���x���ύX���ꂽ��
            _myPlayerStatus.OnPlayerSpeedChange
                .Where(x => x >= 0)
                .Subscribe(x => moveSpeed = x).AddTo(this);

            //�v���C���[�̍s����Ԃ��ω�������
            _myPlayerStatus.OnPlayerActionStateChange
                .Skip(1)//����i�X�|�[������j�͍s��Ȃ�
                .Where(state => state == PlayerActionState.Idle || state == PlayerActionState.Walk || state == PlayerActionState.Dash || state == PlayerActionState.Sneak)
                .Subscribe(state =>
                {
                    //�X�^�~�i�̑���������
                    if (state == PlayerActionState.Dash)
                        StartCoroutine(DecreaseStamina());
                    else if(state != PlayerActionState.Dash)//�X�^�~�i���񕜂ł����Ԃ̎�
                        StartCoroutine(IncreaseStamina());                                                 

                }).AddTo(this);

            //�ҋ@��Ԃɐ؂�ւ�
            //�������͂��Ă��Ȃ� or WS�L�[�̓��������̂悤�Ɍ݂��ɑł������ē����Ȃ��Ƃ��ɐ؂�ւ���
            this.UpdateAsObservable()
                .Where(_ =>!(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) ||
                           _lastPlayerAction != PlayerActionState.Idle && _moveVelocity == Vector3.zero)
                .Subscribe(_ => 
                {
                    _lastPlayerAction = _myPlayerStatus.nowPlayerActionState;//�ω��O�̏�Ԃ��L�^����B
                    _moveAdjustValue = 0;
                });

            //�L�[���͂̏󋵂ɂ����s��Ԃւ̐؂�ւ�
            //�@�_�b�V���L�[�������Ă��Ȃ�,�X�j�[�N�L�[�������Ă��Ȃ�,�ړ������x�N�g����0�łȂ�,WASD�ǂꂩ�͉����Ă���B�����𖞂������Ƃ�
            //�A�����Ă����Ԃ�W�L�[�𗣂����Ƃ�
            //�B�_�b�V���L�[��������Ԃ�ASD�L�[����͂��Ă���.���̂Ƃ�W�L�[�͉����Ă��Ȃ����Ƃ�����
            this.UpdateAsObservable()
                .Where(_ => (!Input.GetKey(dash) && !Input.GetKey(sneak) && _moveVelocity != Vector3.zero &&
                            (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) ) ||
                             (_myPlayerStatus.nowPlayerActionState == PlayerActionState.Dash && !Input.GetKey(KeyCode.W)) || 
                             (Input.GetKey(dash) && !Input.GetKey(KeyCode.W) && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))))
                .Where(_ => _isCanMove && !_isCannotMoveByParalyze)
                .Subscribe(_ => 
                {
                    _lastPlayerAction = _myPlayerStatus.nowPlayerActionState;//�ω��O�̏�Ԃ��L�^����B
                    _moveAdjustValue = 1.0f;
                });

            //�X�^�~�i���؂ꂽ�ۂ̕��s��Ԃւ̐؂�ւ��i�y�i���e�B�����j
            this.UpdateAsObservable()
                .Where(_ => Input.GetKey(dash) && Input.GetKey(KeyCode.W) && _myPlayerStatus.nowStaminaValue == 0)
                .ThrottleFirst(TimeSpan.FromMilliseconds(1000))//1�b�Ԃ̊Ԃ͍ēx�y�i���e�B�����Ȃ��悤�ɂ���B
                .Subscribe(_ =>
                {
                    _lastPlayerAction = _myPlayerStatus.nowPlayerActionState;//�ω��O�̏�Ԃ��L�^����B
                    _moveAdjustValue = 1.0f;

                    _myPlayerSoundManager.PlayEffectClip(EffectClip.Breathlessness, 0.2f);
                    StartCoroutine(CountTiredPenalty());
                });

            //Shift+�ړ��L�[���������Ƃ��_�b�V����Ԃɐ؂�ւ�
            this.UpdateAsObservable()
                .Where(_ => ((Input.GetKeyDown(dash) && Input.GetKey(KeyCode.W)) || (Input.GetKey(dash) && Input.GetKeyDown(KeyCode.W))) && !_isTiredPenalty && _moveVelocity != Vector3.zero)
                .Where(_ => _isCanMove && !_isCannotMoveByParalyze)
                .ThrottleFirst(TimeSpan.FromMilliseconds(500))//0.5�b�Ԃ̊Ԃ͍ēx�_�b�V���ł��Ȃ��悤�ɂ���B
                .Subscribe(_ => 
                {
                    _moveAdjustValue = 2.0f;
                });

            //Ctrl+�ړ��L�[���������Ƃ��E�ѕ�����Ԃɐ؂�ւ�
            this.UpdateAsObservable()
                .Where(_ => (Input.GetKeyDown(sneak) && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))) ||
                            (Input.GetKey(sneak) && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)))
                            && _moveVelocity != Vector3.zero)
                .Where(_ => _isCanMove && !_isCannotMoveByParalyze)
                .Subscribe(_ =>
                {
                    _lastPlayerAction = _myPlayerStatus.nowPlayerActionState;//�ω��O�̏�Ԃ��L�^����B
                    _moveAdjustValue = 0.5f;
                });
            #endregion

            StartCoroutine(CheckParalyze());
        }

        void Update()
        {
            //�����Ă���Ԃ��h���A�j���[�V�������łȂ��Ƃ��̓J�����𑀍�ł���
            if (_myPlayerStatus.nowPlayerSurvive && !_myPlayerStatus.nowReviveAnimationDoing && _isCanRotate)
            {
                float moveMouseX = Input.GetAxis("Mouse X");
                if (Mathf.Abs(moveMouseX) > 0.001f)
                {
                    // ��]���̓��[���h���W��Y��
                    transform.RotateAround(transform.position, Vector3.up, moveMouseX);
                }

                //�J������X�������ɉ�]������B���_���㉺�ɓ�������悤�Ɂi�͈͂ɐ�������j
                float moveMouseY = Input.GetAxis("Mouse Y");
                if (Mathf.Abs(moveMouseY) > 0.001f)
                {
                    _nowCameraAngle.x -= moveMouseY;
                    _nowCameraAngle.x = Mathf.Clamp(_nowCameraAngle.x, -40, 60);
                    _camera.gameObject.transform.localEulerAngles = _nowCameraAngle;
                }
            }
            

            //�������Ԃł���Γ���
            if (_isCanMove && !_isCannotMoveByParalyze && _myPlayerStatus.nowPlayerSurvive && !_myPlayerStatus.nowReviveAnimationDoing)
                Move();
            else if(!_isCanMove || _isCannotMoveByParalyze || !_myPlayerStatus.nowPlayerSurvive || _myPlayerStatus.nowReviveAnimationDoing)
            {
                _lastPlayerAction = _myPlayerStatus.nowPlayerActionState;//�ω��O�̏�Ԃ��L�^����B
                _myPlayerStatus.ChangePlayerActionState(PlayerActionState.Idle);//�ҋ@��Ԃֈڍs
                _animator.SetFloat("MovementSpeed", _characterController.velocity.magnitude);//�����Ȃ��Ƃ��Ɋm���Idle��Ԃ̃��[�V�����ɂ��邽��
            }

            //���R����
            if (this.gameObject.transform.position.y > 0)
                _characterController.Move(new Vector3(0, -9.8f * Time.deltaTime, 0));
        }

        private void Move()
        {
            float forward = 0;
            float right = 0;

            _moveVelocity = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
            {
                _moveVelocity += transform.forward;
                forward++;
            }
            if (Input.GetKey(KeyCode.S))
            {
                _moveVelocity -= transform.forward;
                forward--;
            }
            if (Input.GetKey(KeyCode.A))
            {
                _moveVelocity -= transform.right;
                right++;
            }
            if (Input.GetKey(KeyCode.D))
            {
                _moveVelocity += transform.right;
                right--;
            }

            //�ړ�������
            _moveVelocity = _moveVelocity.normalized;
            _characterController.Move(_moveVelocity * Time.deltaTime * moveSpeed * _moveAdjustValue);

            //�v���C���[�̌������A�j���[�^�[�ɔF��������
            SetPlayerMoveDirection(forward, right);

            //CharacterController�̑��x�ɉ����ď�Ԃ�ω�
            //1.0�����炵�Ă���̂́A�ǂɌ������Ĉړ����Ă���Ƃ��ɒl��0�ł͂Ȃ����ƂƁA�����̊O��l��΍􂷂邽��
            if (1.0f < _characterController.velocity.magnitude && _characterController.velocity.magnitude <= moveSpeed / 2 + 1.0f)
            {
                if (Input.GetKey(sneak))
                {
                    _myPlayerStatus.ChangePlayerActionState(PlayerActionState.Sneak);
                    _animator.SetFloat("MovementSpeed", moveSpeed / 2);
                }
                else
                { 
                    _myPlayerStatus.ChangePlayerActionState(PlayerActionState.Walk);
                    _animator.SetFloat("MovementSpeed", moveSpeed);
                }                  
            }
            else if (moveSpeed / 2 + 1.0f < _characterController.velocity.magnitude && _characterController.velocity.magnitude <= moveSpeed + 1.0f)
            {
                _myPlayerStatus.ChangePlayerActionState(PlayerActionState.Walk);
                _animator.SetFloat("MovementSpeed", moveSpeed);
            }
            else if (moveSpeed + 1.0f < _characterController.velocity.magnitude && _characterController.velocity.magnitude <= moveSpeed * 2 + 1.0f)
            {
                _myPlayerStatus.ChangePlayerActionState(PlayerActionState.Dash);
                _animator.SetFloat("MovementSpeed", moveSpeed * 2);
            }
            else if(_characterController.velocity.magnitude < 1.0f)
            {
                _myPlayerStatus.ChangePlayerActionState(PlayerActionState.Idle);
                _animator.SetFloat("MovementSpeed", _characterController.velocity.magnitude);
            }
        }

        /// <summary>
        /// �v���C���[�̌������A�j���[�^�[�ɔF��������֐�
        /// </summary>
        /// <param name="forward">�O�����</param>
        /// <param name="right">���E����</param>
        private void SetPlayerMoveDirection(float forward, float right)
        {
            /*�A�j���[�^�[�̃p�����[�^�[�uDirection�v�̒�`�ɂ���
             *0.00: �O����
             *0.25: ������
             *0.50: ������
             *0.75: �E����
             */

            //�O�����Ɉړ�
            if (forward == 1)
            {
                if (right == 0)//���͂Ȃ�
                    _animator.SetFloat("Direction", 0);
                else if (right == 1)//�E
                    _animator.SetFloat("Direction", 0.875f);
                else if (right == -1)//��
                    _animator.SetFloat("Direction", 0.125f);
            }
            else if (forward == 0)//�O������̓��͂Ȃ�
            {
                if (right == 1)//�E
                    _animator.SetFloat("Direction", 0.75f);
                else if (right == -1)//��
                    _animator.SetFloat("Direction", 0.25f);
            }
            else if (forward == -1)//�������Ɉړ�
            {
                if (right == 0)//���͂Ȃ�
                    _animator.SetFloat("Direction", 0.5f);
                else if (right == 1)//�E
                    _animator.SetFloat("Direction", 0.625f);
                else if (right == -1)//��
                    _animator.SetFloat("Direction", 0.375f);
            }
        }

        private IEnumerator DecreaseStamina()
        {
            while (_myPlayerStatus.nowPlayerActionState == PlayerActionState.Dash)
            { 
                yield return new WaitForSeconds(0.1f);
                _myPlayerStatus.ChangeStamina(_expandStamina / 10 * (_isPulsation ? 2 : 1), ChangeValueMode.Damage);
            }           
        }

        private IEnumerator IncreaseStamina()
        {
            yield return null;

            if (_isTiredPenalty)//�X�^�~�i���S���
            {
                yield return new WaitForSeconds(0.5f);
                while (_myPlayerStatus.nowPlayerActionState != PlayerActionState.Dash)
                {
                    yield return new WaitForSeconds(0.1f);
                    _myPlayerStatus.ChangeStamina(_recoverStaminaOnlyTired / 10, ChangeValueMode.Heal);
                }
            }else//�ʏ펞
            {
                while (_myPlayerStatus.nowPlayerActionState != PlayerActionState.Dash)
                {
                    yield return new WaitForSeconds(0.1f);
                    _myPlayerStatus.ChangeStamina(_recoverStamina / 10, ChangeValueMode.Heal);
                }
            }

        }

        private IEnumerator CountTiredPenalty()
        { 
            _isTiredPenalty = true;
            yield return new WaitUntil(() => _myPlayerStatus.nowStaminaValue == 100);//�X�^�~�i��100�܂ŉ񕜂���̂�҂�
            _isTiredPenalty = false;
        }

        private IEnumerator CheckParalyze()
        { 
            while (true) 
            {
                yield return new WaitForSeconds(5.0f);
                if (_isParalyzed)
                {
                    //25%�̊m����1�b�ԓ����Ȃ�
                    int random = UnityEngine.Random.Range(0, 4);
                    if (random == 0)
                    {
                        _isCannotMoveByParalyze = true;
                        Debug.Log("�̂��v���悤�ɓ����Ȃ�...!!");
                    }
                    else
                    {
                        _isCannotMoveByParalyze = false;
                        Debug.Log("������!!");
                    }                       
                }
            }
        }

        /// <summary>
        /// �̂���Ⴢ��Ă��邩�ۂ������肷��֐�
        /// </summary>
        /// <param name="value"></param>
        public void Paralyze(bool value)
        {
            _isParalyzed = value;

            //��჏�Ԃ������Ă���A������悤�ɂ�����
            if (value == false)
                _isCannotMoveByParalyze = false;
        }

        /// <summary>
        /// �S�����������Ă��邩�ۂ������肷��֐�
        /// </summary>
        /// <param name="value"></param>
        public void Pulsation(bool value)
        {
            _isPulsation = value;
        }

        /// <summary>
        /// �ړ��ł��邩�ۂ������肷��֐�
        /// </summary>
        /// <param name="value"></param>
        public void MoveControl(bool value)
        { 
            _isCanMove = value;
        }

        /// <summary>
        /// ��]�ł��邩�ۂ������肷��֐�
        /// </summary>
        /// <param name="value"></param>
        public void RotateControl(bool value)
        {
            _isCanRotate = value;
        }

    }
}


