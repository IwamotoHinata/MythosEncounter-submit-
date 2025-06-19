using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using Scenes.Ingame.Enemy;

namespace Scenes.Ingame.Player
{
    public class MeleeWeaponEffect : ItemEffect
    {
        private Vector3 _attackPointOffset = new Vector3(0, 2, 0);
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private bool _attackAnimation = false;
        private const float ATTACKDETECTIONTIME = 1f;//攻撃の判定時間
        private const float ATTACKDISTANCE = 3f;//攻撃の判定距離
        public override void Effect()
        {
            if (!_attackAnimation)
            {
                AttackEffect().Forget();
            }
        }
        private async UniTaskVoid AttackEffect()
        {
            var elapsedTime = 0.0f;
            var findAttack = false;
            _attackAnimation = true;
            while (elapsedTime < ATTACKDETECTIONTIME)
            {
                var halfExtents = new Vector3(1f, 1f, 1f); // 各軸についてのボックスサイズの半分
                var hits = Physics.BoxCastAll
                (
                    center: ownerPlayerStatus.gameObject.transform.position + _attackPointOffset,
                    halfExtents: halfExtents,
                    direction: transform.forward,
                    orientation: ownerPlayerStatus.gameObject.transform.rotation
                );

                foreach (var hit in hits)
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        if (hit.collider.gameObject.TryGetComponent(out EnemyStatus status))
                        {
                            status.ApplyStun().Forget();
                            findAttack = true;
                        }
                    }

                }

                if (findAttack) break;  // 攻撃対象への処理が完了したらタスク終了
                elapsedTime += Time.deltaTime;
                await UniTask.Yield(cancellationToken: _tokenSource.Token);
            }
            _attackAnimation = false;
        }
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            //　Cubeのレイを疑似的に視覚化
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(ownerPlayerStatus.gameObject.transform.position + transform.forward + _attackPointOffset, Vector3.one * 2);
        }
#endif
        public override void OnPickUp()
        {
        }

        public override void OnThrow()
        {
            _tokenSource.Cancel();
        }
    }
}