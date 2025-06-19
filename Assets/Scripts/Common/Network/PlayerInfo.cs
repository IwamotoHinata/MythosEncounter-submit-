using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Common.Network.SceneManager;

namespace Common.Network
{
    public class PlayerInfo : NetworkBehaviour, IPlayerJoined
    {
        [Networked] public string userName { get; set; }

        private NetworkObject _playerObject;

        private void Start()
        {
            DontDestroyOnLoad(this);
        }

        public override void Spawned()
        {
            //�l�b�g���[�N�V�~�����[�V�����̏I���܂Ŏg��
            DontDestroyOnLoad(this);

            //�z�X�g����
            if (Object.HasInputAuthority == false) return;

            Rpc_SendPlayerInfo("username");
            
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            Runner.Despawn(_playerObject);
        }

        /// <summary>
        /// �v���C���[�Q�����Ƀv���C���[�̃A�o�^�[���X�|�[��������
        /// </summary>
        /// <param name="player"></param>
        public void PlayerJoined(PlayerRef player)
        {
            if (Runner.IsServer == false) return;

            var spawner = FindObjectOfType<PlayerSpawner>();
            if (spawner != null && player == Object.InputAuthority)
            {
                _playerObject = spawner.Spawn(Object.InputAuthority);
            }
        }

        /// <summary>
        /// �V�[���}�l�[�W���[����̃R�[���ŃA�o�^�[���X�|�[������
        /// </summary>
        public void SpawnCall()
        {
            if (Runner.IsServer == false) return;

            //�S�v���C���[�A�o�^�[���X�|�[������
            var spawner = FindObjectOfType<PlayerSpawner>();
            if (spawner != null)
            {
                _playerObject = spawner.Spawn(Object.InputAuthority);
            }
        }

        /// <summary>
        /// �z�X�g�ւ̏��`�B
        /// </summary>
        /// <param name="name"></param>
        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        private void Rpc_SendPlayerInfo(string name)
        {
            userName = name;
        }
    }
}
