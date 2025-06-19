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

        //キーバインドの設定用
        KeyCode dash = KeyCode.LeftShift;
        KeyCode sneak = KeyCode.LeftControl;

        [Header("カメラ関係")]
        [SerializeField] private GameObject _camera;
        private Vector3 _nowCameraAngle;
        public Vector3 NowCameraAngle { get { return _nowCameraAngle; } }

        [Header("アニメーション関係")]
        [SerializeField]private Animator _animator;

        [SerializeField] private float moveSpeed;
        [Tooltip("スタミナの回復量(per 1sec)")][SerializeField] private int _recoverStamina;
        [Tooltip("スタミナの回復量[スタミナ切れ時](per 1sec)")][SerializeField] private int _recoverStaminaOnlyTired;
        [Tooltip("スタミナの消費量(per 1sec)")][SerializeField] private int _expandStamina;

        private bool _isTiredPenalty = false;
        private bool _isCanMove = true;
        private bool _isCanRotate = true;//UI操作等を行うとき用。手軽に回転を許可するか決められる
        private bool _isCannotMoveByParalyze = false;
        private PlayerActionState _lastPlayerAction = PlayerActionState.Idle;

        //主に外部スクリプトで扱うフィールド
        private bool _isParalyzed = false;//身体の麻痺.BodyParalyze.Csで使用
        private bool _isPulsation = false;//心拍数増加.IncreasePulsation.Csで使用

        void Start()
        {
            _nowCameraAngle = _camera.transform.localEulerAngles;

            //キーバインドの設定
            KeyCode dash = KeyCode.LeftShift;
            KeyCode sneak = KeyCode.LeftControl;

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
                        StartCoroutine(DecreaseStamina());
                    else if(state != PlayerActionState.Dash)//スタミナを回復できる状態の時
                        StartCoroutine(IncreaseStamina());                                                 

                }).AddTo(this);

            //待機状態に切り替え
            //何も入力していない or WSキーの同時押しのように互いに打ち消して動かないときに切り替える
            this.UpdateAsObservable()
                .Where(_ =>!(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) ||
                           _lastPlayerAction != PlayerActionState.Idle && _moveVelocity == Vector3.zero)
                .Subscribe(_ => 
                {
                    _lastPlayerAction = _myPlayerStatus.nowPlayerActionState;//変化前の状態を記録する。
                    _moveAdjustValue = 0;
                });

            //キー入力の状況による歩行状態への切り替え
            //①ダッシュキーを押していない,スニークキーを押していない,移動方向ベクトルが0でない,WASDどれかは押している。これらを満たしたとき
            //②走っている状態でWキーを離したとき
            //③ダッシュキー押した状態でASDキーを入力している.このときWキーは押していないことが条件
            this.UpdateAsObservable()
                .Where(_ => (!Input.GetKey(dash) && !Input.GetKey(sneak) && _moveVelocity != Vector3.zero &&
                            (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) ) ||
                             (_myPlayerStatus.nowPlayerActionState == PlayerActionState.Dash && !Input.GetKey(KeyCode.W)) || 
                             (Input.GetKey(dash) && !Input.GetKey(KeyCode.W) && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))))
                .Where(_ => _isCanMove && !_isCannotMoveByParalyze)
                .Subscribe(_ => 
                {
                    _lastPlayerAction = _myPlayerStatus.nowPlayerActionState;//変化前の状態を記録する。
                    _moveAdjustValue = 1.0f;
                });

            //スタミナが切れた際の歩行状態への切り替え（ペナルティがつく）
            this.UpdateAsObservable()
                .Where(_ => Input.GetKey(dash) && Input.GetKey(KeyCode.W) && _myPlayerStatus.nowStaminaValue == 0)
                .ThrottleFirst(TimeSpan.FromMilliseconds(1000))//1秒間の間は再度ペナルティがつかないようにする。
                .Subscribe(_ =>
                {
                    _lastPlayerAction = _myPlayerStatus.nowPlayerActionState;//変化前の状態を記録する。
                    _moveAdjustValue = 1.0f;

                    _myPlayerSoundManager.PlayEffectClip(EffectClip.Breathlessness, 0.2f);
                    StartCoroutine(CountTiredPenalty());
                });

            //Shift+移動キーを押したときダッシュ状態に切り替え
            this.UpdateAsObservable()
                .Where(_ => ((Input.GetKeyDown(dash) && Input.GetKey(KeyCode.W)) || (Input.GetKey(dash) && Input.GetKeyDown(KeyCode.W))) && !_isTiredPenalty && _moveVelocity != Vector3.zero)
                .Where(_ => _isCanMove && !_isCannotMoveByParalyze)
                .ThrottleFirst(TimeSpan.FromMilliseconds(500))//0.5秒間の間は再度ダッシュできないようにする。
                .Subscribe(_ => 
                {
                    _moveAdjustValue = 2.0f;
                });

            //Ctrl+移動キーを押したとき忍び歩き状態に切り替え
            this.UpdateAsObservable()
                .Where(_ => (Input.GetKeyDown(sneak) && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))) ||
                            (Input.GetKey(sneak) && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)))
                            && _moveVelocity != Vector3.zero)
                .Where(_ => _isCanMove && !_isCannotMoveByParalyze)
                .Subscribe(_ =>
                {
                    _lastPlayerAction = _myPlayerStatus.nowPlayerActionState;//変化前の状態を記録する。
                    _moveAdjustValue = 0.5f;
                });
            #endregion

            StartCoroutine(CheckParalyze());
        }

        void Update()
        {
            //生きている間かつ蘇生アニメーション中でないときはカメラを操作できる
            if (_myPlayerStatus.nowPlayerSurvive && !_myPlayerStatus.nowReviveAnimationDoing && _isCanRotate)
            {
                float moveMouseX = Input.GetAxis("Mouse X");
                if (Mathf.Abs(moveMouseX) > 0.001f)
                {
                    // 回転軸はワールド座標のY軸
                    transform.RotateAround(transform.position, Vector3.up, moveMouseX);
                }

                //カメラをX軸方向に回転させる。視点が上下に動かせるように（範囲に制限あり）
                float moveMouseY = Input.GetAxis("Mouse Y");
                if (Mathf.Abs(moveMouseY) > 0.001f)
                {
                    _nowCameraAngle.x -= moveMouseY;
                    _nowCameraAngle.x = Mathf.Clamp(_nowCameraAngle.x, -40, 60);
                    _camera.gameObject.transform.localEulerAngles = _nowCameraAngle;
                }
            }
            

            //動ける状態であれば動く
            if (_isCanMove && !_isCannotMoveByParalyze && _myPlayerStatus.nowPlayerSurvive && !_myPlayerStatus.nowReviveAnimationDoing)
                Move();
            else if(!_isCanMove || _isCannotMoveByParalyze || !_myPlayerStatus.nowPlayerSurvive || _myPlayerStatus.nowReviveAnimationDoing)
            {
                _lastPlayerAction = _myPlayerStatus.nowPlayerActionState;//変化前の状態を記録する。
                _myPlayerStatus.ChangePlayerActionState(PlayerActionState.Idle);//待機状態へ移行
                _animator.SetFloat("MovementSpeed", _characterController.velocity.magnitude);//動けないときに確定でIdle状態のモーションにするため
            }

            //自由落下
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

            //移動させる
            _moveVelocity = _moveVelocity.normalized;
            _characterController.Move(_moveVelocity * Time.deltaTime * moveSpeed * _moveAdjustValue);

            //プレイヤーの向きをアニメーターに認識させる
            SetPlayerMoveDirection(forward, right);

            //CharacterControllerの速度に応じて状態を変化
            //1.0ずつずらしているのは、壁に向かって移動しているときに値が0ではないことと、多少の外れ値を対策するため
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
        /// プレイヤーの向きをアニメーターに認識させる関数
        /// </summary>
        /// <param name="forward">前後方向</param>
        /// <param name="right">左右方向</param>
        private void SetPlayerMoveDirection(float forward, float right)
        {
            /*アニメーターのパラメーター「Direction」の定義について
             *0.00: 前向き
             *0.25: 左向き
             *0.50: 後ろ向き
             *0.75: 右向き
             */

            //前向きに移動
            if (forward == 1)
            {
                if (right == 0)//入力なし
                    _animator.SetFloat("Direction", 0);
                else if (right == 1)//右
                    _animator.SetFloat("Direction", 0.875f);
                else if (right == -1)//左
                    _animator.SetFloat("Direction", 0.125f);
            }
            else if (forward == 0)//前後方向の入力なし
            {
                if (right == 1)//右
                    _animator.SetFloat("Direction", 0.75f);
                else if (right == -1)//左
                    _animator.SetFloat("Direction", 0.25f);
            }
            else if (forward == -1)//後ろ向きに移動
            {
                if (right == 0)//入力なし
                    _animator.SetFloat("Direction", 0.5f);
                else if (right == 1)//右
                    _animator.SetFloat("Direction", 0.625f);
                else if (right == -1)//左
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

            if (_isTiredPenalty)//スタミナ完全消費時
            {
                yield return new WaitForSeconds(0.5f);
                while (_myPlayerStatus.nowPlayerActionState != PlayerActionState.Dash)
                {
                    yield return new WaitForSeconds(0.1f);
                    _myPlayerStatus.ChangeStamina(_recoverStaminaOnlyTired / 10, ChangeValueMode.Heal);
                }
            }else//通常時
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
            yield return new WaitUntil(() => _myPlayerStatus.nowStaminaValue == 100);//スタミナが100まで回復するのを待つ
            _isTiredPenalty = false;
        }

        private IEnumerator CheckParalyze()
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


