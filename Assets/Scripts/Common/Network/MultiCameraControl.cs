using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Common.Network
{
    public class MultiCameraControl : NetworkBehaviour
    {
        [SerializeField] private Camera _camera;

        public override void Spawned()
        {
            if (Object.HasInputAuthority == true)
            {
                _camera.enabled = true;
            }
        }
    }
}
