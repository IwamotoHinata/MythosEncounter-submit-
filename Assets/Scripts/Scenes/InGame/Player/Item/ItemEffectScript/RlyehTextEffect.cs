using Fusion;
using Scenes.Ingame.Enemy;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;

namespace Scenes.Ingame.Player
{
    public class RlyehTextEffect : ItemEffect
    {
        [SerializeField] private GameObject _fogEffect;
        private bool _used = false;
        CancellationTokenSource source = new CancellationTokenSource();

        public override void OnPickUp()
        {
            //�U����������Ƃ�������Bool��True�ɂȂ����Ƃ��ɃA�C�e���g�p�𒆒f
            ownerPlayerStatus.OnEnemyAttackedMe
                .Subscribe(_ =>
                {
                    source?.Cancel();
                    source?.Dispose();
                }).AddTo(this);
        }

        public override void OnThrow()
        {

        }

        public override void Effect()
        {
            RlyehEffect().Forget();
        }

        private async UniTaskVoid RlyehEffect()
        {
            if (_used) return;
            _used = true;
            var token = ownerPlayerStatus.GetCancellationTokenOnDestroy();
            SoundManager.Instance.PlaySe("se_R'lyehText_00", transform.position);

            var fog = RunnerSpawner.RunnerInstance.Spawn(_fogEffect, ownerPlayerStatus.transform.position, Quaternion.identity);
            float effectRadius = 10f;
            fog.transform.localScale = new Vector3(effectRadius * 2, 1, effectRadius * 2);

            // ���ʔ͈͓��̓G���擾
            Collider[] enemiesInRange = Physics.OverlapSphere(ownerPlayerStatus.transform.position, effectRadius);
            foreach (var collider in enemiesInRange)
            {
                if (collider.CompareTag("Enemy"))
                {
                    // �ǂ����邩�m�F
                    Vector3 directionToEnemy = collider.transform.position - ownerPlayerStatus.transform.position;
                    Ray ray = new Ray(ownerPlayerStatus.transform.position, directionToEnemy);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, effectRadius))
                    {
                        if (hit.collider.gameObject == collider.gameObject)
                        {
                            // �G�̃��Z�b�g����
                            collider.GetComponent<EnemyMove>().ResetPosition();
                        }
                    }
                }
            }
            ownerPlayerItem.ConsumeItem(ownerPlayerItem.nowIndex);

            await UniTask.WaitForSeconds(10f, cancellationToken: token);
            RunnerSpawner.RunnerInstance.Despawn(fog);  // 10�b��ɖ��G�t�F�N�g��j��  
        }
    }
}