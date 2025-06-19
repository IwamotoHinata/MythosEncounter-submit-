using System.Collections;
using System.Collections.Generic;
using Scenes.Ingame.Player;
using UnityEngine;
using UniRx;
using UnityEngine.AI;
using Fusion;

namespace Scenes.Ingame.Enemy
{
    /// <summary>
    /// パトロールする。プレイヤーの痕跡を探す。巡回状態と索敵状態の動きを決定し、追跡と攻撃状態への移行を行う。
    /// </summary>
    public class EnemySearch : NetworkBehaviour
    {
        protected EnemyVisibilityMap _myVisivilityMap;
        [SerializeField]
        protected float _checkRate;//何秒ごとに視界の状態をチェックするか
        protected float _checkTimeCount;//前回チェックしてからの時間を計測
        [SerializeField]
        protected bool _debugMode;
        [SerializeField]
        protected EnemyMove _myEnemyMove;
        protected NavMeshAgent _myAgent;


        [Tooltip("視界の長さ、今はマップ端から端まで見えるようにしています。小さくすると軽量化可能")]protected float _visivilityRange = 500;
        [SerializeField]
        protected EnemyStatus _enemyStatus;
        protected float _audiomaterPower;

        public EnemyVisibilityMap MyVisivillityMap { get => _myVisivilityMap; }

        //索敵行動のクラスです
        // Start is called before the first frame update
        void Start()
        {



        }

        /// <summary>
        /// 外部からこのスクリプトの初期設定をするために呼び出す
        /// </summary>
        public void Init(EnemyVisibilityMap setVisivilityMap)
        {
            _myVisivilityMap = setVisivilityMap;
            _myAgent = GetComponent<NavMeshAgent>();

            //スペックの初期設定
            _audiomaterPower = _enemyStatus.AudiometerPower;



            //スペックの変更を受け取る
            _enemyStatus.OnAudiometerPowerChange.Subscribe(x => { _audiomaterPower = x; }).AddTo(this);


            foreach (PlayerStatus playerStatus in _enemyStatus.MultiPlayManager.PlayerStatusList)
            {
                playerStatus.OnPlayerActionStateChangeEvent.Subscribe(x =>
                {
                    //プレイヤーの騒音を聞く
                    if (_enemyStatus.State == EnemyState.Patrolling || _enemyStatus.State == EnemyState.Searching)//時分が探しているときであれば
                    {
                        float valume = 0;
                        switch (x.nowPlayerActionState)
                        {
                            case PlayerActionState.Sneak:
                                valume = playerStatus.nowPlayerSneakVolume;
                                break;
                            case PlayerActionState.Walk:
                                valume = playerStatus.nowPlayerWalkVolume;
                                break;
                            case PlayerActionState.Dash:
                                valume = playerStatus.nowPlayerRunVolume;
                                break;
                        }
                        if (Mathf.Pow((float)(valume * _audiomaterPower * 0.01f), 2f) - (Mathf.Pow(transform.position.x - playerStatus.transform.position.x, 2) + (Mathf.Pow(transform.position.y - playerStatus.transform.position.y, 2))) > 0)
                        {//音が聞こえるかどうか
                            _myVisivilityMap.HearingSound(playerStatus.transform.position, 15, false);
                            _myEnemyMove.SetMovePosition(_myVisivilityMap.GetNextNearWatchPosition(this.transform.position));
                        }
                    }
                }).AddTo(this);
            }

        }


        public override void FixedUpdateNetwork()
        {
            if (_myVisivilityMap != null)//索敵の準備ができていない場合
            {
                Debug.LogError("マップ情報がありません、_myVisivilityMapを作成してください");
                return;
            }

            if (_enemyStatus.State == EnemyState.Patrolling || _enemyStatus.State == EnemyState.Searching)
            { //巡回状態または捜索状態の場合
                //定期的に視界情報を調べる
                _checkTimeCount += Runner.DeltaTime;
                if (_checkTimeCount > _checkRate)
                {
                    _checkTimeCount = 0;
                    //いろんなものを調べる。これは決定的なものほど優先して認識する
                    if (CheckCanSeeThePlayer())
                    {//プレイヤー自身が見えるか調べる
                    }
                    else if (CheckCanSeeTheLight())
                    {//プレイヤーの持つ光が見えるか調べる
                    }                    
                    else if (CheckCanHearThePlayerSound()) 
                    { //プレイヤーの騒音が聞こえるか調べる
                    }
                    else
                    {
                        //なんの痕跡も見つからなかった場合普通に巡回する
                        _myVisivilityMap.CheckVisivility(this.transform.position, _visivilityRange);
                        
                        if (_myEnemyMove.EndMove)//移動が終わっている場合
                        {
                            if (_debugMode) { Debug.Log(_myEnemyMove.GetMovePosition()); }
                            _myVisivilityMap.ChangeGridWatchNum(_myEnemyMove.GetMovePosition(), 1, true);
                            //痕跡のあった場所まで来たが何もいなかった場合ここが実行されるのでStatusを書き換える
                            _enemyStatus.SetEnemyState(EnemyState.Patrolling);
                            //あらたな移動先を取得するメソッドを書き込む
                            _myEnemyMove.SetMovePosition(_myVisivilityMap.GetNextNearWatchPosition(this.transform.position));
                            
                        }
                    }
                }
            }
        }

        /// <summary>
        /// プレイヤーを直接視認可能かどうかを調べる
        /// </summary>
        /// <returns></returns>
        protected bool CheckCanSeeThePlayer() {
            bool see = false;//見ることができた
            float approachRange = float.MaxValue;//追いかけるプレイヤーの距離
            Vector3 tgtPosition = _myEnemyMove.GetMovePosition();
            foreach (PlayerStatus playerStatus in _enemyStatus.MultiPlayManager.PlayerStatusList)
            {//プレイヤーごとの処理
                float range = Vector3.Magnitude(this.transform.position - playerStatus.transform.position);//平方根を求めるのはすごくコストが重いらしいので確実に計算が必要になってからしてます
                bool hit = true;
                Ray ray = new Ray(this.transform.position + new Vector3(0,2,0), playerStatus.transform.position - this.transform.position);
                hit = Physics.Raycast(ray, out RaycastHit hitInfo, range, -1 ^ LayerMask.GetMask(new string[] { "Ignore Raycast", "Player" }), QueryTriggerInteraction.Collide);
                if (!hit) 
                { //何にもあたっていない=対称まで直線的に視界が通る
                    SanCheck(playerStatus);
                    if (_visivilityRange > range) //見える距離内
                    {
                        if (true) { //視野角内部にプレイヤーがいるかどうか=今はずっとTrue
                            see = true;
                            if (_debugMode) { Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 5); Debug.Log("プレイヤー発見"); }
                            if (approachRange > range)
                            { //先ほどまでに直接見ることのできたプレイヤーより近いプレイヤーである場合
                                approachRange = range;
                                tgtPosition = playerStatus.transform.position;
                            }
                        }
                    }
                }
            }
            if (see)
            { //発見し追跡する場合
                
                _myEnemyMove.SetMovePosition(tgtPosition);
                _enemyStatus.SetEnemyState(EnemyState.Discover);
                return true;
            }
            else { 
                return false;
            }
        }

        protected bool CheckCanSeeTheLight() {
            Vector3 neerLightPosition = _myEnemyMove.GetMovePosition();
            bool canWatchLight = false;
            if (_enemyStatus.ReactToLight ) 
            {
                //そもそも光が見えるか調べる
                foreach (PlayerStatus playerStatus in _enemyStatus.MultiPlayManager.PlayerStatusList)
                {//プレイヤーごとの処理
                    Vector3 positionCandidate = Vector3.zero;
                    if (_myVisivilityMap.RightCheck(this.transform.position, playerStatus.transform.position, _visivilityRange, playerStatus.nowPlayerLightRange, ref  positionCandidate)) {
                        //対称のプレイヤーの光が見えるかどうか
                        if (canWatchLight) {//光が見えた
                            if ((this.transform.position - neerLightPosition).magnitude > (this.transform.position - positionCandidate).magnitude) {
                                neerLightPosition = positionCandidate;
                            }
                        }else{
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
                _enemyStatus.SetEnemyState(EnemyState.Searching);
                _myEnemyMove.SetMovePosition(neerLightPosition);
                return true;
            }
            else {            
            return false; 
            }
        }

        protected bool CheckCanHearThePlayerSound() {
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
                _enemyStatus.SetEnemyState(EnemyState.Searching);
                _myEnemyMove.SetMovePosition(_myVisivilityMap.GetNextNearWatchPosition(this.transform.position));
                return true;
            }
            else { return false; }
        }

        protected void SanCheck(PlayerStatus playerStatus) { 
        //#######################################プレイヤーの情報が必要です。
        }


    } 
}
