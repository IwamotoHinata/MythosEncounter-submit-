using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Scenes.Ingame.Stage;
using Scenes.Ingame.Manager;
using Scenes.Ingame.InGameSystem.UI;

namespace Scenes.Ingame.Player
{
    public class MultiPlayManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {
        [SerializeField] GameObject[] _spawnPoint;
        [SerializeField] GameObject _playerPrefab;
        [SerializeField] private bool _isIngame;

        
        [Networked][Capacity(4)]
        public NetworkLinkedList<NetworkId> playerNetworkList { get;}
        


        private ChangeDetector _changeDetector;
        public List<GameObject> PlayerList { get; private set; } = new List<GameObject>();
        public List<PlayerStatus> PlayerStatusList { get; private set; } = new List<PlayerStatus>();
        public void PlayerJoined(PlayerRef playerRef)
        {
            Debug.Log("MultiPlayManager.PlayerJoined");
            if (HasStateAuthority == false)
                return;

            var player = Runner.Spawn(_playerPrefab, GetSpawnPosition(), Quaternion.identity, playerRef);
            playerNetworkList.Add(player.Id);
            Runner.SetPlayerObject(playerRef, player.GetComponent<MultiPlayerMove>().Object);

            //Ingame用
            if(IngameManager.Instance != null)
            {
                IngameManager.Instance.SetReady(InGameSystem.ReadyEnum.PlayerReady);
                FindAnyObjectByType<FadeBlackImage>().SubscribeFadePanelEvent();//プレイヤーの死亡・蘇生時のイベントを登録
            }
        }

        public void PlayerLeft(PlayerRef playerRef)
        {

        }

        public override void Spawned()
        {
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }

        Vector3 GetSpawnPosition()
        {
            if(_isIngame)
            {
                return FindObjectOfType<StageGenerator>().spawnPosition;
            }
            var spawnPoint = _spawnPoint[Random.Range(0, _spawnPoint.Length)];
            //var randomPositionOffset = Random.insideUnitCircle * spawnPoint.Radius;
            return spawnPoint.transform.position;
        }

        /// <summary>
        /// ネットワークのシミュレーションごとに呼び出される。ロールバック等にも対応
        /// </summary>
        public override void FixedUpdateNetwork()
        {

            foreach (var change in _changeDetector.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(playerNetworkList):
                        List<GameObject> set = new List<GameObject>();
                        List<PlayerStatus> setCs = new List<PlayerStatus>();
                        for (int i = 0 ; i < playerNetworkList.Count; i++) {
                            if (Runner.TryFindObject(playerNetworkList[i], out var setObj)) {
                                set.Add(setObj.gameObject);
                                setCs.Add(setObj.gameObject.GetComponent<PlayerStatus>());
                            }
                        }
                        PlayerList = set;
                        PlayerStatusList = setCs;
                        break;
                        
                }
            }
        }
    }
}

