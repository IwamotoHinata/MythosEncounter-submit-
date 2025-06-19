using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using Cinemachine;
using UniRx;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace Scenes.Ingame.Player
{
    public class PlayerAnimationManager : NetworkBehaviour
    {
        [SerializeField]
        SimpleKCC _simpleKCC;
        [SerializeField]
        Animator _animator;
        [SerializeField]
        CinemachineVirtualCamera _firstPersonCamera;
        [SerializeField]
        CinemachineVirtualCamera _thirdPersonCamera;
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        private void Start()
        {
            var status = GetComponent<PlayerStatus>();
            status.OnGetDamange.Subscribe(_ => _impulseSource.GenerateImpulse()).AddTo(this);
            status.OnPlayerSurviveChange.Subscribe(survive => ChangeCameraPriority(survive)).AddTo(this);
        }
        private CancellationTokenSource _cameraSwitchCts = new CancellationTokenSource();
        private async UniTaskVoid ChangeCameraPriority(bool survive)
        {
            _cameraSwitchCts.Cancel();
            _cameraSwitchCts = new CancellationTokenSource();
            if (survive)
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(10f), cancellationToken: _cameraSwitchCts.Token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                _firstPersonCamera.Priority = 10;
                _thirdPersonCamera.Priority = 5;
            }
            else
            {
                _firstPersonCamera.Priority = 5;
                _thirdPersonCamera.Priority = 10;
            }
        }
        public override void Render()
        {
            var moveVelocity = GetAnimationMoveVelocity();

            //_animator.SetFloat("MovementSpeed", moveVelocity);
            _animator.SetFloat("MoveZ", moveVelocity.z, 0.05f, Time.deltaTime);
            _animator.SetFloat("MoveX", moveVelocity.x, 0.05f, Time.deltaTime);
        }


        Vector3 GetAnimationMoveVelocity()
        {
            if (_simpleKCC.RealSpeed < 0.01f)
            {
                return default;
            }
                

            var velocity = _simpleKCC.RealVelocity;
            //Debug.Log(velocity);
            velocity.y = 0f;

            //1に正規化
            /*if(velocity.sqrMagnitude > 1f)
            {
                velocity.Normalize();
            }*/

            return transform.InverseTransformVector(velocity);  //ワールド空間からローカル空間へ変換
        }
    }
}

