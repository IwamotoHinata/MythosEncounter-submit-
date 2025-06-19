using System.Collections;
using UnityEngine;
using Scenes.Ingame.InGameSystem.UI;
using Scenes.Ingame.Enemy;

namespace Scenes.Ingame.Player
{

    public class MagicWhistleEffect : ItemEffect
    {
        [SerializeField] float _warpRange = 20f;//最低ワープ距離
        [SerializeField] float _warpPlayerRadius = 0.4f;
        [SerializeField] float _waitWarpTime = 1.25f;
        [SerializeField] float _waitCanUseItemTime = 1.5f;
        [SerializeField] float _attractRange = 12f;// 音による敵への誘引が可能な距離
        private GameObject[] _enemys;
        private Vector3 _warpPosition;
        private Collider[] _overlapResults = new Collider[1];//Physics.OverlapSphereのバッファ用
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
            float randomY = UnityEngine.Random.value < 0.5f ? 0f : 5.92f;//ワープ先の階数を決定
            int layerMask = ~LayerMask.GetMask("Floor");// 床オブジェクト以外のレイヤー取得

            while (true)
            {
                float minX = 0f, maxX = 70f, minZ = 0f, maxZ = 70f;
                float randomX = UnityEngine.Random.Range(minX, maxX);
                float randomZ = UnityEngine.Random.Range(minZ, maxZ);
                _warpPosition = new Vector3(randomX, randomY, randomZ);
                if (Mathf.Abs(randomY - transform.position.y) > 1 || Vector3.Distance(transform.position, _warpPosition) > _warpRange)//　ワープ先が別の階であるとき、もしくはワープ距離が範囲以上であるときのみ
                {
                    if (Physics.OverlapSphereNonAlloc(_warpPosition, _warpPlayerRadius, _overlapResults, layerMask, QueryTriggerInteraction.Collide) == 0)//ワープ位置に床以外のオブジェクトが重複しているか確認するため
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
                if(Vector3.Distance(transform.position, enemy.transform.position) < _attractRange)//敵が範囲内に居れば惹きつける
                {
                    enemy.transform.root.gameObject.GetComponent<EnemyAttack>().MyVisivillityMap.HearingSound(this.transform.position, 2f, false);
                    enemy.transform.root.gameObject.GetComponent<EnemySearch>().MyVisivillityMap.HearingSound(this.transform.position, 2f, false);
                }
            }
        }

    }
}
