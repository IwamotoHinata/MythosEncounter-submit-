using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Fusion;
using System;
using Common.Player;
using Cysharp.Threading.Tasks;
using System.Threading;
using Scenes.Ingame.Enemy;

/// <summary>
/// プレイヤーのステータスを管理するクラス
/// MV(R)PにおけるModelの役割を想定
/// </summary>
namespace Scenes.Ingame.Player
{
    public class PlayerStatus : NetworkBehaviour
    {
        //プレイヤーのデータベース(仮置き)
        [Header("プレーヤーのデータベース")]
        [SerializeField] private int _playerID = 0;
        [SerializeField] private int _healthBase = 100;
        [SerializeField] private int _staminaBase = 100;
        [SerializeField] private int _sanBase = 100;
        [SerializeField] private int _speedBase = 5;
        [SerializeField][Tooltip("プレイヤーの持つ照明の光の届く距離")] private float _lightrangeBase = 20;
        [SerializeField][Tooltip("プレイヤーのしゃがみ歩き時の音量")] private float _sneakVolumeBase = 5;
        [SerializeField][Tooltip("プレイヤーの歩き時の音量")] private float _walkVolumeBase = 10;
        [SerializeField][Tooltip("プレイヤーの走りの音量")] private float _runVolumeBase = 15;


        //現在のステータスの変数（今後ネットワーク化予定）
        //初期化についても今後はデータベースを参照して行うようにする。
        [SerializeField] private IntReactiveProperty _health = new IntReactiveProperty();//HP.ゼロになると死亡
        [SerializeField] private IntReactiveProperty _bleedingHealth = new IntReactiveProperty();//出血処理用HP
        [SerializeField] private IntReactiveProperty _stamina = new IntReactiveProperty();//スタミナ
        [SerializeField] private IntReactiveProperty _san = new IntReactiveProperty();//SAN値
        [SerializeField] private IntReactiveProperty _speed = new IntReactiveProperty();//移動速度の基準値
        [SerializeField] private BoolReactiveProperty _survive = new BoolReactiveProperty();//生死.trueのときは生きている
        [SerializeField] private BoolReactiveProperty _bleeding = new BoolReactiveProperty();//出血状態.trueのときに時間経過で体力が減少
        [SerializeField] private bool _isGetDamageSlow = true;//ダメージを受けたときに足が遅くなるか
        private Subject<Unit> _getDamange = new Subject<Unit>();//攻撃された場合の処理

        [SerializeField] private ReactiveProperty<PlayerActionState> _playerActionState = new ReactiveProperty<PlayerActionState>();
        [SerializeField] private FloatReactiveProperty _lightrange = new FloatReactiveProperty();//光の届く距離
        [SerializeField] private FloatReactiveProperty _sneakVolume = new FloatReactiveProperty();//しゃがみ時の音量
        [SerializeField] private FloatReactiveProperty _walkVolume = new FloatReactiveProperty();//しゃがみ時の音量
        [SerializeField] private FloatReactiveProperty _runVolume = new FloatReactiveProperty();//しゃがみ時の音量

        [Header("必要なコンポーネント")]
        [SerializeField] private Animator _anim;
        [SerializeField] private CharacterController _controller;
        [SerializeField] private CapsuleCollider _cupsuleCollider;
        [SerializeField] private PlayerMagic _playerMagic;
        [SerializeField] private PlayerItem _playerItem;
        [SerializeField] private MultiPlayerMove _multiPlayerMove;

        private Subject<float> castEvent = new Subject<float>();//呪文の詠唱時間を発行
        private Subject<Unit> cancelCastEvent = new Subject<Unit>();//呪文の詠唱をキャンセルされた際のイベント
        public IObserver<float> OnCastEventCall { get { return castEvent; } }//別のスクリプトから呪文の詠唱時間イベントのOnNextを飛ばせるようにする



        //その他のSubject
        private Subject<Unit> _enemyAttackedMe = new Subject<Unit>();//敵から攻撃を食らったときのイベント
        private Subject<PlayerStatus> playerActionStateChangeEvent = new Subject<PlayerStatus>();//プレイヤーのStateが変化した時のイベント（どのプレイヤーステータスか判別可能）

        //それぞれの購読側を公開する。他のClassでSubscribeできる。
        public IObservable<int> OnPlayerHealthChange { get { return _health; } }//_health(体力)が変化した際にイベントが発行
        public IObservable<int> OnPlayerBleedingHealthChange { get { return _bleedingHealth; } }//_bleedinghealthが変化した際にイベントが発行
        public IObservable<int> OnPlayerStaminaChange { get { return _stamina; } }//_stamina(スタミナ)が変化した際にイベントが発行
        public IObservable<int> OnPlayerSanValueChange { get { return _san; } }//_san(SAN値)が変化した際にイベントが発行
        public IObservable<int> OnPlayerSpeedChange { get { return _speed; } }//_speed(移動速度の基準値)が変化した際にイベントが発行
        public IObservable<Unit> OnGetDamange { get { return _getDamange; } }//攻撃された場合の処理

        public IObservable<bool> OnPlayerSurviveChange { get { return _survive; } }//_survive(生死)が変化した際にイベントが発行
        public IObservable<bool> OnPlayerbleedingChange { get { return _bleeding; } }//_bleeding(出血状態)が変化した際にイベントが発行
        public IObservable<PlayerActionState> OnPlayerActionStateChange { get { return _playerActionState; } }//_PlayerActionState(プレイヤーの行動状態)が変化した際にイベントが発行
        public IObservable<float> OnLightrangeChange { get { return _lightrange; } }//プレイヤーの光の届く距離が変化した場合にイベントが発行
        public IObservable<float> OnSneakVolumeChange { get { return _sneakVolume; } }//プレイヤーの忍び歩きの音が届く距離が変化した場合にイベントが発行
        public IObservable<float> OnWalkVolumeChange { get { return _walkVolume; } }//プレイヤーの歩く音が届く距離が変化した場合にイベントが発行
        public IObservable<float> OnRunVolumeChange { get { return _runVolume; } }//プレイヤーの走る音が届く距離が変化した場合にイベントが発行

        public IObservable<Unit> OnEnemyAttackedMe { get { return _enemyAttackedMe; } }//敵から攻撃を受けた際のイベントを登録させる
        public IObserver<Unit> OnEnemyAttackedMeEvent { get { return _enemyAttackedMe; } }//敵から攻撃を受けた際にイベントが発行
        public IObservable<PlayerStatus> OnPlayerActionStateChangeEvent { get { return playerActionStateChangeEvent; } }//PlayerStateが変化した場合にイベント発行

        public IObservable<float> OnCastEvent { get { return castEvent; } }//呪文詠唱を始めた際に呼ばれるイベント
        public IObservable<Unit> OnCancelCastEvent { get { return cancelCastEvent; } }//敵から攻撃を受けた際のイベントを登録させる
        public IObserver<Unit> OnCancelCastEventCall { get { return cancelCastEvent; } }//敵から攻撃を受けた際にイベントが発行

        //一部情報の開示
        public int playerID { get { return _playerID; } }
        public int health_max { get { return _healthBase; } }
        public int nowPlayerHealth { get { return _networkHealth; } }
        public int stamina_max { get { return _staminaBase; } }
        public int nowStaminaValue { get { return _networkStamina; } }
        public int nowPlayerSanValue { get { return _networkSanValue; } }
        public int nowPlayerSpeed { get { return _networkSpeed; } }

        public bool nowBleedingValue { get { return _networkBleeding; } }
        public bool nowPlayerSurvive { get { return _networkSurvive; } }

        public PlayerActionState nowPlayerActionState { get { return _networkPlayerActionState; } }
        public float nowPlayerLightRange { get { return _networkLightrange; } }
        public float nowPlayerSneakVolume { get { return _networkSneakVolume; } }
        public float nowPlayerWalkVolume { get { return _networkWalkVolume; } }
        public float nowPlayerRunVolume { get { return _networkRunVolume; } }

        public bool nowPlayerUseMagic { get { return _isUseMagic; } }
        public bool nowReviveAnimationDoing { get { return _startReviveAnimation; } }


        [HideInInspector] public int lastHP;//HPの変動前の数値を記録。比較に用いる
        [HideInInspector] public int lastSanValue;//SAN値の変動前の数値を記録。比較に用いる
        [HideInInspector] public int bleedingDamage = 10;//出血時に受けるダメージ
        private bool _isInvincibleMode = false;//デバッグ用. 敵による体力,SAN値の減少を無効化

        //アニメーション関連の変数
        private int _deathEventCount = 0;//死亡アニメーションのイベント回数確認用
        private bool _startReviveAnimation = false;//蘇生アニメーションが始まったか否か

        private bool _startDeathAnimation = false;//死亡アニメーションが始まったか否か
        private bool _isStaminaHealBuff = false;
        private bool _isStaminaBuff = false;
        public bool IsStaminaBuff { get { return _isStaminaBuff; } }


        private CancellationTokenSource _cancellationTokenSource;


        /*--- ネットワーク同期用 ---*/
        private ChangeDetector _changeDetector;
        [Networked] private int _networkHealth { get; set; }
        [Networked] private int _networkBleedingHealth { get; set; }
        [Networked] private int _networkStamina { get; set; }
        [Networked] private int _networkSanValue { get; set; }
        [Networked] private int _networkSpeed { get; set; }
        [Networked] private bool _networkSurvive { get; set; }
        [Networked] private bool _networkBleeding { get; set; }
        [Networked] private float _networkLightrange { get; set; }
        [Networked] private float _networkSneakVolume { get; set; }
        [Networked] private float _networkWalkVolume { get; set; }
        [Networked] private float _networkRunVolume { get; set; }
        [Networked] private float _netWorkeStanTime { get; set; }
        [Networked] private PlayerActionState _networkPlayerActionState { get; set; }

        [Networked] private bool _networkFinishInit { get; set; } = false;
        public bool NetworkFinishInit { get { return _networkFinishInit; } }

        [Networked] private bool _isUseItem { get; set; } = false;
        [Networked] private bool _isUseMagic { get; set; } = false;
        [Networked] private bool _isHaveCharm { get; set; } = false;
        [Networked] private bool _isProtect { get; set; } = false;//バリア呪文でダメージを防ぐか否か
        [Networked] private bool _isUseEscapePoint { get; set; } = false;
        [Networked] private bool _isPulsationBleeding { get; set; } = false;
        [Networked] private bool _isGetDamage { get; set; } = false;

        [Networked] private NetworkId _lastAtackEnemy { get; set;}

        public bool IsUseItem { get { return _isUseItem; } }

        [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
        public void RPC_Init(int playerID = 0)//今後IDを用いてデータベースにアクセスし、Init処理するかも?
        {
            //ReactivePropertyの初期値設定
            _health.Value = _healthBase;
            _bleedingHealth.Value = _healthBase;
            _stamina.Value = _staminaBase;
            _san.Value = _sanBase;
            _speed.Value = _speedBase;
            _survive.Value = true;
            _bleeding.Value = false;
            _lightrange.Value = _lightrangeBase;
            _sneakVolume.Value = _sneakVolumeBase;
            _walkVolume.Value = _walkVolumeBase;
            _runVolume.Value = _runVolumeBase;
            _playerActionState.Value = PlayerActionState.Idle;

            //その他
            lastHP = _healthBase;
            lastSanValue = _sanBase;
            bleedingDamage = 10;

            //Networked変数の初期化
            if (HasStateAuthority)
            {
                _networkHealth = _healthBase;
                _networkBleedingHealth = _healthBase;
                _networkStamina = _staminaBase;
                _networkSanValue = _sanBase;
                _networkSpeed = _speedBase;
                _networkSurvive = true;
                _networkBleeding = false;
                _networkLightrange = _lightrangeBase;
                _networkSneakVolume = _sneakVolumeBase;
                _networkWalkVolume = _walkVolumeBase;
                _networkRunVolume = _runVolumeBase;
                _networkPlayerActionState = PlayerActionState.Idle;

                _networkFinishInit = true;
            }
        }

        public override void Spawned()
        {
            //PlayerGUIPresenterの設定
            FindObjectOfType<PlayerGUIPresenter>().SetInputAuthorityPlayer(this.Object);
            _cancellationTokenSource = new CancellationTokenSource();

            Debug.Log("スポーン処理");
            RPC_Init();
            /*--- PlayerStatusの初期化処理 ---*/
            //ホストのみに処理させたい処理を記述
            if (HasStateAuthority)
            {
                //RPC_Init();
                _health.Subscribe(x => CheckHealth(x)).AddTo(this);//体力が変化したときにゲーム内で変更を加える
                _stamina.Subscribe(x => CheckStamina(x)).AddTo(this);//スタミナが変化したときにゲーム内で変更を加える
                _san.Subscribe(x => CheckSanValue(x)).AddTo(this);//SAN値が変化したときにゲーム内で変更を加える
                _bleeding.
                    Where(x => x == true).
                    Subscribe(_ => StartCoroutine(Bleeding(bleedingDamage))).AddTo(this);//出血状態になったときに出血処理を開始

                //死亡判定
                _survive
                    .Skip(1)
                    .Subscribe(x => RPC_CheckSurvive(x)).AddTo(this);



                OnPlayerActionStateChange.Skip(1).Subscribe(x => playerActionStateChangeEvent.OnNext(this)).AddTo(this);
            }

            //同期する必要のない個人用の設定などを記述
            if (HasInputAuthority)
            {
                //攻撃を受けた際に呪文の詠唱時間UIを非表示にする
                OnEnemyAttackedMe
                    .Where(_ => _isUseMagic)
                    .Subscribe(x => cancelCastEvent.OnNext(default)).AddTo(this);


                //呪文解放条件関連
                _san
                    .Skip(1)
                    .First(x => x <= 50)
                    .Where(_ => HasInputAuthority)//入力権限があったら
                    .Subscribe(x => SpellUnlockSystem.Instance.SetSpellUnlockInfoData(3, true)).AddTo(this);//SAN値が初めて50%をきったら自己洗脳呪文の解放条件を満たす


                //ダメージを受けたときに足を遅くする
                if (_isGetDamageSlow)
                {
                    _getDamange
                        .Subscribe(async _ =>
                        {
                            _isGetDamage = true;
                            ChangeSpeed();
                            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: _cancellationTokenSource.Token);
                            _isGetDamage = false;
                            ChangeSpeed();
                        }).AddTo(this);
                }
            }

            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }

        public override void FixedUpdateNetwork()
        {
            if (_networkFinishInit)
            {
                foreach (var change in _changeDetector.DetectChanges(this))
                {
                    switch (change)
                    {
                        case nameof(_networkHealth):
                            if (_health.Value> _networkHealth)
                            {
                                SoundManager.Instance.PlayDamageSe(transform.position);
                            }
                            _health.Value = _networkHealth;
                            break;
                        case nameof(_networkBleedingHealth):
                            if (_bleedingHealth.Value > _networkBleedingHealth)
                            {
                                _getDamange.OnNext(default);
                            }
                            _bleedingHealth.Value = _networkBleedingHealth;
                            break;
                        case nameof(_networkStamina):
                            _stamina.Value = _networkStamina;
                            break;
                        case nameof(_networkSanValue):
                            _san.Value = _networkSanValue;
                            break;
                        case nameof(_networkSpeed):
                            _speed.Value = _networkSpeed;
                            break;
                        case nameof(_networkSurvive):
                            _survive.Value = _networkSurvive;
                            break;
                        case nameof(_networkBleeding):
                            _bleeding.Value = _networkBleeding;
                            break;
                        case nameof(_networkLightrange):
                            _lightrange.Value = _networkLightrange;
                            break;
                        case nameof(_networkSneakVolume):
                            _sneakVolume.Value = _networkSneakVolume;
                            break;
                        case nameof(_networkWalkVolume):
                            _walkVolume.Value = _networkWalkVolume;
                            break;
                        case nameof(_networkRunVolume):
                            _runVolume.Value = _networkRunVolume;
                            break;
                        case nameof(_networkPlayerActionState):
                            _playerActionState.Value = _networkPlayerActionState;
                            break;
                        default:
                            break;
                    }
                }
            }

            if (_netWorkeStanTime >0) {
                _netWorkeStanTime -= Runner.DeltaTime;
                if (_netWorkeStanTime < 0) {
                    _netWorkeStanTime = 0;
                }
                if (_netWorkeStanTime == 0) ChangeSpeed();
            }


            //死亡時 or 蘇生時に当たり判定を変化
            ChangeCollider();
        }

        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR

            //デバッグ用.(必要無くなれば消す)
            if (Input.GetKeyDown(KeyCode.L))
            {
                ChangeHealth(10, ChangeValueMode.Damage);
                ChangeSanValue(10, ChangeValueMode.Damage);
                ChangeBleedingBool(true);
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                ChangeBleedingBool(false);
                ChangeHealth(10, ChangeValueMode.Heal);
                ChangeSanValue(10, ChangeValueMode.Heal);
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                _enemyAttackedMe.OnNext(default);
            }

            if (Input.GetKeyDown(KeyCode.Home))
            {
                if (!HasInputAuthority)
                    return;

                Debug.Log("_networkHealth:" + _networkHealth);
                Debug.Log("_networkBleedingHealth:" + _networkBleedingHealth);
                Debug.Log("_networkStamina:" + _networkStamina);
                Debug.Log("_networkSanValue:" + _networkSanValue);
                Debug.Log("_networkSpeed:" + _networkSpeed);
                Debug.Log("_networkSurvive:" + _networkSurvive);
                Debug.Log("_networkBleeding:" + _networkBleeding);
                Debug.Log("_networkLightrange:" + _networkLightrange);
                Debug.Log("_networkSneakVolume:" + _networkSneakVolume);
                Debug.Log("_networkWalkVolume:" + _networkWalkVolume);
                Debug.Log("_networkRunVolume:" + _networkRunVolume);
            }

            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                Debug.Log("_networkFinishInit:" + _networkFinishInit);
                Debug.Log(this.Object.name + ":_isUseItem:" + _isUseItem);
                Debug.Log(this.Object.name + ":_isUseMagic:" + _isUseMagic);
                Debug.Log(this.Object.name + ":_isHaveCharm:" + _isHaveCharm);
                Debug.Log(this.Object.name + ":_isProtect:" + _isProtect);
                Debug.Log(this.Object.name + ":_isUseEscapePoint:" + _isUseEscapePoint);
                Debug.Log(this.Object.name + ":_isPulsationBleeding:" + _isPulsationBleeding);
                Debug.Log(this.Object.name + ":_isGetDamage:" + _isGetDamage);
                Debug.Log(this.Object.name + ":_isStaminaBuff:" + _isStaminaBuff);
            }

#endif           
        }

        /// <summary>
        /// 死亡時 or 蘇生時に当たり判定を変更する関数
        /// </summary>
        private void ChangeCollider()
        {
            //死亡時に当たり判定を死体と同じ場所に動かす
            //Todo:蘇生時に当たり判定を体と同じ場所に動かす（今後実装）
            if (_startDeathAnimation || _startReviveAnimation)
            {
                Debug.Log(this.Object.name + "当たり判定変更中");

                _cupsuleCollider.height = _anim.GetFloat("ColliderHeight");//　コライダの高さの調整       
                _cupsuleCollider.center = new Vector3(_cupsuleCollider.center.x, _anim.GetFloat("ColliderCenterY"), _cupsuleCollider.center.z);//　コライダの中心位置の調整

                //　コライダの半径の調整
                _cupsuleCollider.radius = _anim.GetFloat("ColliderRadius");

                //CharacterControllerのコライダー半径の変更（死亡時のアバターの壁埋まり防止のため）
                if (_startDeathAnimation && !_startReviveAnimation)//死亡時
                    _controller.radius = 1.2f;
                else if (!_startDeathAnimation && _startReviveAnimation)//蘇生時
                    _controller.radius = 0.4f;


                //　コライダの向きの調整
                if (_anim.GetFloat("ColliderDirection") >= 1.0f)
                {
                    _cupsuleCollider.direction = 2;//Z軸方向の向きに変化
                }
                else
                {
                    _cupsuleCollider.direction = 1;//Y軸方向の向きに変化
                }
            }
        }

        /// <summary>
        /// 体力を変更させるための関数
        /// </summary>
        /// <param name="value">変更量</param>
        /// <param name="mode">値変更のモードを指定</param>
        public void ChangeHealth(int value, ChangeValueMode mode)
        {
            if (!HasStateAuthority)
                return;

            if (mode == ChangeValueMode.Heal)
            {
                lastHP = _networkHealth;
                _networkHealth = Mathf.Min(100, _networkHealth + value);
                _networkBleedingHealth = Mathf.Min(100, _networkBleedingHealth + value);

                if (HasInputAuthority)
                    SpellUnlockSystem.Instance.IncreaseHealedHealth(_networkHealth - lastHP);
            }
            else if (mode == ChangeValueMode.Damage)
            {
                if (_isInvincibleMode)
                    return;

                if (_isProtect)
                {
                    _isProtect = false;
                    Debug.Log("ダメージが無効化されました。");
                    return;
                }
                lastHP = _networkHealth;
                _networkHealth = Mathf.Max(0, _networkHealth - value);
                _networkBleedingHealth = Mathf.Max(0, _networkBleedingHealth - value);
            }
            else if (mode == ChangeValueMode.Bleeding)
            {
                lastHP = _networkHealth;
                _networkHealth = Mathf.Max(0, _networkHealth - value);
            }
            else if (mode == ChangeValueMode.Debug)
            {
                lastHP = _health.Value;
                _networkHealth = value;
                _networkBleedingHealth = value;
            }
        }

        /// <summary>
        /// 体力を変更させるための関数(最大体力の比率を用いて計算する用)
        /// </summary>
        /// <param name="value">変更量</param>
        /// <param name="mode">値変更のモードを指定</param>
        public void ChangeHealth(float value, ChangeValueMode mode)
        {
            if (!HasStateAuthority)
                return;

            if (mode == ChangeValueMode.Heal)
            {
                lastHP = _networkHealth;
                _networkHealth = Mathf.Min(100, _networkHealth + (int)(_healthBase * value));
                _networkBleedingHealth = Mathf.Min(100, _networkBleedingHealth + (int)(_healthBase * value));

                if (HasInputAuthority)
                    SpellUnlockSystem.Instance.IncreaseHealedHealth(_networkHealth - lastHP);
            }
            else if (mode == ChangeValueMode.Damage)
            {
                if (_isInvincibleMode)
                    return;

                if (_isProtect)
                {
                    _isProtect = false;
                    Debug.Log("ダメージが無効化されました。");
                    return;
                }
                lastHP = _networkHealth;
                _networkHealth = Mathf.Max(0, _networkHealth - (int)(_healthBase * value));
                _networkBleedingHealth = Mathf.Max(0, _networkBleedingHealth - (int)(_healthBase * value));
            }
            else if (mode == ChangeValueMode.Bleeding)
            {
                lastHP = _networkHealth;
                _networkHealth = Mathf.Max(0, _networkHealth - (int)(_healthBase * value));
            }
        }


        /// <summary>
        /// 出血処理用体力を変更させるための関数
        /// </summary>
        /// <param name="value">変更量</param>
        public void ChangeBleedingHealth(int value)
        {
            if (!HasStateAuthority)
                return;

            _networkBleedingHealth = Mathf.Max(0, _networkBleedingHealth - value);
        }

        /// <summary>
        /// スタミナを変更させるための関数
        /// </summary>
        /// <param name="value">変更量</param>
        /// <param name="mode">値変更のモードを指定</param>
        public void ChangeStamina(int value, ChangeValueMode mode)
        {
            if (!HasStateAuthority)
                return;

            if (mode == ChangeValueMode.Heal)
                _networkStamina = Mathf.Min(100, _networkStamina + (int)(value * (_isStaminaHealBuff ? 1.5f : 1)));
            else if (mode == ChangeValueMode.Damage)
            {
                //_networkStamina = Mathf.Max(0, _networkStamina - (int)(value * (_isStaminaBuff ? 0.5f : 1)));
                _networkStamina = Mathf.Max(0, _networkStamina - (int)value);
            }
        }

        /// <summary>
        /// SAN値を変更させるための関数
        /// </summary>
        /// <param name="value">変更量</param>
        /// <param name="mode">値変更のモードを指定</param>
        public void ChangeSanValue(int value, ChangeValueMode mode)
        {
            if (!HasStateAuthority)
                return;

            if (mode == ChangeValueMode.Heal)
            {
                lastSanValue = _networkSanValue;
                _networkSanValue = Mathf.Min(100, _networkSanValue + value);
            }

            else if (mode == ChangeValueMode.Damage)
            {
                if (_isInvincibleMode)
                    return;

                lastSanValue = _networkSanValue;
                _networkSanValue = Mathf.Max(0, _networkSanValue - value / (_isHaveCharm ? 2 : 1));
            }
            else if (mode == ChangeValueMode.Debug)
            {
                lastSanValue = _san.Value;
                _networkSanValue = value;
            }
        }

        /// <summary>
        /// 移動速度を変更させる関数
        /// </summary>
        public void ChangeSpeed()
        {
            if (!HasStateAuthority)
                return;
            if (_netWorkeStanTime > 0)
            {
                _networkSpeed = 0;
            }
            else 
            {
                _networkSpeed = (int)(_speedBase * (_isUseItem ? 0.5f : 1) * (_isUseMagic ? 0.5f : 1) * (_isUseEscapePoint ? 0.5f : 1) * (_isGetDamage ? 0.5f : 1));
            }
        }

        /// <summary>
        /// アイテムを使っているのか管理するための関数
        /// </summary>
        /// <param name="value"></param>
        public void UseItem(bool value)
        {
            if (!HasStateAuthority)
                return;

            _isUseItem = value;
        }

        /// <summary>
        /// 呪文を唱えているか管理するための関数
        /// </summary>
        /// <param name="value"></param>
        public void UseMagic(bool value)
        {
            if (!HasStateAuthority)
                return;

            _isUseMagic = value;
        }

        /// <summary>
        /// お守りがアイテムスロットにあるか判定する関数
        /// </summary>
        /// <param name="value"></param>
        public void HaveCharm(bool value)
        {
            if (!HasStateAuthority)
                return;

            _isHaveCharm = value;
        }

        /// <summary>
        /// バリア呪文を使ったときに_isProtectをtrueにさせる関数
        /// </summary>
        /// <param name="value"></param>
        public void UseProtectMagic(bool value)
        {
            if (!HasStateAuthority)
                return;

            _isProtect = value;
        }

        /// <summary>
        /// デバッグ用。敵による体力,SAN値の減少を無効化するのか判定するBoolを変更する関数
        /// </summary>
        /// <param name="value"></param>
        public void SetInvincibleModeBool(bool value)
        {
            _isInvincibleMode = value;
        }


        /// <summary>
        /// 呪文を唱えているか管理するための関数
        /// </summary>
        /// <param name="value"></param>
        public void UseEscapePoint(bool value, float time = 0)
        {
            if (value)
            {
                castEvent.OnNext(time);
            }
            _isUseEscapePoint = value;
        }

        /// <summary>
        /// 心拍数に応じて出血状態時の出血量を変化させる関数
        /// </summary>
        /// <param name="value"></param>
        public void PulsationBleeding(bool value)
        {
            _isPulsationBleeding = value;
        }

        /// <summary>
        /// _bleeding(出血状態)の値を変更するための関数
        /// </summary>
        /// <param name="value"></param>
        public void ChangeBleedingBool(bool value)
        {
            if (!HasStateAuthority)
                return;

            _networkBleeding = value;
            if (value == false)
            {
                if (HasInputAuthority)
                    SpellUnlockSystem.Instance.IncreaseStopBleedingTimes();
            }

        }

        public void ChangePlayerActionState(PlayerActionState state)
        {
            _networkPlayerActionState = state;
        }

        public void ReviveCharacter()
        {
            Debug.Log("ReviveCharacter起動");
            _networkSurvive = true;
            ChangeHealth(50, ChangeValueMode.Heal);
        }

        /// <summary>
        /// 出血状態の処理を行う関数。
        /// </summary>
        /// <returns></returns>
        private IEnumerator Bleeding(int damage)
        {
            ChangeBleedingHealth(damage * (_isPulsationBleeding ? 2 : 1));
            for (int i = 0; i < damage; i++)
            {
                if (_networkBleeding)
                {
                    ChangeHealth((_isPulsationBleeding ? 2 : 1), ChangeValueMode.Bleeding);
                    Debug.Log("出血ダメージが入りました。");

                    if (i == damage - 1)//最後の出血処理の後、すぐに出血Boolをfalseを変更させるため
                        break;
                }
                else
                {
                    Debug.Log("出血状態が回復したので終了します");
                    break;
                }


                yield return new WaitForSeconds(1.0f);
            }
            ChangeBleedingBool(false);
            yield break;
        }

        /// <summary>
        /// 体力に関する処理を行う
        /// </summary>
        /// <param name="health">残り体力</param>
        private void CheckHealth(int health)
        {
            if (health <= 0) 
            {
                _networkSurvive = false;
                //最後に攻撃を行った敵へとキルを通知
                if (_lastAtackEnemy != null)
                {
                    NetworkObject enemyObject = Runner.FindObject(_lastAtackEnemy);
                    if (enemyObject != null)
                    {
                        if (enemyObject.TryGetComponent<EnemyAttack>(out EnemyAttack enemyAttack))
                        {
                            enemyAttack.SendPlayerKillEvent();
                        }
                    }
                }
            }
                

            if (lastHP >= 0 && health < 0)
            {
                _networkSurvive = true;
            }

        }

        /// <summary>
        /// スタミナに関する処理を行う
        /// </summary>
        /// <param name="stamina">残りスタミナ</param>
        private void CheckStamina(int stamina)
        {
            //スタミナ残量をゲーム内に表示.
            //Debug.Log("残りスタミナ：" + stamina);
        }

        /// <summary>
        /// san値に関する処理を行う
        /// </summary>
        /// <param name="sanValue">残りのSAN値</param>
        private void CheckSanValue(int sanValue)
        {
            //Debug.Log("残りsan値：" + sanValue);           

            if (sanValue <= 0)
                _networkSurvive = false;
        }


        /// <summary>
        /// 生死状態の変更時に処理を行う
        /// </summary>
        /// <param name="isSurvive">生きているか否か</param>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        private void RPC_CheckSurvive(bool isSurvive)
        {
            Debug.Log(this.Object.name + "CheckSurvive起動");
            if (isSurvive)//生き返ったとき
            {
                if (HasStateAuthority)
                {
                    _playerItem.ChangeCanUseItem(true);
                    if (!_playerMagic.GetUsedMagicBool())
                    {
                        _playerMagic.ChangeCanUseMagicBool(true);
                    }
                    _multiPlayerMove.RotateControl(true);
                }

                //今後蘇生関連の仕様が上がったら処理を実行させる
                _anim.SetBool("Survive", true);
                _deathEventCount = 0;
                _startReviveAnimation = true;
            }
            else //死んだとき
            {
                //死因に応じて死亡時アニメーションを変える
                if (_networkHealth <= 0)//体力が0で死んだとき
                {

                }
                else if (_networkSanValue <= 0)//SAN値が0で死んだとき
                {

                }

                if (HasStateAuthority)
                {
                    _playerMagic.ChangeCanUseMagicBool(false);
                    _playerItem.ChangeCanUseItem(false);
                    _multiPlayerMove.RotateControl(false);
                }
                _anim.SetBool("Survive", false);
                _anim.SetBool("FinishRevive", false);
                _deathEventCount = 0;
            }
        }

        /// <summary>
        /// アニメーションイベント。死亡時に実行
        /// </summary>
        private void DeathAnimationBoolChange()
        {
            _deathEventCount += 1;
            _startDeathAnimation = true;

            if (_deathEventCount == 2)//2回目のイベント時のみ実行
            {
                _startDeathAnimation = !_startDeathAnimation;
                _playerItem.CheckHaveDoll();
            }

        }

        /// <summary>
        /// 蘇生アニメーションが終了したことを知らせる関数
        /// </summary>
        private void FinishReviveAnimation()
        {
            _anim.SetBool("FinishRevive", true);
            _startReviveAnimation = false;
        }

        /// <summary>
        /// スタミナの減少を半減させるバフの状態を変化させるための関数
        /// </summary>
        public void SetStaminaBuff(bool value)
        {
            _isStaminaBuff = value;
        }

        /// <summary>
        ///  スタミナの減少を50%上昇させるバフの状態を変化させるための関数
        /// </summary>
        public void SetStaminaHealBuff(bool value)
        {
            _isStaminaHealBuff = value;
        }

        /// <summary>
        /// 光の距離を変化させる関数
        /// </summary>
        /// <param name="extendLightRange"></param>光の距離の伸ばすか否か判定する変数
        public void ChangeLightRange(bool extendLightRange)
        {
            _networkLightrange = (extendLightRange ? 80 : 20);
        }

        private void OnDestroy()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        public void SetStunStime(float stunTime) {    
            _netWorkeStanTime = stunTime;
            ChangeSpeed();
        }

        /// <summary>
        /// 最後に攻撃を行った敵を指定します
        /// </summary>
        /// <param name="id">最後に攻撃を行った敵のID</param>
        public void SetlastAtackEnemyId(NetworkId id) {
            _lastAtackEnemy = id;
        }
    }
}