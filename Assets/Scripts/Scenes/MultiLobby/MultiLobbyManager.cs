using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
using Common.Network.SceneManager;
using Scenes.MultiLobby.Player;

namespace Scenes.MultiLobby
{
    public class MultiLobbyManager : NetworkBehaviour, ISpawnAccess
    {
        [SerializeField] private int _inGameSceneIndex = 0;
        [Header("Player Spawn")]
        [SerializeField] private NetworkObject _playerObject;
        [SerializeField] private Vector3 _spawnPosition;
        [SerializeField] private Quaternion _spawnRotation;

        public NetworkObject GetPrefab()
        {
            return _playerObject;
        }

        public Vector3 GetPosition()
        {
            return _spawnPosition;
        }

        public Quaternion GetRotation()
        {
            return _spawnRotation;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void Rpc_StartGame()
        {
            var players = FindObjectsByType<MultiLobbyPlayer>(FindObjectsSortMode.None);

            bool allReady = true;
            foreach (var player in players)
            {
                allReady = player.isReady;
            }

            if (allReady == true)
            {
                Debug.Log("To InGameScene");
                Runner.LoadScene(SceneRef.FromIndex(_inGameSceneIndex));
            }
        }
    }
}
