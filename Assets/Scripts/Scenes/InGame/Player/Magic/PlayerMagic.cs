using UnityEngine;
using UniRx;
using System;
using Fusion;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// プレイヤーの魔法関連を管理するスクリプト
    /// </summary>
    public class PlayerMagic : NetworkBehaviour
    {
        private bool _isCanUseMagic = true;//現在魔法が使えるか否か
        private bool _isUsedMagic = false;//魔法を1度使ったか否か
        [SerializeField] private Magic _myMagic;//使用可能な魔法
        [SerializeField] private PlayerSoundManager _playerSoundManager;//使用可能な魔法

        private Subject<Unit> _FinishUseMagic = new Subject<Unit>();//魔法の詠唱が終わり、効果が発動したらイベントが発生.
        public IObserver<Unit> OnPlayerFinishUseMagic { get { return _FinishUseMagic; } }//外部で_FinishUseMagicのOnNextを呼ぶためにIObserverを公開

        PlayerStatus _myPlayerStatus;
        private TickTimer _coolTimer = TickTimer.None;

        public override void Spawned()
        {
            //自身のPlayerStatusを取得
            _myPlayerStatus = this.GetComponent<PlayerStatus>();

            //_myMagicの中身を自身が設定した呪文に設定する処理
            //α版では無視(インゲーム前が実装されたら実装)
            //RPC_AddMagicScript();

            //呪文スクリプトにPlayerStatusとPlayerMagicを取得させる
            _myMagic.myPlayerStatus = _myPlayerStatus;
            _myMagic.myPlayerMagic = this;

            if (HasStateAuthority)
            {
                //攻撃くらったときのイベントが発行されたときに呪文詠唱を中断(使用中のみ)
                _myPlayerStatus.OnEnemyAttackedMe
                    .Where(_ => _isCanUseMagic && _myPlayerStatus.nowPlayerUseMagic)
                    .Subscribe(_ =>
                    {
                        //詠唱中の移動速度50%Downを解除
                        _myPlayerStatus.UseMagic(false);
                        _myPlayerStatus.ChangeSpeed();

                        //魔法を使う処理をキャンセル
                        _myMagic.cancelMagic = true;
                        Debug.Log("攻撃を受けたので詠唱中止！");
                    }).AddTo(this);


                //呪文の詠唱が終了したら足の遅さを元に戻す。
                _FinishUseMagic
                    .Subscribe(_ =>
                    {
                        //詠唱中の移動速度50%Downを解除
                        _myPlayerStatus.UseMagic(false);
                        _myPlayerStatus.ChangeSpeed();
                        _isUsedMagic = true;
                    }).AddTo(this);
            }

        }

        public override void FixedUpdateNetwork()
        {
            /* --- サーバ(ホスト)側のみの処理に ---*/
            if (HasStateAuthority)
            {
                //入力に基づいた処理を実行
                var input = GetInput<GameplayInput>();
                ProcessInput(input.GetValueOrDefault());

            }
        }

        private void ProcessInput(GameplayInput input)
        {
            //呪文を使用・中断する処理
            if (input.Buttons.IsSet(EInputButton.UseMagicOrStopMagic) && _isCanUseMagic)
            {
                if (_myPlayerStatus.nowPlayerUseMagic && _coolTimer.Expired(Runner))//呪文を詠唱し始めて1秒経ってたら // && _coolTimer.Expired(Runner)
                {
                    //詠唱中の移動速度50%Downを解除
                    _myPlayerStatus.UseMagic(false);
                    _myPlayerStatus.ChangeSpeed();

                    //魔法を使う処理をキャンセル
                    _myMagic.cancelMagic = true;
                    Debug.Log("操作による詠唱中止");

                    //PlayerUIの方で呪文の詠唱時間を表示を終了
                    RPC_CastEventCall(false);

                    //初期化
                    _coolTimer = TickTimer.CreateFromSeconds(Runner, 1.0f);
                }
                else if (!_myPlayerStatus.nowPlayerUseMagic && _coolTimer.ExpiredOrNotRunning(Runner))//呪文をまだ詠唱していないとき
                {
                    //San値が10以下のときは詠唱できない
                    if (_myPlayerStatus.nowPlayerSanValue <= _myMagic.consumeSanValue)
                    {
                        Debug.Log("SAN値が足りないので詠唱できません");
                        return;
                    }

                    //各呪文で一部使用しなくて良い状況であれば呪文を使わせない
                    bool needMagic = true;//呪文を使う必要があるか否か
                    switch (_myMagic)
                    {
                        case SelfBrainwashMagic:
                            if (_myPlayerStatus.nowPlayerSanValue > 50)
                            {
                                needMagic = false;
                                Debug.Log("発狂していないので呪文を使う必要がありません");
                            }
                            break;
                        case RecoverMagic:
                            if (_myPlayerStatus.nowPlayerHealth == _myPlayerStatus.health_max)
                            {
                                needMagic = false;
                                Debug.Log("体力減っていないので呪文を使う必要がありません");
                            }
                            break;
                        default:
                            break;
                    }

                    if (needMagic)
                    {
                        //詠唱中は移動速度50%Down
                        _myPlayerStatus.UseMagic(true);
                        _myPlayerStatus.ChangeSpeed();

                        //魔法を使う処理
                        _myMagic.MagicEffect();
                        Debug.Log("呪文の詠唱開始");

                        //PlayerUIの方で呪文の詠唱時間を表示させる
                        RPC_CastEventCall(true);

                        //SEを鳴らす
                        _playerSoundManager.PlayEffectClip(EffectClip.Cast);

                        //キャンセル用
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
        /// 既に１度呪文を使ったかを管理しているBoolの値を取得する関数
        /// </summary>
        /// <returns>_isUsedMagicの値</returns>
        public bool GetUsedMagicBool()
        {
            return _isUsedMagic;
        }

        /// <summary>
        /// RPCでPlayerUIのキャストゲージを表示・非表示
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
        /// RPCでmagicスクリプトをアタッチ
        /// </summary>
        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
        public void RPC_AddMagicScript()
        {
            //Todo:自身で設定したmagicスクリプトをアタッチする処理

            //呪文スクリプトにPlayerStatusとPlayerMagicを取得させる
            _myMagic.myPlayerStatus = _myPlayerStatus;
            _myMagic.myPlayerMagic = this;
        }
    }
}

