using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Common.Network.SceneManager
{
    public interface ISpawnAccess
    {
        public NetworkObject GetPrefab();
        public Vector3 GetPosition();
        public Quaternion GetRotation();
    }
}
