using System.Collections;
using System.Collections.Generic;
using Scenes.Ingame.Player;
using UnityEngine;
using Fusion;
using System;
using UniRx;

namespace Scenes.Ingame.Enemy
{
    /// <summary>
    /// 敵キャラの攻撃を管理する。追跡状態と攻撃状態はこのスクリプトが作動する
    /// </summary>
    public class EnemyAttack : NetworkBehaviour
    {
        [Header("このスクリプトを制御する変数")]
        [SerializeField][Tooltip("何秒ごとに視界の状態、攻撃可能性、SANをチェックするか")] private float _checkRate;
        [Tooltip("戦闘時の視界の広さ、マップ端から端まで見える値で固定中")] private float _visivilityRange = 500;
       
        [SerializeField][Tooltip("デバッグするかどうか")] private bool _debugMode;



        private int _horror;



        [Header("攻撃動作")]
        [SerializeField][Tooltip("攻撃スクリプトたち、射程の短いものから長いものの順番で入れてください")] private List<EnemyAttackBehaviour> _enemyAttackBehaviours;
        //private float _massSUM;
        private float _atackRange;//最も射程の長い攻撃
        private float _massSUM;//使用可能な攻撃の重み付け
        



        [Header("自身についているメソッド")]
        [SerializeField] private EnemyStatus _enemyStatus;
        [SerializeField] private EnemySearch _enemySearch;
        [SerializeField] private EnemyMove _myEnemyMove;

        //##########内部で使う変数##########

        private float _audiomaterPower;//聞く力
        private float _checkTimeCount;//前回チェックしてからの時間を計測
        
        private EnemyVisibilityMap _myVisivilityMap;
        Vector3 nextPositionCandidate = new Vector3(0, 0, 0);
        private Camera _camera;
        private float _blindChaseTime;

        private Subject<Unit> _playerKillEvent = new Subject<Unit>();
        public IObservable<Unit> OnPlayerKill
        {
            get { return _playerKillEvent; }
        }


        [Networked] private float _blindChaseTimeCount { get; set; }

        public EnemyVisibilityMap MyVisivillityMap { get => _myVisivilityMap; }

        public List<EnemyAttackBehaviour> GetEnemyAtackBehaviours (){
            return _enemyAttackBehaviours;
        }

        public void SetEnemyAtackBehaviours(List<EnemyAttackBehaviour> setList) { 
            _enemyAttackBehaviours = setList;
        }


        /// <summary>
        /// 初期化処理、外部からアクセスする
        /// </summary>
        public void Init(EnemyVisibilityMap setVisivilityMap) {
            _myVisivilityMap = setVisivilityMap;
            _horror = _enemyStatus.Horror;
            _audiomaterPower = _enemyStatus.AudiometerPower;
            _blindChaseTime = _enemyStatus.BrindCheseTime;

            //_camera = GameObject.Find("Main Camera").GetComponent<Camera>();

            foreach(EnemyAttackBehaviour behaviour  in _enemyAttackBehaviours)
            {
                behaviour.Init();
            }

            /*
             #####################射程順に並び変える
             */
            _atackRange = _enemyAttackBehaviours[_enemyAttackBehaviours.Count -1].GetRange();


            //if(_debugMode)_playerStatus.OnEnemyAttackedMe.Subscribe(_ => Debug.Log("攻撃された"));

            _enemyStatus.OnEnemyStateChange.Subscribe(x => {
                if (x == EnemyState.Discover) 
                {
                    _enemyStatus.CausesChromaticAberration.AddIntensity();
                }
            }).AddTo(this);

        }

        // Update is called once per frame
        public override void FixedUpdateNetwork()
        {
            if (_enemyStatus.State == EnemyState.Chase || _enemyStatus.State == EnemyState.Attack || _enemyStatus.State == EnemyState.Discover)//メモ、Discover中は移動先の変更などはするが、Stateの変更や攻撃はしない。移動速度（Discover中は移動しない）についてはEnemyMoveが行ってくれる
            { //追跡状態または攻撃状態の場合
              //定期的に状態を変更
                _checkTimeCount += Runner.DeltaTime;
                if (EnemyState.Discover == _enemyStatus.State)//発見動作
                {//発見動作をする場合
                    if (_checkTimeCount > _enemyStatus.DiscoverTime) {
                        SoundManager.Instance.PlaySe("se_screaming00",transform.position);
                        _myVisivilityMap.SetEveryGridWatchNum(50);//リセット
                        _enemyStatus.SetEnemyState(EnemyState.Chase);
                        _blindChaseTimeCount = 0;
                    }
                    return;
                }
                if (_checkTimeCount > _checkRate)
                {
                    _checkTimeCount = 0;
                    if (CheckCanSeeThePlayer()) //敵が見えるルートがあるかかどうかを確認する
                    {
                        _blindChaseTimeCount = 0;
                    }
                    else
                    { //敵が見えないならせめてなんとかいそうなエリアへ行こうとする
                        _blindChaseTimeCount += _checkRate;
                        if (_blindChaseTimeCount > _blindChaseTime)
                        { //あきらめるかどうかの判定
                            if (_enemyStatus.State != EnemyState.Discover) 
                            { 
                                _enemyStatus.SetEnemyState(EnemyState.Searching);//追っかけるのあきらめた
                            }                                
                        }
                        else
                        { //まだあきらめない場合、近距離に特化したのSearchを行う
                            if (_enemyStatus.State != EnemyState.Discover)
                            {
                                _enemyStatus.SetEnemyState(EnemyState.Chase);
                            }
                            if (CheckCanSeeTheLight())//&&は左から評価される事に注意
                            { //光が見えるか調べる
                                if (_debugMode) Debug.Log("追跡中光が見えた");
                            }
                            else if (_myEnemyMove.EndMove)
                            { //移動が終了している場合
                                if (CheckCanHearThePlayerSound())//忍音が聞こえるかどうか
                                {

                                    if (_debugMode) Debug.Log("追跡中忍ぶ音が聞こえる");

                                }
                                else
                                {
                                    //なんの痕跡も見つからなかった場合普通に巡回する
                                    _myVisivilityMap.CheckVisivility(this.transform.position, _visivilityRange);
                                    if (_myEnemyMove.EndMove)//移動が終わっている場合
                                    {
                                        _myVisivilityMap.ChangeGridWatchNum(_myEnemyMove.GetMovePosition(), 1, true);
                                        //あらたな移動先を取得するメソッドを書き込む
                                        _myEnemyMove.SetMovePosition(_myVisivilityMap.GetNextNearWatchPosition(this.transform.position));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected virtual bool CheckCanSeeThePlayer()
        {//キャラクターによって視界の角度判定つける？ミゴは360°、深き者どもは270°、一般的な人間の狂信者とかなら180°とか...
            float _playerDistance = float.MaxValue;
            PlayerStatus neerPlayerStatus = null;
            bool canWatch = false;
            foreach (PlayerStatus playerStatus in _enemyStatus.MultiPlayManager.PlayerStatusList)
            { //全てのプレイヤーをチェック
                bool hit = true;//あたったかどうか
                float checkPlayerDistance = Vector3.Magnitude(this.transform.position - playerStatus.transform.position); //プレイヤーまでの距離
                Ray ray = new Ray(this.transform.position + new Vector3(0, 2, 0), playerStatus.transform.position - this.transform.position);
                hit = Physics.Raycast(ray, out RaycastHit hitInfo, checkPlayerDistance, -1 ^ LayerMask.GetMask(new string[] { "Ignore Raycast", "Player" }), QueryTriggerInteraction.Collide);
                if (!hit)
                { //何にもあたっていない=対称までの間に障害物はないを見ることが来た
                    SanCheck(playerStatus);//こちらまでプレイヤーはまっすぐ見れる
                    if (true) 
                    { //視野角内にプレイヤーはいるかどうか?＝現状は常にTrue
                        if (checkPlayerDistance < _visivilityRange) {//見える距離内にいるかどうか
                            canWatch = true;
                            if (_debugMode) { Debug.DrawRay(ray.origin, ray.direction * checkPlayerDistance, Color.red, 3); Debug.Log("プレイヤー発見"); }

                            if (checkPlayerDistance < _playerDistance)//最も近い対象であるのか？
                            {
                                _playerDistance = checkPlayerDistance;
                                neerPlayerStatus = playerStatus;
                            }
                        }

                    }
                }
            }

            if (canWatch)
            { //プレイヤーが存在する場合
              //こちらがなんらかのプレイヤーを見れるならSanチェックしてしまおう
                _myVisivilityMap.ChangeEveryGridWatchNum(1, true);
                _myVisivilityMap.SetGridWatchNum(neerPlayerStatus.transform.position, 0);
                _blindChaseTimeCount = 0;//見えたのであきらめるまでのカウントはリセット
                                         //移動目標をプレイヤーの座標にする
                _myEnemyMove.SetMovePosition(neerPlayerStatus.transform.position);

                if (_enemyStatus.State != EnemyState.Discover)
                {//発見動作中は攻撃したりしない
                    if (_atackRange > _playerDistance )
                    { //攻撃可能範囲であれば
                        _enemyStatus.SetEnemyState(EnemyState.Attack);
                        if (_enemyStatus.StiffnessTime <= 0) 
                        { //硬直していなければ
                            if (HasStateAuthority)
                            {//攻撃のスクリプトを叩くのはホストのみ
                                _massSUM = 0;
                                for (int i = 0; i < _enemyAttackBehaviours.Count; i++)
                                {
                                    if (_enemyAttackBehaviours[i].GetRange() > _playerDistance)
                                    {
                                        _massSUM += _enemyAttackBehaviours[i].GetMass();
                                    }
                                }
                                float _pickNum = UnityEngine.Random.RandomRange(0f, _massSUM);
                                for (int i = 0; i < _enemyAttackBehaviours.Count; i++)
                                {
                                    _massSUM -= _enemyAttackBehaviours[i].GetMass();
                                    if (_massSUM <= _pickNum)
                                    {
                                        _enemyStatus.ChangeStiffnessTime(_enemyAttackBehaviours[i].GetStiffness());
                                        _enemyAttackBehaviours[i].Behaviour(neerPlayerStatus);                                     
                                        break;
                                    }
                                }
                            }
                        }                      
                    }
                    else
                    {//攻撃できないなら追いかける
                        _enemyStatus.SetEnemyState(EnemyState.Chase);
                        _massSUM = 0;

                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool CheckCanSeeTheLight()
        {
            Vector3 neerLightPosition = _myEnemyMove.GetMovePosition();//最も近い光のとこ
            bool canWatchLight = false;
            if (_enemyStatus.ReactToLight)
            {//そもそも光が見えるか調べる
                foreach (PlayerStatus playerStatus in _enemyStatus.MultiPlayManager.PlayerStatusList)
                {//プレイヤーごとの処理
                    Vector3 positionCandidate = Vector3.zero;//候補として使う
                    if (_myVisivilityMap.RightCheck(this.transform.position, playerStatus.transform.position, _visivilityRange, playerStatus.nowPlayerLightRange, ref positionCandidate))
                    {
                        //対称のプレイヤーの光が見えるかどうか
                        if (canWatchLight)
                        {
                            if ((this.transform.position - neerLightPosition).magnitude > (this.transform.position - positionCandidate).magnitude)
                            {
                                neerLightPosition = positionCandidate;
                            }
                        }
                        else
                        {
                            canWatchLight = true;
                            neerLightPosition = positionCandidate;
                        }
                    }
                }
            }
            if (canWatchLight)
            {
                //光が見えるか調べる
                if (_debugMode) Debug.Log("光が見えた");
                _enemyStatus.SetEnemyState(EnemyState.Chase);
                _myEnemyMove.SetMovePosition(neerLightPosition);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool CheckCanHearThePlayerSound()
        {
            bool canHear = false;
            foreach (PlayerStatus playerStatus in _enemyStatus.MultiPlayManager.PlayerStatusList)
            {
                float valume = 0;//プレイヤーの騒音を記録
                switch (playerStatus.nowPlayerActionState)
                {
                    case PlayerActionState.Sneak:
                        valume = playerStatus.nowPlayerSneakVolume;
                        if (_debugMode) Debug.Log("忍ぶ音が聞こえる");
                        break;
                    case PlayerActionState.Walk:
                        valume = playerStatus.nowPlayerWalkVolume;
                        if (_debugMode) Debug.Log("歩く音が聞こえる");
                        break;
                    case PlayerActionState.Dash:
                        valume = playerStatus.nowPlayerRunVolume;
                        if (_debugMode) Debug.Log("走る音が聞こえる");
                        break;
                }
                if (Mathf.Pow((float)(valume * _audiomaterPower * 0.01f), 2f) - (Mathf.Pow(transform.position.x - playerStatus.transform.position.x, 2) + (Mathf.Pow(transform.position.y - playerStatus.transform.position.y, 2))) > 0)
                {
                    _myVisivilityMap.HearingSound(playerStatus.transform.position, 15, true);
                    canHear = true;
                }
            }
            if (canHear)
            {
                _enemyStatus.SetEnemyState(EnemyState.Chase);
                _myEnemyMove.SetMovePosition(_myVisivilityMap.GetNextNearWatchPosition(this.transform.position));
                return true;
            }
            else { return false; }
        }

        protected virtual void SanCheck(PlayerStatus playerStatus) {
            /*
             ################################################### 各プレイヤーごとのカメラ判定が必要です。向きとか視野角とかいわもとくんまかせたー
            Vector3 ScreenPosition = _camera.WorldToScreenPoint(this.transform.position);
            //Debug.Log(ScreenPosition);
            if (ScreenPosition.x > 0 && ScreenPosition.x < 1920) {
                if (ScreenPosition.y > 0 && ScreenPosition.y < 1080) {
                    if (ScreenPosition.z > 0)
                    {
                        _playerStatus.ChangeSanValue(_horror, ChangeValueMode.Damage);
                    }
                }
            }
            */
        }

        public void SendPlayerKillEvent() {
            _playerKillEvent.OnNext(Unit.Default);
        }

    }
}
