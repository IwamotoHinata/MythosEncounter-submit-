using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Scenes.MultiLobby.Player
{
    public class MultiLobbyPlayer : NetworkBehaviour
    {
        [Networked] public bool isReady { get; private set; } = false;

        public override void Render()
        {
            if (Object.HasInputAuthority)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    Rpc_IsReady();
                }
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void Rpc_IsReady()
        {
            Debug.Log(Object.InputAuthority.PlayerId + " is ready");
            isReady = true;
        }
    }
}
