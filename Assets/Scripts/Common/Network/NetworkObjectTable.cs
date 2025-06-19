using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Common.Network
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Common/Network/NetworkObjectTable", fileName = "NetworkObjectTable")]
    public class NetworkObjectTable : ScriptableObject
    {
        [Serializable]
        public class SceneManagerTable
        {
            public int index; //�V�[���C���f�b�N�X
            public NetworkObject Object; //�V�[���}�l�[�W���[�v���n�u
        }

        public NetworkObject playerInfo; //�v���C���[�̏����Ǘ�����I�u�W�F�N�g
        public List<SceneManagerTable> sceneManager = new List<SceneManagerTable>(); //�V�[���}�l�[�W���[�I�u�W�F�N�g
    }
}
