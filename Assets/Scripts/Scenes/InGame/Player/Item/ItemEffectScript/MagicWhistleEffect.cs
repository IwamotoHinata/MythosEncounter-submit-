using System.Collections;
using UnityEngine;
using Scenes.Ingame.InGameSystem.UI;
using Scenes.Ingame.Enemy;

namespace Scenes.Ingame.Player
{

    public class MagicWhistleEffect : ItemEffect
    {
        [SerializeField] float _warpRange = 20f;//�Œ჏�[�v����
        [SerializeField] float _warpPlayerRadius = 0.4f;
        [SerializeField] float _waitWarpTime = 1.25f;
        [SerializeField] float _waitCanUseItemTime = 1.5f;
        [SerializeField] float _attractRange = 12f;// ���ɂ��G�ւ̗U�����\�ȋ���
        private GameObject[] _enemys;
        private Vector3 _warpPosition;
        private Collider[] _overlapResults = new Collider[1];//Physics.OverlapSphere�̃o�b�t�@�p
        public override void OnPickUp()
        {
 
        }

        public override void OnThrow()
        {

        }

        public override void Effect()
        {
            PrepareWarp();
        }

        private void PrepareWarp()
        {
            float _warpPlayerRadius = 0.7f;
            float randomY = UnityEngine.Random.value < 0.5f ? 0f : 5.92f;//���[�v��̊K��������
            int layerMask = ~LayerMask.GetMask("Floor");// ���I�u�W�F�N�g�ȊO�̃��C���[�擾

            while (true)
            {
                float minX = 0f, maxX = 70f, minZ = 0f, maxZ = 70f;
                float randomX = UnityEngine.Random.Range(minX, maxX);
                float randomZ = UnityEngine.Random.Range(minZ, maxZ);
                _warpPosition = new Vector3(randomX, randomY, randomZ);
                if (Mathf.Abs(randomY - transform.position.y) > 1 || Vector3.Distance(transform.position, _warpPosition) > _warpRange)//�@���[�v�悪�ʂ̊K�ł���Ƃ��A�������̓��[�v�������͈͈ȏ�ł���Ƃ��̂�
                {
                    if (Physics.OverlapSphereNonAlloc(_warpPosition, _warpPlayerRadius, _overlapResults, layerMask, QueryTriggerInteraction.Collide) == 0)//���[�v�ʒu�ɏ��ȊO�̃I�u�W�F�N�g���d�����Ă��邩�m�F���邽��
                    {
                        StartCoroutine(DoWorp());
                        AttractEnemy();
                        GameObject.FindWithTag("FadeOut").transform.parent.gameObject.GetComponent<FadeBlackImage>().FadeInWarp();
                        ownerPlayerStatus.UseItem(true);
                        ownerPlayerStatus.ChangeSpeed();
                        ownerPlayerItem.ChangeCanChangeBringItem(false);
                        SoundManager.Instance.PlaySe("se_whistle00", transform.position);
                        break;
                    }
                }
            }
        }

        IEnumerator DoWorp()
        {
            yield return new WaitForSeconds(1.25f);
            ownerPlayerStatus.gameObject.transform.position = _warpPosition;
            yield return new WaitForSeconds(1.5f);
            ownerPlayerStatus.UseItem(false);
            ownerPlayerStatus.ChangeSpeed();
            ownerPlayerItem.ChangeCanChangeBringItem(true);
            ownerPlayerItem.ConsumeItem(ownerPlayerItem.nowIndex);
        }

        private void AttractEnemy()
        {
            _enemys = GameObject.FindGameObjectsWithTag("Enemy");
            
            foreach(GameObject enemy in _enemys)
            {
                if(Vector3.Distance(transform.position, enemy.transform.position) < _attractRange)//�G���͈͓��ɋ���Ύ䂫����
                {
                    enemy.transform.root.gameObject.GetComponent<EnemyAttack>().MyVisivillityMap.HearingSound(this.transform.position, 2f, false);
                    enemy.transform.root.gameObject.GetComponent<EnemySearch>().MyVisivillityMap.HearingSound(this.transform.position, 2f, false);
                }
            }
        }

    }
}
