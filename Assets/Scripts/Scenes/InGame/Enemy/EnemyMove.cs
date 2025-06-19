using UnityEngine;
using UnityEngine.AI;
using UniRx;
using Fusion;
using System;


namespace Scenes.Ingame.Enemy
{
    /// <summary>
    /// 敵キャラクターの移動を管理する
    /// </summary>
    public class EnemyMove : NetworkBehaviour
    {
        protected NavMeshAgent _myAgent;
        [SerializeField] protected EnemyStatus _enemyStatus;

        protected BoolReactiveProperty _endMoveProperty = new BoolReactiveProperty(false);
        public IObservable<bool> _OnEndMoveChanged { get { return _endMoveProperty; } }
        public bool EndMove {get{ return _endMoveProperty.Value; } }


        /// <summary> 硬直しているか否か</summary>
        protected bool _stiffness = false;

        protected float _staminaChangeCount = 0;//スタミナを毎秒減らすのに使用
        [Networked] protected Vector3 _movePosition { get; set; }//移動先

        public Vector3 GetMovePosition() {
            return _movePosition;
        }

        protected Vector3 _initialPosition = new Vector3(30, 0, 18);//初期位置保存用変数

        protected ChangeDetector _changeDetector;


        /// <summary>
        /// 初期化処理外部から呼び出す
        /// </summary>
        public void Init() {
            _myAgent = GetComponent<NavMeshAgent>();
            if (_myAgent == null) Debug.LogError("NavMeshAgentが認識できません");
            _myAgent.destination = this.transform.position;
            _initialPosition = this.transform.position;

            //変更を検出する準備をする
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

            _enemyStatus.OnStiffnessTimeChange.Subscribe(stiffnessTime => {
                if (stiffnessTime > 0)
                {
                    _stiffness = true;
                }
                else {
                    _stiffness = false;
                }
                SpeedChange();
            }
            ).AddTo(this);

           _enemyStatus.OnBindChange
                .Skip(1)//初期化の時は無視
                .Subscribe(x =>
                {
                    SpeedChange();
                }).AddTo(this);
            _enemyStatus.OnEnemyStateChange.Subscribe(x =>{
                SpeedChange();
            }).AddTo(this);
            _enemyStatus.OnIsWaterEffectDebuffChange.Skip(1).Subscribe(x =>
            { 
                SpeedChange(); 
            }).AddTo(this);
            _enemyStatus.OnStaminaOverChange.Subscribe(x =>
            {
                SpeedChange();
            }).AddTo(this);

            _enemyStatus.OnSlowTimeChange.Subscribe(x =>
              {
                  SpeedChange();
              }).AddTo(this);
            _enemyStatus.OnForcedWalkingChange.Subscribe(x =>
            {
                SpeedChange();
            }).AddTo(this);

            _enemyStatus.OnSpeedChanged.Subscribe(x => 
            {
                Debug.Log("SpeedSet" + x);
                _myAgent.speed = x; 
            }).AddTo(this);

            SpeedChange();
        }

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
            } else
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
                                 //スタミナを削れるなら、これがあるのはスタミナが0のキャラがいた時にまともに動かすため
                                    _enemyStatus.StaminaChange(_enemyStatus.Stamina - 1);
                                
                            }
                        }
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


        protected virtual void SpeedChange() {
            

            if (_stiffness)
            {//硬直中は移動不能に
                _enemyStatus.SetSpeed(0);
            }
            else
            {
                if (_enemyStatus.ForcedWalkingTime > 0) //強制的に歩かせる時間中であれば
                { 
                _enemyStatus.SetSpeed(_enemyStatus.PatrollingSpeed * (_enemyStatus.Bind ? 0.1f : 1) * (_enemyStatus.IsWaterEffectDebuff ? 0.8f : 1) * ((_enemyStatus.SlowTime > 0) ? 0.1F : 1));
                }
                else{
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
                                _enemyStatus.SetSpeed(_enemyStatus.SearchSpeed * (_enemyStatus.Bind ? 0.1f : 1) * (_enemyStatus.IsWaterEffectDebuff ? 0.8f : 1) * ((_enemyStatus.SlowTime > 0) ? 0.1F : 1));
                            }
                            else
                            {
                                _enemyStatus.SetSpeed(_enemyStatus.ChaseSpeed * (_enemyStatus.Bind ? 0.1f : 1) * (_enemyStatus.IsWaterEffectDebuff ? 0.8f : 1) * ((_enemyStatus.SlowTime > 0) ? 0.1F : 1));
                            }
                            break;
                        case EnemyState.Attack:

                            if (_enemyStatus.StaminaOver)
                            {
                                _enemyStatus.SetSpeed(_enemyStatus.SearchSpeed * (_enemyStatus.Bind ? 0.1f : 1) * (_enemyStatus.IsWaterEffectDebuff ? 0.8f : 1) * ((_enemyStatus.SlowTime > 0) ? 0.1F : 1));
                            }
                            else
                            {
                                _enemyStatus.SetSpeed(_enemyStatus.ChaseSpeed * (_enemyStatus.Bind ? 0.1f : 1) * (_enemyStatus.IsWaterEffectDebuff ? 0.8f : 1) * ((_enemyStatus.SlowTime > 0) ? 0.1F : 1));
                            }
                            break;
                        case EnemyState.FallBack:
                            break;
                        case EnemyState.Stun:
                            _enemyStatus.SetSpeed(0);
                            break;
                        case EnemyState.Discover:
                            _enemyStatus.SetSpeed(0);
                            break;
                        default:
                            Debug.LogWarning("想定外のEnemyStatus");
                            break;
                    }
                }
            }
        }

        public void SetMovePosition(Vector3 targetPosition)
        {
                _movePosition = targetPosition;
        }

        /// <summary>
        /// 座標を初期位置に移動する関数
        /// </summary>
        public void ResetPosition()
        {
            _myAgent.enabled = false;
            _enemyStatus.SetEnemyState(EnemyState.Patrolling);
            transform.position = _initialPosition;
            _endMoveProperty.Value = true;
            _myAgent.enabled = true;
        }
    } 
}
