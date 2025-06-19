using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Fusion;
using Common.Network;
using Common.UI;
using TMPro;

namespace Scenes.RoomSetting
{
    public class RoomSettingManager : MonoBehaviour
    {
        private static readonly int RANDOMKEY_MAX = 1000000; //IDの最大値

        [Header("Prefabs")]
        [SerializeField] private NetworkRunner _runnerPrefab;
        [SerializeField] private Dialog _dialog;
        [Header("Scene Objects")]
        [SerializeField] private GameObject _canvas;
        [SerializeField] private List<GameObject> _roomSettingPanels = new List<GameObject>();
        [Header("Parameter")]
        [SerializeField] private int _maxPlayer = 0; //最大人数
        [SerializeField] private int _sessionStartSceneIndex = 0; //セッションが開始するシーンのインデックス

        private int _panelIndex = 0;

        private async void Start()
        {
            await BootRunner(); //Runnerの起動
            SwitchPanel(1);
        }

        /// <summary>
        /// NetworkRunnerを起動する
        /// </summary>
        public async UniTask BootRunner()
        {
            var runner = Instantiate(_runnerPrefab); //Runnerインスタンスを設置
            runner.ProvideInput = true; //入植権限を使う

            var result = await runner.JoinSessionLobby(SessionLobby.ClientServer); //セッションロビーに参加（仮想的）
            if (result.Ok)
            {
                //Debug.Log("Join SessionLobby");
            }
            else
            {
                //Debug.LogError($"Error : {result.ShutdownReason}");
            }
        }

        /// <summary>
        /// NetworkRunnerを停止する
        /// </summary>
        public void DiscardRunner()
        {
            if (RunnerManager.Runner == null)
            {
                //Debug.LogError("Error : Not Found Runner");
            }
            else
            {
                //Debug.Log("Runner Shutdown");
                RunnerManager.Runner.Shutdown();
            }
        }

        /// <summary>
        /// パブリックセッションの作成
        /// </summary>
        public async void CreatePublicSession()
        {
            var args = new StartGameArgs()
            {
                GameMode = GameMode.Host, //セッション内の役割
                Scene = SceneRef.FromIndex(_sessionStartSceneIndex), //セッション開始時のシーンインデックス
                SceneManager = RunnerManager.Runner.GetComponent<NetworkSceneManagerDefault>(), //Runnerで使うシーン管理コンポーネント
                PlayerCount = _maxPlayer, //セッションの最大人数
                ConnectionToken = Guid.NewGuid().ToByteArray(), //プレイヤーの接続トークン（ホスト遷移で使う）
            };

            var result = await RunnerManager.Runner.StartGame(args); //セッション開始
            if (result.Ok == true)
            {
                //Debug.Log("Create Public Session");
            }
            else
            {
                //Debug.LogError($"Error : {result.ShutdownReason}");
            }
        }

        /// <summary>
        /// プライベートセッションの作成
        /// </summary>
        public async void CreatePrivateSession()
        {
            string sessionId = GetSessionId();

            var args = new StartGameArgs()
            {
                GameMode = GameMode.Host,
                Scene = SceneRef.FromIndex(_sessionStartSceneIndex),
                SceneManager = RunnerManager.Runner.GetComponent<NetworkSceneManagerDefault>(),
                SessionName = sessionId, //セッションID
                PlayerCount = _maxPlayer,
                ConnectionToken = Guid.NewGuid().ToByteArray(),
            };

            var result = await RunnerManager.Runner.StartGame(args); //セッション開始
            if (result.Ok == true)
            {
                //Debug.Log("Create Private Session");
                Debug.Log("Session Id : " + sessionId);
            }
            else
            {
                //Debug.LogError($"Error : {result.ShutdownReason}");
            }
        }

        /// <summary>
        /// パブリックセッションに参加
        /// </summary>
        public async void JoinPublicSession()
        {
            //空きのあるセッションを検索中
            bool sessionExist = false;
            foreach (var session in RunnerManager.Instance.SessionInfoList)
            {
                if (session.PlayerCount != _maxPlayer) sessionExist = true;
            }

            //セッションに空きがない
            if (sessionExist == false)
            {
                var dialog = Instantiate(_dialog, _canvas.transform);
                dialog.Init("ルームが見つかりませんでした。");
                return;
            }

            var args = new StartGameArgs()
            {
                GameMode = GameMode.Client,
                SceneManager = RunnerManager.Runner.GetComponent<NetworkSceneManagerDefault>(),
                ConnectionToken = Guid.NewGuid().ToByteArray(),
            };

            var result = await RunnerManager.Runner.StartGame(args);
            if (result.Ok == true)
            {
                //Debug.Log("Join Public Session");
            }
            else
            {
                //Debug.LogError($"Error : {result.ShutdownReason}");
            }
        }

        /// <summary>
        /// プライベートセッションに参加
        /// </summary>
        /// <param name="sessionId"></param>
        public async void JoinPrivateSession(TMP_InputField sessionId)
        {
            //セッション存在の確認
            var sessionExist = RunnerManager.Instance.SessionInfoList.FirstOrDefault(x => x.Name == sessionId.text);
            if (sessionExist == null) //セッションが見つからない
            {
                var dialog = Instantiate(_dialog, _canvas.transform);
                dialog.Init("ルームが見つかりませんでした。");
                return;
            }

            var args = new StartGameArgs()
            {
                GameMode = GameMode.Client,
                SceneManager = RunnerManager.Runner.GetComponent<NetworkSceneManagerDefault>(),
                SessionName = sessionId.text,
                ConnectionToken = Guid.NewGuid().ToByteArray(),
            };

            var result = await RunnerManager.Runner.StartGame(args);
            if (result.Ok == true)
            {
                //Debug.Log("Join Private Session");
            }
            else
            {
                //Debug.LogError($"Error : {result.ShutdownReason}");
            }
        }

        /// <summary>
        /// パネルUIの切り替え
        /// </summary>
        /// <param name="index"></param>
        public void SwitchPanel(int index)
        {
            if (index >= _roomSettingPanels.Count || index < 0) return;

            _roomSettingPanels[_panelIndex].SetActive(false);
            _roomSettingPanels[index].SetActive(true);
            _panelIndex = index;
        }

        /// <summary>
        /// プライベートセッションIDの作成
        /// </summary>
        /// <returns></returns>
        private string GetSessionId()
        {
            string sessionId; //セッションID

            while (true)
            {
                int num = UnityEngine.Random.Range(0, RANDOMKEY_MAX);
                string id = num.ToString("D6");

                var result = RunnerManager.Instance.SessionInfoList.FirstOrDefault(x => x.Name == id);
                if (result == null)
                {
                    sessionId = id;
                    break;
                }
            }

            return sessionId;
        }
    }
}
