using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using static Fusion.NetworkBehaviour;

namespace Scenes.Ingame.Enemy
{
    public class HasturMove : EnemyMove
    {
        [SerializeField][Tooltip("�ǐՒ����b�ǂꂾ���������邩")] private int _acceraration;
        [SerializeField][Tooltip("�ǐՒ��ǂꂾ�������ł��邩")] private int _maxSpeedBuff;
        [SerializeField] private bool _debugMode;
        /// <summary>
        /// ���݂̉�����
        /// </summary>
        [HideInInspector][Networked] public int _speedBuff { get; private set; }



        public override void FixedUpdateNetwork()
        {

            //�ύX�����o��UniRx�̃C�x���g�𔭍s��
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(_movePosition):
                        _myAgent.destination = _movePosition;
                        break;
                }
            }


            if (Vector3.Magnitude(this.transform.position - _myAgent.path.corners[_myAgent.path.corners.Length - 1]) < 1.5f)
            {
                _endMoveProperty.Value = true;
            }
            else
            {
                _endMoveProperty.Value = false;
            }

            _staminaChangeCount += Time.deltaTime;
            if (_staminaChangeCount > 1)
            {//���b����
                _staminaChangeCount -= 1;

                switch (_enemyStatus.State)
                {
                    case EnemyState.Patrolling:
                    case EnemyState.Searching:
                        //�ʏ�̏ꍇ
                        if (_enemyStatus.StaminaBase > _enemyStatus.Stamina)
                        { //�X�^�~�i�����Ă�����
                            _enemyStatus.StaminaChange(_enemyStatus.Stamina + 1);
                        }
                        else if (_enemyStatus.StaminaBase < _enemyStatus.Stamina)
                        {
                            _enemyStatus.SetStuminaOver(false);
                        }
                        break;
                    case EnemyState.Chase:
                    case EnemyState.Attack:
                        //����ꍇ
                        if (_enemyStatus.StaminaOver)
                        { //�X�^�~�i���؂�؂������ǂ���
                            if (_enemyStatus.StaminaBase <= _enemyStatus.Stamina && !(_enemyStatus.StaminaBase == 0))
                            { //�񕜂�����Ԃɂ��邩�ǂ���
                                _enemyStatus.SetStuminaOver(false);
                            }
                            else
                            {//�񕜂������Ă��Ȃ��Ȃ�񕜂���
                                if (_enemyStatus.StaminaBase > _enemyStatus.Stamina)
                                { //�X�^�~�i�����Ă�����A���ꂪ����̂̓X�^�~�i��0�̃L�������������ɂ܂Ƃ��ɓ���������
                                    _enemyStatus.StaminaChange(_enemyStatus.Stamina + 1);
                                }
                            }
                        }
                        else
                        { //�܂��X�^�~�i���؂�؂��Ė����ꍇ
                            if (0 >= _enemyStatus.Stamina)
                            { //���������؂�؂������ǂ���
                                _enemyStatus.SetStuminaOver(true);
                                if (_enemyStatus.StaminaBase > _enemyStatus.Stamina)
                                { //�X�^�~�i�����Ă�����A���ꂪ����̂̓X�^�~�i��0�̃L�������������ɂ܂Ƃ��ɓ���������
                                    _enemyStatus.StaminaChange(_enemyStatus.Stamina + 1);
                                }
                            }
                            else
                            {
                                //�n�X�^�[�͒ǐՂ��Ă���ꍇ�X�^�~�i�͏���Ȃ��̂ł��̏����͍s���Ȃ�

                            }
                        }

                        if (_speedBuff < _maxSpeedBuff) {
                            _speedBuff += _acceraration;
                            if (_speedBuff > _maxSpeedBuff) { 
                                _speedBuff = _maxSpeedBuff;
                            }
                        }
                        if (_debugMode) { Debug.Log("���݂̉���" + _speedBuff); }
                        SpeedChange();
                        break;
                    case EnemyState.FallBack:

                        break;
                    case EnemyState.Discover:
                        break;
                    default:
                        Debug.LogWarning("�z��O��EnemyStatus");
                        break;
                }
            }
        }


        protected override void SpeedChange()
        {


            if (_stiffness)
            {//�d�����͈ړ��s�\��
                _myAgent.speed = 0;
            }
            else
            {
                if (_enemyStatus.ForcedWalkingTime > 0) //�����I�ɕ������鎞�Ԓ��ł����
                {
                    _enemyStatus.SetSpeed(_enemyStatus.PatrollingSpeed * (_enemyStatus.Bind ? 0.1f : 1) * (_enemyStatus.IsWaterEffectDebuff ? 0.8f : 1) * ((_enemyStatus.SlowTime > 0) ? 0.1F : 1));
                }
                else
                {
                    switch (_enemyStatus.State)
                    {
                        case EnemyState.Patrolling:
                            _enemyStatus.SetSpeed(_enemyStatus.PatrollingSpeed * (_enemyStatus.Bind ? 0.1f : 1) * (_enemyStatus.IsWaterEffectDebuff ? 0.8f : 1) * ((_enemyStatus.SlowTime > 0) ? 0.1F : 1));

                            break;
                        case EnemyState.Searching:
                            _enemyStatus.SetSpeed(_enemyStatus.SearchSpeed * (_enemyStatus.Bind ? 0.1f : 1) * (_enemyStatus.IsWaterEffectDebuff ? 0.8f : 1) * ((_enemyStatus.SlowTime > 0) ? 0.1F : 1));

                            break;
                        case EnemyState.Chase:
                            if (_enemyStatus.StaminaOver)
                            {
                                _enemyStatus.SetSpeed((_enemyStatus.SearchSpeed + _speedBuff) * (_enemyStatus.Bind ? 0.1f : 1) * (_enemyStatus.IsWaterEffectDebuff ? 0.8f : 1) * ((_enemyStatus.SlowTime > 0) ? 0.1F : 1));
                            }
                            else
                            {
                                _enemyStatus.SetSpeed((_enemyStatus.ChaseSpeed + _speedBuff) * (_enemyStatus.Bind ? 0.1f : 1) * (_enemyStatus.IsWaterEffectDebuff ? 0.8f : 1) * ((_enemyStatus.SlowTime > 0) ? 0.1F : 1));
                            }
                            break;


                        case EnemyState.Attack:

                            if (_enemyStatus.StaminaOver)
                            {
                                _enemyStatus.SetSpeed((_enemyStatus.SearchSpeed + _speedBuff) * (_enemyStatus.Bind ? 0.1f : 1) * (_enemyStatus.IsWaterEffectDebuff ? 0.8f : 1) * ((_enemyStatus.SlowTime > 0) ? 0.1F : 1));
                            }
                            else
                            {
                                _enemyStatus.SetSpeed((_enemyStatus.ChaseSpeed + _speedBuff) * (_enemyStatus.Bind ? 0.1f : 1) * (_enemyStatus.IsWaterEffectDebuff ? 0.8f : 1) * ((_enemyStatus.SlowTime > 0) ? 0.1F : 1));
                            }
                            break;
                        case EnemyState.FallBack:

                            break;
                        case EnemyState.Discover:
                            _enemyStatus.SetSpeed(0);
                            break;
                        case EnemyState.Stun:
                            _enemyStatus.SetSpeed(0);
                            break;
                        default:
                            Debug.LogWarning("�z��O��EnemyStatus");
                            break;


                    }

                }

            }

        }




    }
}
