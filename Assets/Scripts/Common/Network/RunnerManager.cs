using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Linq;

namespace Common.Network
{
    public class RunnerManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private NetworkObjectTable _networkObjectTable;

        public static RunnerManager Instance;
        public static NetworkRunner Runner;
        public List<SessionInfo> SessionInfoList = new List<SessionInfo>();

        private void Start()
        {
            if (Instance == null) Instance = this;
            else Destroy(this.gameObject);

            if (Runner == null) Runner = GetComponent<NetworkRunner>();
            else Destroy(this.gameObject);
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer == false) return;

            //プレイヤー情報の生成
            var playerInfo = runner.Spawn(_networkObjectTable.playerInfo, Vector3.zero, Quaternion.identity, player);
            if (runner.TryGetPlayerObject(player, out var _) == false)
            {
                runner.SetPlayerObject(player, playerInfo);
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer == false) return;

            //プレイヤー削除
            if (runner.TryGetPlayerObject(player, out var playerInfo) == true)
            {
                runner.Despawn(playerInfo);
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Runner = null;
            Instance = null;
        }

        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            SessionInfoList = new List<SessionInfo>(sessionList);
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            if (runner.IsServer == false) return;

            int index = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            var sceneManager = _networkObjectTable.sceneManager.FirstOrDefault(x => x.index == index).Object;

            runner.Spawn(sceneManager, Vector3.zero, Quaternion.identity, null);
        }

        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    }
}