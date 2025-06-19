using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Common.Network.SceneManager
{
    public class PlayerSpawner : NetworkBehaviour
    {
        public NetworkObject Spawn(PlayerRef player)
        {
            if (TryGetComponent<ISpawnAccess>(out var access) == true)
            {
                var playerObj = Runner.Spawn(
                    access.GetPrefab(),
                    access.GetPosition(),
                    access.GetRotation(),
                    player);

                return playerObj;
            }
            else
            {
                Debug.LogError("Player Not Spawned");
                return null;
            }
        }
    }
}
