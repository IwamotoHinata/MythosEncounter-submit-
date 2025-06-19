using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Scenes.Ingame.Player;

/// <summary>
/// VirtualCamera�̎����v�Z�ɂ��J�����̊p�x�E�ʒu�̒����ɂ��ăR�}���h�ŕύX���邽�߂̃N���X
/// </summary>
public class VcamExtension : CinemachineExtension
{
    [SerializeField] private PlayerMove _playerMove;
    [SerializeField] private MultiPlayerMove _multiPlayerMove;
    [SerializeField] private bool _isOnlineMode;

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (!_isOnlineMode)
        {
            if (_playerMove != null)
            {
                if (stage == CinemachineCore.Stage.Aim)
                {
                    var eulerAngles = state.RawOrientation.eulerAngles;
                    eulerAngles.x = _playerMove.NowCameraAngle.x;
                    state.RawOrientation = Quaternion.Euler(eulerAngles);
                }
            }
        }
        else
        {
            if (_multiPlayerMove != null)
            {
                if (stage == CinemachineCore.Stage.Aim)
                {
                    var eulerAngles = state.RawOrientation.eulerAngles;
                    eulerAngles.x = _multiPlayerMove.NowCameraAngle.x;
                    state.RawOrientation = Quaternion.Euler(eulerAngles);
                }
            }
        }
                
    }
}
