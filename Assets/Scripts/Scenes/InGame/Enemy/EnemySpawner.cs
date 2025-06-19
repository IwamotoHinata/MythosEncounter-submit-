using System.Collections;
using System.Collections.Generic;
using Scenes.Ingame.Manager;
using Scenes.Ingame.Player;
using UnityEngine;
using UniRx;
using Scenes.Ingame.InGameSystem;
using Cysharp.Threading.Tasks;
using System.Threading;
using Scenes.Ingame.Stage;
using Fusion;




namespace Scenes.Ingame.Enemy
{
    /// <summary>
    /// 敵キャラを作成する
    /// </summary>
    public class EnemySpawner : NetworkBehaviour
    {
        public static EnemySpawner Instance;

        [Header("デバッグ関連")]
        [SerializeField] private bool _debugMode;
        [SerializeField][Tooltip("InGameManager無しで機能させるかどうか")] private bool _nonInGameManagerMode;
        [SerializeField][Tooltip("最初に敵の抽選をするかどうか")] private bool _raffle = true;//デフォルトではオン
       
        [Header("各種設定")]
        [SerializeField][Tooltip("作成する敵")] private EnemyName _enemyName;
        [SerializeField][Tooltip("ゲームの進行度合い、0の場合設定なしとして全ての敵を抽選させる")] private byte _gamePhase;

        [Header("マップの設定")]
        [SerializeField]
        [Tooltip("自動で生成されるので挿入しない事")]
        private EnemyVisibilityMap _enemyVisibilityMap;
        [SerializeField]
        [Tooltip("各マス目の数")]
        private byte _x, _y, _z;
        [SerializeField]
        [Tooltip("マップのマス目の幅")]
        private float _range;
        [SerializeField]
        [Tooltip("最も視界の長い敵の視界の距離")]
        private float _maxVisiviilityRange;
        [SerializeField]
        [Tooltip("マップのマス目の最も左下のマス目の中心部")]
        private Vector3 _centerPosition;

        [Header("作成する敵のプレハブ一覧")]
        [SerializeField] private GameObject _testEnemy;
        [SerializeField] private GameObject _deepOnes;
        [SerializeField] private GameObject _spawnOfCthulhu;
        [SerializeField] private GameObject _Dagon;
        [SerializeField] private GameObject _miGo;
        [SerializeField] private GameObject _rhanTegoth;
        [SerializeField] private GameObject _subNiggurath;
        [SerializeField] private GameObject _hasture;



        [Header("生成する際の設定")]
        [SerializeField] private Vector3 _enemySpawnPosition;
        [SerializeField] private GameObject _playerSpawner;

        public MultiPlayManager _multiPlayerManagerCs { get; private set; }//なぁぜぇかぁ直接シリアライズできない（メソッド扱い？でもこれ表示されるケースもあるんだよなぁ...）


        /*
        [Header("プレイヤー一覧")]
        [SerializeField] private List<GameObject> _players;
        */

        private List<StageDoor> _doors = new List<StageDoor>();

        private CancellationTokenSource _cancellationTokenSource;

        // Start is called before the first frame update
        async void Start()
        { _multiPlayerManagerCs = _playerSpawner.GetComponent<MultiPlayManager>();
            _cancellationTokenSource = new CancellationTokenSource();
            if (_nonInGameManagerMode)
            {
                InitialSpawn(_cancellationTokenSource.Token).Forget();
            }
            else
            {
                IngameManager.Instance.OnPlayerSpawnEvent.Subscribe(_ => InitialSpawn(_cancellationTokenSource.Token).Forget());//プレイヤースポーンはマップが完成してから行われる
            }

            if (Instance == null)
                Instance = this;
            else
                Destroy(this.gameObject);
        }



        private async UniTaskVoid InitialSpawn(CancellationToken token)
        {
            //ドアを入手
            _doors = new List<StageDoor>(FindObjectsOfType<StageDoor>());

            //全てのドアが動き終わったか確認する
            bool stop = false;
            while (!stop)
            {
                stop = true;
                //全てのドアが動き終わったか確認する
                for (int i = 0; i < _doors.Count; i++)
                {
                    if (_doors[i].ReturnIsAnimation)
                    {
                        stop = false;
                    }
                    if (!stop) await UniTask.Delay(100, cancellationToken: token);


                }
            }

            /*      なぜこれが上手くいかんのだ！？
            //全てのドアが動き終わったか確認する
            for (int i = 0; i < _doors.Count; i++)
            {
                Debug.Log("ここまで2");
                await UniTask.WaitWhile(() => !_doors[i].ReturnIsAnimation);
                Debug.Log("ここまで3");
            }
            */

            //全てのドアを閉める
            for (int i = 0; i < _doors.Count; i++)
            {
                _doors[i].ChangeDoorQuickOpen(false);
            }

            //全てのドアが動き終わったか確認する
            stop = false;
            while (!stop)
            {
                stop = true;
                //全てのドアが動き終わったか確認する
                for (int i = 0; i < _doors.Count; i++)
                {
                    if (_doors[i].ReturnIsAnimation)
                    {
                        stop = false;
                    }
                    if (!stop) await UniTask.Delay(100, cancellationToken: token);
                }
            }

            //マップをスキャン
            _enemyVisibilityMap = new EnemyVisibilityMap();
            _enemyVisibilityMap.debugMode = _debugMode;
            _enemyVisibilityMap.maxVisivilityRange = _maxVisiviilityRange;
            _enemyVisibilityMap.GridMake(_x, _y, _z, _range, _centerPosition);
            _enemyVisibilityMap.MapScan();


            //_doors[0].gameObject.transform.position = _doors[0].gameObject.transform.position + new Vector3(5,0,5);


            //コライダーの更新を待つ
            await UniTask.DelayFrame(2, PlayerLoopTiming.FixedUpdate, token);
            _enemyVisibilityMap.NeedOpenDoorScan();


            //全てのドアを開ける
            for (int i = 0; i < _doors.Count; i++)
            {
                _doors[i].ChangeDoorQuickOpen(true);
            }

            //全てのドアが動き終わったか確認する
            stop = false;
            while (!stop)
            {
                stop = true;
                //全てのドアが動き終わったか確認する
                for (int i = 0; i < _doors.Count; i++)
                {
                    if (_doors[i].ReturnIsAnimation)
                    {
                        stop = false;
                        await UniTask.Delay(100, cancellationToken: token);
                    }
                }
            }
            //コライダーの更新を待つ
            await UniTask.DelayFrame(2, PlayerLoopTiming.FixedUpdate, token);
            _enemyVisibilityMap.NeedCloseDoorScan();


            //全てのドアを初期状態にする
            for (int i = 0; i < _doors.Count; i++)
            {
                _doors[i].ChangeDoorInitial();
            }

            if (_raffle)
            { //InGameManagerが存在する場合
                switch (_gamePhase)
                {
                    case 0:
                        switch (Random.Range(0, 3))
                        {
                            case 0:
                                _enemyName = EnemyName.DeepOnes;
                                break;
                            case 1:
                                _enemyName = EnemyName.MiGo;
                                break;
                            case 2:
                                _enemyName = EnemyName.SpawnOfCthulhu;
                                break;
                        }
                        if (_debugMode) Debug.Log("TestEnemyを除く全ての敵を抽選します抽選結果は" + _enemyName);
                        break;
                    case 1:
                        switch (Random.Range(0, 3))
                        {
                            case 0:
                                _enemyName = EnemyName.DeepOnes;
                                break;
                            case 1:
                                _enemyName = EnemyName.SubNiggurath;
                                break;
                            case 2:
                                _enemyName = EnemyName.RhanTegoth;
                                break;
                        }
                        if (_debugMode) Debug.Log("深き者ども、ミゴ、落とし子から抽選します抽選結果は" + _enemyName);
                        break;
                    case 2:
                        switch (Random.Range(0, 3))
                        {
                            case 0:
                                _enemyName = EnemyName.Dagon;
                                break;
                            case 1:
                                _enemyName = EnemyName.Dagon;
                                break;
                            case 2:
                                _enemyName = EnemyName.Dagon;
                                break;
                        }
                        break;
                    case 3:
                        switch (Random.Range(0, 3))
                        {
                            case 0:
                                _enemyName = EnemyName.Hastur;
                                break;
                            case 1:
                                _enemyName = EnemyName.Hastur;
                                break;
                            case 2:
                                _enemyName = EnemyName.Hastur;
                                break;
                        }
                        if (_debugMode) Debug.Log("ハスター、ニャルラトホテプ、ヨグソトースから抽選します" + _enemyName);
                        break;
                    default:
                        Debug.LogError("想定外のゲームフェーズです");
                        break;
                }
            }
            if (_nonInGameManagerMode)
            {
                Debug.Log("必要な数のプレイヤーが沸いた場合、Xキーを押してください");
                while (true) {
                    await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.X), cancellationToken: token);
                    break;
                }
                EnemySpawn(_enemyName, _enemySpawnPosition);

            }
            else
            {
                //ここでEnemy制作
                EnemySpawn(_enemyName, _enemySpawnPosition);
                //敵の沸きが完了したことを知らせる
                IngameManager.Instance.SetReady(ReadyEnum.EnemyReady);
            }
        }



        public GameObject EnemySpawn(EnemyName enemeyName, Vector3 spownPosition)//位置を指定してスポーンさせたい場合
        {
            GameObject createEnemy;
            EnemyStatus createEnemyStatus;
            EnemyVisibilityMap createEnemyVisiviityMap = _enemyVisibilityMap.DeepCopy();
            switch (enemeyName)
            {

                case EnemyName.TestEnemy:
                    createEnemy = Runner.Spawn(_testEnemy, spownPosition, Quaternion.identity).gameObject;
                    if (_debugMode) Debug.Log("テストエネミーは制作されました");
                    break;
                case EnemyName.DeepOnes:
                    createEnemy = Runner.Spawn(_deepOnes, spownPosition, Quaternion.identity).gameObject;
                    if (_debugMode) Debug.Log("深き者どもは制作されました");
                    break;
                case EnemyName.SpawnOfCthulhu:
                    createEnemy = Runner.Spawn(_spawnOfCthulhu, spownPosition, Quaternion.identity).gameObject;
                    if (_debugMode) Debug.Log("クトゥルフ落とし子は制作されました");
                    break;
                case EnemyName.MiGo:
                    createEnemy = Runner.Spawn(_miGo, spownPosition, Quaternion.identity).gameObject;
                    if (_debugMode) Debug.Log("ミゴは制作されました");
                    break;
                case EnemyName.Dagon:
                    createEnemy = Runner.Spawn(_Dagon, spownPosition, Quaternion.identity).gameObject;
                    if (_debugMode) Debug.Log("ダゴンは制作されました");
                    break;
                case EnemyName.RhanTegoth:
                    createEnemy = Runner.Spawn(_rhanTegoth, spownPosition, Quaternion.identity).gameObject;
                    if (_debugMode) Debug.Log("エネミーは制作されました");
                    break;
                case EnemyName.SubNiggurath:
                    createEnemy = Runner.Spawn(_subNiggurath, spownPosition, Quaternion.identity).gameObject;
                    if (_debugMode) Debug.Log("エネミーは制作されました");
                    break;
                case EnemyName.Hastur:
                    createEnemy = Runner.Spawn(_hasture, spownPosition, Quaternion.identity).gameObject;
                    if (_debugMode) Debug.Log("エネミーは制作されました");
                    break;
                default:
                    Debug.LogError("このスクリプトに、すべての敵のプレハブが格納可能かを確認してください");
                    return null;
            }
            if (createEnemy.TryGetComponent<EnemyStatus>(out createEnemyStatus))
            {
                if (_debugMode) Debug.Log("作成した敵にはEnemyStatusクラスがあります");
                createEnemyVisiviityMap.DontApproachPlayer();
                createEnemyStatus.Init(this);

            }
            return createEnemy;

        }

        /// <summary>
        /// ゲームの進行度合いをセットする
        /// </summary>
        /// <param name="value">ゲームの進行度合い</param>
        private void SetGamePhase(byte value) { 
            _gamePhase = value;
        }

        private void OnDestroy()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        public  EnemyVisibilityMap GetEnemyVisivilityMap() {
            return _enemyVisibilityMap;
        }


    }
}