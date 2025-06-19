using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Common.Network;

namespace Scenes.MultiLobby.StartDoor
{
    public class InGameStart : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                var manager = FindFirstObjectByType<MultiLobbyManager>();
                if (manager != null) manager.Rpc_StartGame();
            }
        }
    }
}
