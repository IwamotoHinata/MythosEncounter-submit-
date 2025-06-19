using System.Collections;
using System.Collections.Generic;
using Scenes.Ingame.Player;
using System;
using UniRx;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Threading;
using Fusion;
using Scene.Ingame.GlobalSettings;

namespace Scenes.Ingame.Enemy
{
    /// <summary>
    /// 敵のスペックと現在の状態を記録する
    /// </summary>
    public class EnemyStatus : NetworkBehaviour
    {
        //Countはタイミングを計るような変数を表す。例えば..クールダウンがどれだけ終了しているかや何かい攻撃をしたかなど
        [Header("敵キャラの基本スペックの初期値")]
        [SerializeField][Tooltip("hpの初期値")] private int _hpBase;
        [SerializeField][Tooltip("巡回時の速度の初期値")] private float _patrollingSpeed;
        [SerializeField][Tooltip("索敵時の速度")] private float _searchSpeed;
        [SerializeField][Tooltip("追跡時の速度")] private float _chaseSpeed;
        [SerializeField][Tooltip("聴力の初期値。0は全く聞こえず100はどんな小さい音も聞き逃さない")][Range(0, 100)] private float _audiometerPowerBase;
        [SerializeField][Tooltip("光に反応するかどうかの初期値")] private bool _reactToLight;
        [SerializeField][Tooltip("飛行しているかどうかの初期値")] private bool _flying;
        [SerializeField][Tooltip("光でひるむかどうかの初期値")] private bool _flinchingInTheLight;
        [SerializeField][Tooltip("スタミナの初期値")] private int _staminaBase;
        [SerializeField][Tooltip("特殊行動のクールタイム")] private int _actionCoolTime;
        [SerializeField][Tooltip("初期のState")] private EnemyState _enemyStateBase;
        [SerializeField][Tooltip("足音の初期値")][Range(0, 1.0f)] private float _footSoundBase;
        [SerializeField][Tooltip("プレイヤーを見失った後何秒間はあきらめないか")] private float _blindCheseTime;
        [SerializeField][Tooltip("受けるスタン時間（秒）")] private float _stunTime;

        [Header("敵キャラの攻撃性能の初期値")]
        [SerializeField][Tooltip("Sanへの攻撃力")] private int _horror;

        [Header("その他")]
        [SerializeField][Tooltip("撤退時に落とすユニークアイテム")] private GameObject _uniqueItem;
        [SerializeField][Tooltip("敵の覚えている可能性のある呪文")] private List<EnemyMagic> _enemyMagics;
        [SerializeField][Tooltip("一個体の覚えている呪文")] private int _hasMagicNum;
        [SerializeField][Tooltip("退散から復帰するまでのミリ秒")] private int _fallBackTime;
        [SerializeField][Tooltip("見た目のゲームオブジェクト")] private GameObject _visual;
        [SerializeField][Tooltip("発見動作は何ミリ秒間続くのかどうか")] private int _discoverTime;
        [SerializeField][Tooltip("敵キャラクターのID")] private int _enemyId;


        [Header("自身についているであろうスクリプト")]
        [SerializeField] EnemySearch _enemySearch;
        [SerializeField] EnemyAttack _enemyAttack;
        [SerializeField] EnemyMove _enemyMove;
        [SerializeField] EnemyUniqueAction _enemyUniqueAction;




        [Header("デバッグするかどうか")]
        [SerializeField] private bool _debugMode;
        private bool _isCheckWaterEffect = false;//水の生成がされているか否か



        //########################
        //[NetWorked]置き場、注意！Fusionの仕様上getsetは独自実装してはならない！


        [HideInInspector][Networked] public float Speed { get;private set;}
        private Subject<float> _speedSubject = new Subject<float>();
        public IObservable<float> OnSpeedChanged
        {
            get { return _speedSubject; }
        }


        [HideInInspector][Networked] public int Hp { get;private set; }
        private Subject<int> _hpSubject = new Subject<int>();
        public IObservable<int> OnHpChanged
        {
            get { return _hpSubject; }
        }

        [HideInInspector][Networked] public float AudiometerPower { get; private set; }
        private Subject<float> _audiometerPowerSubject = new Subject<float>();
        public IObservable<float> OnAudiometerPowerChange
        {
            get { return _audiometerPowerSubject; }
        }

        [HideInInspector][Networked] public int Stamina { get; private set; }
        private Subject<int> _staminaSubject = new Subject<int>();
        public IObservable<int> OnStaminaChange
        {
            get { return _staminaSubject; }
        }

        [HideInInspector][Networked] public EnemyState State { get; private set; } = EnemyState.Patrolling;
        private Subject<EnemyState> _stateSubject = new Subject<EnemyState>();
        public IObservable<EnemyState> OnEnemyStateChange
        {
            get { return _stateSubject; }
        }

        [HideInInspector][Networked] public bool Bind { get; private set; }
        private Subject<bool> _bindSubject = new Subject<bool>();
        public IObservable<bool> OnBindChange
        {
            get { return _bindSubject; }
        }

        [HideInInspector][Networked] public float StiffnessTime { get; private set; }
        private Subject<float> _stiffnessTimeSubject = new Subject<float>();
        public IObservable<float> OnStiffnessTimeChange
        {
            get { return _stiffnessTimeSubject; }
        }

        [HideInInspector][Networked] public float SlowTime { get; private set; }
        private Subject<float> _slowTimeSubject = new Subject<float>();
        public IObservable<float> OnSlowTimeChange
        {
            get { return _slowTimeSubject; }
        }

        [HideInInspector][Networked] public bool IsWaterEffectDebuff { get; private set; }
        private Subject<bool> _isWaterEffectDebuffSubject = new Subject<bool>();
        public IObservable<bool> OnIsWaterEffectDebuffChange
        {
            get { return _isWaterEffectDebuffSubject; }
        }

        [HideInInspector][Networked] public bool StaminaOver { get; private set; }
        private Subject<bool> _staminaOverSubject = new Subject<bool>();
        public IObservable<bool> OnStaminaOverChange
        {
            get { return _staminaOverSubject; }
        }

        [HideInInspector][Networked] public float ForcedWalkingTime { get; private set; }
        private Subject<float> _forcedWalkingSubject = new Subject<float>();
        public IObservable<float> OnForcedWalkingChange
        {
            get { return _forcedWalkingSubject; }
        }


       


        [HideInInspector][Networked] private EnemySpawner _enemySpawner { get; set; }

        [HideInInspector][Networked] public MultiPlayManager MultiPlayManager { get; private set; }

        //##########GetとかSetのかたまり
        public float PatrollingSpeed { get { return _patrollingSpeed; } }
        public float SearchSpeed { get { return _searchSpeed; } }
        public float ChaseSpeed { get { return _chaseSpeed; } }
        public int StaminaBase { get { return _staminaBase; } }
        public bool ReactToLight { get { return _reactToLight; } }
        public int Horror { get { return _horror; } }
        public float BrindCheseTime { get { return _blindCheseTime; } }
        public int DiscoverTime { get { return _discoverTime; } }
        public int EnemyId { get { return _enemyId; } }
        public EnemyVisibilityMap MyEnemyVisivilityMap { get { return _myEnemyVisivilityMap; } }

        public CausesChromaticAberration CausesChromaticAberration { get { return _causesChromaticAberration; } }



        //##########UniRxにかかわらない変数
        private EnemyVisibilityMap _myEnemyVisivilityMap;
        private AudioSource _audioSource;

        private ChangeDetector _changeDetector;
        private CausesChromaticAberration _causesChromaticAberration;


        /// <summary>
        /// 初期設定をする。外部から呼び出すこととする
        /// </summary>
        public void Init(EnemySpawner spawner) {
            
            RPC_Init(spawner);
        }


        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_Init(EnemySpawner spawner)
        {
            //他所のデータを読み込んでゆく
            _enemySpawner = spawner;
            MultiPlayManager = _enemySpawner._multiPlayerManagerCs;
            //初期値を設定してゆく
            ResetStatus();


            //自身についているメソッドの初期化
            _enemyMove.Init();
            _enemySearch.Init(_myEnemyVisivilityMap);
            _enemyAttack.Init(_myEnemyVisivilityMap.DeepCopy());//Atackはサーチの後にInit          
            _enemyUniqueAction.Init(_actionCoolTime);

            //撃破されたことを検出
            OnHpChanged.Where(hp => hp <= 0).Subscribe(hp =>
            {
                FallBack();
            }).AddTo(this);

            ////////////
            //変更を検出する準備をする
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

            this.gameObject.TryGetComponent<AudioSource>(out _audioSource);
            _audioSource.volume = _footSoundBase;

            //エフェクトをかけるためのスクリプトを取得
            _causesChromaticAberration = (CausesChromaticAberration)FindObjectOfType(typeof(CausesChromaticAberration));
        }



        /*
         if (x)//拘束状態になった瞬間
                        _myAgent.speed *= 0.1f;
                    else//拘束状態が解けた瞬間
                        _myAgent.speed *= 10;
         */

        /// <summary>
        /// ネットワークのシミュレーションごとに呼び出される。ロールバック等にも対応
        /// </summary>
        public override void FixedUpdateNetwork()
        {
            if (_changeDetector == null) {
                Debug.Log("ChangeDetectorがヌルです");
                return;
            }
            //変更を検出しUniRxのイベントを発行す
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(Speed):
                        _speedSubject.OnNext(Speed);
                        break;
                    case nameof(Hp):
                        _hpSubject.OnNext(Hp);
                        break;
                    case nameof(AudiometerPower):
                        _audiometerPowerSubject.OnNext(AudiometerPower);
                        break;
                    case nameof(Stamina):
                        _staminaSubject.OnNext(Stamina);
                        break;
                    case nameof(State):
                        _stateSubject.OnNext(State);
                        break;
                    case nameof(Bind):
                        _bindSubject.OnNext(Bind);
                        break;
                    case nameof(StiffnessTime):
                        _stiffnessTimeSubject.OnNext(StiffnessTime);
                        break;
                    case nameof(SlowTime):
                        _slowTimeSubject.OnNext(SlowTime);
                        break;
                    case nameof(IsWaterEffectDebuff):
                        _isWaterEffectDebuffSubject.OnNext(IsWaterEffectDebuff);
                        break;
                    case nameof(StaminaOver):
                        _staminaOverSubject.OnNext(StaminaOver);
                        break;
                    case nameof(ForcedWalkingTime):
                        _forcedWalkingSubject.OnNext(ForcedWalkingTime);
                        break;
                }
            }
            if (_debugMode && Input.GetKey(KeyCode.Z)) { FallBack(); }
            if (_debugMode && Input.GetKey(KeyCode.N)) { AddLightBeam(); }
            if (StiffnessTime > 0) {
                StiffnessTime -= Runner.DeltaTime;
                if (StiffnessTime < 0)
                {
                    StiffnessTime = 0;
                }
            }

            if (SlowTime > 0)
            {
                SlowTime -= Time.deltaTime;
                if (SlowTime < 0)
                {
                    SlowTime = 0;
                }
            }
            if (ForcedWalkingTime > 0) { 
                ForcedWalkingTime -= Time.deltaTime;
                if (ForcedWalkingTime < 0) { 
                    ForcedWalkingTime = 0;
                }
            }

            //水の影響で自分の速度が下がるのか, 足音が大きくなるのかを確認
            if (_isCheckWaterEffect)
            {
                if (_flying)//飛翔状態の時は影響を受けない
                {
                    IsWaterEffectDebuff = false;
                }
                else //飛翔状態でない時は影響を受ける
                {
                    IsWaterEffectDebuff = true;                   
                }

                //足音の大きさを変更
                _audioSource.volume = _footSoundBase * (IsWaterEffectDebuff ? 1.5f : 1);
            }

        }

        public void SetEnemyState(EnemyState state) {
            if (HasStateAuthority)
            {//Stateの変更は予期せぬ動作やアニメーションにかかわるのでホストのみが行う
                State = state;
                if (_debugMode) { Debug.Log("State変更" + State); }
            }
        }

        /// <summary>
        /// ステータスのみ初期化する
        /// </summary>
        public void ResetStatus() {
            Hp = _hpBase;
            AudiometerPower = _audiometerPowerBase;
            Stamina = _staminaBase;
            State = _enemyStateBase;
            _myEnemyVisivilityMap = _enemySpawner.GetEnemyVisivilityMap().DeepCopy();

        }

        /// <summary>
        /// 攻撃を加えるために使用する
        /// </summary>
        /// <param name="damage">与えるダメージ</param>
        public void AddDamage(int damage) {
            Hp -= damage;
        }

        /// <summary>
        /// 移動速度を指定した値に設定する
        /// </summary>
        /// <param name="value">設定する速度</param>
        public void SetSpeed(float value) { { Speed = value; } }

        /// <summary>
        /// 退散させるために使用する
        /// </summary>
        public void FallBack() { 
            //機能を停止
            _enemyAttack.enabled = false;
            _enemyMove.enabled = false; 
            _enemySearch.enabled = false;
            _visual.active = false;
            GameObject.Instantiate(_uniqueItem,this.transform.position,Quaternion.identity);
            Debug.Log(this.gameObject.name + "退散しました！");
            ReMap(this.GetCancellationTokenOnDestroy()).Forget();
        }

        /// <summary>
        /// スタミナの値を書き換えるのに使用する
        /// </summary>
        /// <param name="changeStamina">書き換えるスタミナの値</param>
        public void StaminaChange(int changeStamina) { 
            Stamina = changeStamina;
        }

        private async Cysharp.Threading.Tasks.UniTaskVoid ReMap(CancellationToken ct)
        {
            await Task.Delay(_fallBackTime,ct);
            //機能を停止
            _enemyAttack.enabled = true;
            _enemyMove.enabled = true;
            _enemySearch.enabled = true;
            _visual.active = true;
        }

        public void SetBind(bool value)
        {
            Bind = value;
        }

        public void SetStuminaOver(bool setValue) { 
            StaminaOver = setValue;
        }
        /// <summary>
        /// 硬直時間を変化さえる
        /// </summary>
        /// <param name="value">変化する値</param>
        public void ChangeStiffnessTime(float value) { 
            StiffnessTime += value;
        }

        /// <summary>
        /// 水の影響を変更する
        /// </summary>
        /// <param name="value">水の影響があるときはtrue無いときはfalse</param>
        public void SetCheckWaterEffect(bool value)
        { 
            _isCheckWaterEffect = value;
            if (_isCheckWaterEffect)
            {
                if (_flying)//飛翔状態の時は影響を受けない
                {
                    IsWaterEffectDebuff = false;
                }
                else //飛翔状態でない時は影響を受ける
                {
                    IsWaterEffectDebuff = true;
                }
            }
            else //水の生成が終わったときに、各変数を初期値に戻す
            {
                IsWaterEffectDebuff = false; 
                _audioSource.volume = _footSoundBase;
            }
        }

        /// <summary>
        /// 速度を落とすのに使用してください
        /// </summary>
        /// <param name="addTime"></param>
        public void AddSlowTime(float addTime) {
            SlowTime += addTime;
        }

        /// <summary>
        /// ライトを当てることで強制的に歩かせるのに用いてください
        /// </summary>
        public void AddLightBeam() {
            if (_flinchingInTheLight){
                ForcedWalkingTime = 1.1f;
            }
        }

        public async UniTaskVoid ApplyStun()
        {
            SetEnemyState(EnemyState.Stun);
            SoundManager.Instance.PlaySe("se_screaming00", transform.position);
            await UniTask.Delay(TimeSpan.FromSeconds(_stunTime));
            SetEnemyState(EnemyState.Searching);
        }
    }
}