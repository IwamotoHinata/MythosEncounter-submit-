using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cinemachine;
using System.Runtime.InteropServices;


namespace Scenes.Ingame.Player
{
    /// <summary>
    /// VirturalCameraに付属しているCinemachineVirtualCameraコンポーネントのデータを調整するためのクラス
    /// </summary>
    public class PlayerVcamStatus : MonoBehaviour
    {
        [SerializeField] private List<CinemachineVirtualCamera> _cinemachineVirtualCameras;
        [SerializeField] private PlayerStatus _myPlayerStatus;
        private BoolReactiveProperty _isNoising = new BoolReactiveProperty(true);


        private void Start()
        {
            _myPlayerStatus.OnPlayerActionStateChange
                .Where(state => state == PlayerActionState.Idle || state == PlayerActionState.Walk || state == PlayerActionState.Dash || state == PlayerActionState.Sneak)
                .Subscribe(state =>
                {

                    foreach (var vcam in _cinemachineVirtualCameras) {
                        vcam.Priority = 0;
                    }
                    switch (state) {
                        case PlayerActionState.Idle:
                            _cinemachineVirtualCameras[0].Priority = 1;
                            break;

                        case PlayerActionState.Walk:
                            _cinemachineVirtualCameras[1].Priority = 1;
                            break;

                        case PlayerActionState.Dash:
                            _cinemachineVirtualCameras[2].Priority = 1;
                            break;

                        case PlayerActionState.Sneak:
                            _cinemachineVirtualCameras[3].Priority = 1;
                            break;
                    }

                });

            _isNoising
                .Skip(1)
                .Subscribe(x =>
                {
                    foreach (var vcam in _cinemachineVirtualCameras) {
                        vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = (x ? 1 : 0);
                    }
                });      
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Y)){
                if (_isNoising.Value == true) {
                    _isNoising.Value = false;
                }
                else {
                    _isNoising.Value = true;
                }
            }
        }


    }
}

