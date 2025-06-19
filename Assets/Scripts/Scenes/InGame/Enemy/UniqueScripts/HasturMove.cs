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
        [SerializeField][Tooltip("追跡中毎秒どれだけ加速するか")] private int _acceraration;
        [SerializeField][Tooltip("追跡中どれだけ加速できるか")] private int _maxSpeedBuff;
        [SerializeField] private bool _debugMode;
        /// <summary>
        /// 現在の加速状況
        /// </summary>
        [HideInInspector][Networked] public int _speedBuff { get; private set; }



        public override void FixedUpdateNetwork()
        {

            //変更を検出しUniRxのイベントを発行す
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
            {//毎秒処理
                _staminaChangeCount -= 1;

                switch (_enemyStatus.State)
                {
                    case EnemyState.Patrolling:
                    case EnemyState.Searching:
                        //通常の場合
                        if (_enemyStatus.StaminaBase > _enemyStatus.Stamina)
                        { //スタミナが削れていたら
                            _enemyStatus.StaminaChange(_enemyStatus.Stamina + 1);
                        }
                        else if (_enemyStatus.StaminaBase < _enemyStatus.Stamina)
                        {
                            _enemyStatus.SetStuminaOver(false);
                        }
                        break;
                    case EnemyState.Chase:
                    case EnemyState.Attack:
                        //走る場合
                        if (_enemyStatus.StaminaOver)
                        { //スタミナが切れ切ったかどうか
                            if (_enemyStatus.StaminaBase <= _enemyStatus.Stamina && !(_enemyStatus.StaminaBase == 0))
                            { //回復した状態にあるかどうか
                                _enemyStatus.SetStuminaOver(false);
                            }
                            else
                            {//回復しきっていないなら回復する
                                if (_enemyStatus.StaminaBase > _enemyStatus.Stamina)
                                { //スタミナが削れていたら、これがあるのはスタミナが0のキャラがいた時にまともに動かすため
                                    _enemyStatus.StaminaChange(_enemyStatus.Stamina + 1);
                                }
                            }
                        }
                        else
                        { //まだスタミナが切れ切って無い場合
                            if (0 >= _enemyStatus.Stamina)
                            { //たった今切れ切ったかどうか
                                _enemyStatus.SetStuminaOver(true);
                                if (_enemyStatus.StaminaBase > _enemyStatus.Stamina)
                                { //スタミナが削れていたら、これがあるのはスタミナが0のキャラがいた時にまともに動かすため
                                    _enemyStatus.StaminaChange(_enemyStatus.Stamina + 1);
                                }
                            }
                            else
                            {
                                //ハスターは追跡している場合スタミナは消費しないのでこの処理は行われない

                            }
                        }

                        if (_speedBuff < _maxSpeedBuff) {
                            _speedBuff += _acceraration;
                            if (_speedBuff > _maxSpeedBuff) { 
                                _speedBuff = _maxSpeedBuff;
                            }
                        }
                        if (_debugMode) { Debug.Log("現在の加速" + _speedBuff); }
                        SpeedChange();
                        break;
                    case EnemyState.FallBack:

                        break;
                    case EnemyState.Discover:
                        break;
                    default:
                        Debug.LogWarning("想定外のEnemyStatus");
                        break;
                }
            }
        }


        protected override void SpeedChange()
        {


            if (_stiffness)
            {//硬直中は移動不能に
                _myAgent.speed = 0;
            }
            else
            {
                if (_enemyStatus.ForcedWalkingTime > 0) //強制的に歩かせる時間中であれば
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
                            Debug.LogWarning("想定外のEnemyStatus");
                            break;


                    }

                }

            }

        }




    }
}
