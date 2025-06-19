using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UniRx;

using Fusion;
using Fusion.Addons.SimpleKCC;

using Cinemachine;



namespace Scenes.Ingame.Player
{
    public class MultiPlayerMove : NetworkBehaviour
    {
        Dictionary<PlayerActionState, float> _ajustValueOfState = new()
        {
            [PlayerActionState.Idle] = 0,
            [PlayerActionState.Walk] = 1,
            [PlayerActionState.Dash] = 1.5f,
            [PlayerActionState.Sneak] = 0.5f
        };


        Vector3 _moveVelocity;
        float _moveAdjustValue;


        [Header("参照")]
        private GameObject _camera;
        [SerializeField] Transform CameraPivot;
        [SerializeField] NetworkCharacterController _multiCharacterController;
        [SerializeField] SimpleKCC _simpleKCC;
        [SerializeField] PlayerInput _input;
        [SerializeField] PlayerStatus _myPlayerStatus;
        [SerializeField] PlayerSoundManager _myPlayerSoundManager;

        [Header("移動")]
        [SerializeField] float moveSpeed;
        [SerializeField] float GroundAcceleration = 55f;
        [SerializeField] float GroundDeceleration = 25f;
        [SerializeField] float AirAcceleration = 25f;
        [SerializeField] float AirDeceleration = 1.3f;
        [SerializeField] float UpGravity = 15f;
        [SerializeField] float DownGravity = 25f;
        [Tooltip("スタミナの回復量(per 1sec)")] [SerializeField] private int _recoverStamina;
        [Tooltip("スタミナの回復量[スタミナ切れ時](per 1sec)")] [SerializeField] private int _recoverStaminaOnlyTired;
        [Tooltip("スタミナの消費量(per 1sec)")] [SerializeField] private int _expandStamina;

        bool _isTiredPenalty = false;
        bool _isCanMove = true;
        bool _isCanRotate = true;//UI操作等を行うとき用。手軽に回転を許可するか決められる
        bool _isCannotMoveByParalyze = false;

        //主に外部スクリプトで扱うフィールド
        bool _isParalyzed = false;//身体の麻痺.BodyParalyze.Csで使用
        bool _isPulsation = false;//心拍数増加.IncreasePulsation.Csで使用


        [Header("カメラ関係")]
        Vector3 _nowCameraAngle;
        public Vector3 NowCameraAngle { get { return _nowCameraAngle; } }

        [Networked] private Vector2 _networkpitchRotation { get; set; }
        private ChangeDetector _changeDetector;

        public override void Spawned()
        {
            name = $"{Object.InputAuthority} ({(HasInputAuthority ? "Input Authority" : (HasStateAuthority ? "State Authority" : "Proxy"))})";

            if (HasInputAuthority == false)
            {
                //相手プレイヤーのVirtualCameraをオフにする
                var virtualCameras = GetComponentsInChildren<CinemachineVirtualCamera>(true);
                for (int i = 0; i < virtualCameras.Length; i++)
                {
                    virtualCameras[i].enabled = false;
                }
            }
            else
            {
                _camera = GameObject.FindWithTag("MainCamera");
            }

            #region Subscribes

            //プレイヤーの基礎速度が変更されたら
            _myPlayerStatus.OnPlayerSpeedChange
                .Where(x => x >= 0)
                .Subscribe(x => moveSpeed = x).AddTo(this);

            //プレイヤーの行動状態が変化したら
            _myPlayerStatus.OnPlayerActionStateChange
                .Skip(1)//初回（スポーン直後）は行わない
                .Where(state => state == PlayerActionState.Idle || state == PlayerActionState.Walk || state == PlayerActionState.Dash || state == PlayerActionState.Sneak)
                .Subscribe(state =>
                {
                    //スタミナの増減を決定
                    if (state == PlayerActionState.Dash)
                    {
                        Debug.Log("ダッシュ");
                        StartCoroutine(DecreaseStamina());
                    }
                        
                    else if (state != PlayerActionState.Dash)//スタミナを回復できる状態の時
                        StartCoroutine(IncreaseStamina());
                }).AddTo(this);

            _myPlayerStatus.OnPlayerStaminaChange
                .Skip(1)
                //.Where(x => x <= 0)
                .Subscribe(_ =>
                {
                    if(_ <= 0)
                    {
                        _myPlayerStatus.ChangePlayerActionState(PlayerActionState.Walk);    //歩行状態に変更
                        _myPlayerSoundManager.PlayEffectClip(EffectClip.Breathlessness, 0.2f);
                        StartCoroutine(CountTiredPenalty());
                    }
                    
                }).AddTo(this);
            #endregion Subscribes

            StartCoroutine(CheckParalyze());
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }

        public override void FixedUpdateNetwork()
        {
            /*
            if (HasStateAuthority)
            {
                var input = GetInput<GameplayInput>();
                ProcessInput(input.GetValueOrDefault(), _input.PreviousButtons);
            }

            if (HasInputAuthority)
            { 
                foreach (var change in _changeDetector.DetectChanges(this))
                {
                    switch (change)
                    {
                        case nameof(_networkpitchRotation):
                            CameraPivot.localRotation = Quaternion.Euler(_networkpitchRotation);
                            break;
                    
                        default:
                            break;
                    }
                }
            }
            */
            //問題があったら上の処理をコメントアウト, 下の処理を適応させる
            var input = GetInput<GameplayInput>();
            ProcessInput(input.GetValueOrDefault(), _input.PreviousButtons);
        }

        //入力の処理
        void ProcessInput(GameplayInput input, NetworkButtons previousButtons)
        {
            if (_isCanRotate)
                _simpleKCC.AddLookRotation(input.LookRotation, -89f, 89f);

            _simpleKCC.SetGravity(_simpleKCC.RealVelocity.y >= 0f ? -UpGravity : -DownGravity);

            
            if (input.MoveDirection == Vector2.zero) //移動入力が無ければ
            {
                _myPlayerStatus.ChangePlayerActionState(PlayerActionState.Idle);
            }
            //移動可能状態であれば
            else if(_isCanMove && !_isCannotMoveByParalyze && _myPlayerStatus.nowPlayerSurvive && !_myPlayerStatus.nowReviveAnimationDoing)
            {
                //ダッシュキーが押されている状態で疲れていなければ
                if (input.Buttons.IsSet(EInputButton.Dash) && !_isTiredPenalty)
                {
                    //ASD入力が無ければ
                    if (input.MoveDirection.x == 0 && input.MoveDirection.y > 0)
                    {
                        _myPlayerStatus.ChangePlayerActionState(PlayerActionState.Dash);
                    }
                    else
                        _myPlayerStatus.ChangePlayerActionState(PlayerActionState.Walk);
                }
                //スニークキーが押されていれば
                else if (input.Buttons.IsSet(EInputButton.Sneak))
                {
                    _myPlayerStatus.ChangePlayerActionState(PlayerActionState.Sneak);
                }
                //何も押されていなければ
                else
                {
                    _myPlayerStatus.ChangePlayerActionState(PlayerActionState.Walk);
                }
            }
            //移動不可能状態であれば
            else
            {
                _myPlayerStatus.ChangePlayerActionState(PlayerActionState.Idle);
            }

            //プレイヤーの状態に応じた調整速度を設定
            _moveAdjustValue = _ajustValueOfState[_myPlayerStatus.nowPlayerActionState];

            //_nowCameraAngle = _camera.transform.eulerAngles;

            var moveDirection = _simpleKCC.TransformRotation * new Vector3(input.MoveDirection.x, 0f, input.MoveDirection.y);
            //Walk,Dash,Sneakに応じた移動速度を計算
            var desireMoveVelocity = moveDirection * moveSpeed * _moveAdjustValue;
            MovePlayer(desireMoveVelocity);

            if (_isCanRotate)
            {
                _networkpitchRotation = _simpleKCC.GetLookRotation(true, false);
                CameraPivot.localRotation = Quaternion.Euler(_networkpitchRotation);
            }

            //var pitchRotation = _simpleKCC.GetLookRotation(true, false);
            //CameraPivot.localRotation = Quaternion.Euler(pitchRotation);

        }

        //引数方向へ移動
        void MovePlayer(Vector3 desiredMoveVelocity = default)
        {
            float accelaration;
            if (desiredMoveVelocity == Vector3.zero)
            {
                accelaration = _simpleKCC.IsGrounded == true ? GroundDeceleration : AirDeceleration;
            }
            else
            {
                accelaration = _simpleKCC.IsGrounded == true ? GroundAcceleration : AirAcceleration;
            }

            _moveVelocity = Vector3.Lerp(_moveVelocity, desiredMoveVelocity, accelaration * Runner.DeltaTime);

            _simpleKCC.Move(_moveVelocity);
        }

        IEnumerator DecreaseStamina()
        {
            while (_myPlayerStatus.nowPlayerActionState == PlayerActionState.Dash)
            {
                yield return new WaitForSeconds(0.1f * (_myPlayerStatus.IsStaminaBuff ? 2 : 1));
                Debug.Log("スタミナ減少");
                _myPlayerStatus.ChangeStamina(_expandStamina / 10 * (_isPulsation ? 2 : 1), ChangeValueMode.Damage);
            }
        }

        IEnumerator IncreaseStamina()
        {
            yield return null;

            if (_isTiredPenalty)//スタミナ完全消費時
            {
                yield return new WaitForSeconds(0.5f);
                while (_myPlayerStatus.nowPlayerActionState != PlayerActionState.Dash)
                {
                    yield return new WaitForSeconds(0.1f);
                    _myPlayerStatus.ChangeStamina(_recoverStaminaOnlyTired / 10, ChangeValueMode.Heal);
                }
            }
            else//通常時
            {
                while (_myPlayerStatus.nowPlayerActionState != PlayerActionState.Dash)
                {
                    yield return new WaitForSeconds(0.1f);
                    _myPlayerStatus.ChangeStamina(_recoverStamina / 10, ChangeValueMode.Heal);
                }
            }

        }

        IEnumerator CountTiredPenalty()
        {
            _isTiredPenalty = true;
            yield return new WaitUntil(() => _myPlayerStatus.nowStaminaValue == 100);//スタミナが100まで回復するのを待つ
            _isTiredPenalty = false;
        }

        IEnumerator CheckParalyze()
        {
            while (true)
            {
                yield return new WaitForSeconds(5.0f);
                if (_isParalyzed)
                {
                    //25%の確率で1秒間動けない
                    int random = UnityEngine.Random.Range(0, 4);
                    if (random == 0)
                    {
                        _isCannotMoveByParalyze = true;
                        Debug.Log("体が思うように動かない...!!");
                    }
                    else
                    {
                        _isCannotMoveByParalyze = false;
                        Debug.Log("動ける!!");
                    }
                }
            }
        }

        /// <summary>
        /// 体が麻痺しているか否かを決定する関数
        /// </summary>
        /// <param name="value"></param>
        public void Paralyze(bool value)
        {
            _isParalyzed = value;

            //麻痺状態が治ってたら、動けるようにもする
            if (value == false)
                _isCannotMoveByParalyze = false;
        }

        /// <summary>
        /// 心拍数が増えているか否かを決定する関数
        /// </summary>
        /// <param name="value"></param>
        public void Pulsation(bool value)
        {
            _isPulsation = value;
        }

        /// <summary>
        /// 移動できるか否かを決定する関数
        /// </summary>
        /// <param name="value"></param>
        public void MoveControl(bool value)
        {
            _isCanMove = value;
        }

        /// <summary>
        /// 回転できるか否かを決定する関数
        /// </summary>
        /// <param name="value"></param>
        public void RotateControl(bool value)
        {
            _isCanRotate = value;
        }

    }
}

