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
            public int index; //シーンインデックス
            public NetworkObject Object; //シーンマネージャープレハブ
        }

        public NetworkObject playerInfo; //プレイヤーの情報を管理するオブジェクト
        public List<SceneManagerTable> sceneManager = new List<SceneManagerTable>(); //シーンマネージャーオブジェクト
    }
}
