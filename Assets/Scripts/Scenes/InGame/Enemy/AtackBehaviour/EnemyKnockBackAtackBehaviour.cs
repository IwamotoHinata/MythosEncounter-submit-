using Fusion;
using Fusion.Addons.SimpleKCC;
using Scenes.Ingame.Player;
using UniRx;
using UnityEngine;


namespace Scenes.Ingame.Enemy
{
    public class EnemyKnockBackAtackBehaviour : EnemyAttackBehaviour
    {
        [SerializeField] private int _damage;
        [SerializeField] private int _breedDamage;
        [SerializeField][Tooltip("ノックバックさせる距離")] private float _nockBackDistance;
        [SerializeField][Tooltip("Boxの横のスケール")] private float _boxlCastHorizontalSize;
        [SerializeField][Tooltip("Boxの縦のスケール")] private float _boxCastVerticalSize;
            
        //[SerializeField][Tooltip("カプセルキャストする際のターゲット座標の上方への補正")] private float _targetPositionUpwardCorrection;
        [SerializeField][Tooltip("カプセルキャストする際の自身の座標の上方への補正")] private float _myPositionUpwardCorrection;

        public override void Behaviour(PlayerStatus targetStatus)
        {
            Debug.Log("スタン攻撃！");
            targetStatus.ChangeHealth(_damage, ChangeValueMode.Damage);
            targetStatus.SetlastAtackEnemyId(_myNetworkId);
            targetStatus.OnEnemyAttackedMeEvent.OnNext(Unit.Default);
            
            //rayCastの準備
            Vector3 direction = (targetStatus.transform.position - this.transform.position).normalized;
            Vector3 horizontalInnerProduct = new Vector3(direction.x, 0, direction.z);//水平方向への変更

            if (Physics.BoxCast(this.transform.position + new Vector3(0, _myPositionUpwardCorrection, 0),//BoxCast
                new Vector3(_boxlCastHorizontalSize / 2, _boxCastVerticalSize / 2, _boxlCastHorizontalSize / 2),
                horizontalInnerProduct,
                out RaycastHit hitInfo,
                targetStatus.transform.rotation,
                _nockBackDistance * horizontalInnerProduct.magnitude,
                -1 ^ LayerMask.GetMask(new string[] { "Ignore Raycast", "Player", "Enemy" })
                ))
            {
                Debug.Log("プレイヤーにスタンが入ります！ネットワーク化したプレイヤーのあるブランチにマージしたのちに追加します");
                Vector3 hitPoint = hitInfo.point;
                Vector3 localHitPoint = hitPoint - this.transform.position;
                Vector3 localCastPoint = Vector3.Project(localHitPoint,horizontalInnerProduct.normalized);//異動させるべき点を取得
                Vector3 movePoint = localCastPoint + this.transform.position;
                //targetStatus.transform.position = movePoint;
                targetStatus.GetComponent<SimpleKCC>().SetPosition(movePoint);
            }
            else {
                targetStatus.GetComponent<SimpleKCC>().SetPosition(targetStatus.transform.position + (horizontalInnerProduct * _nockBackDistance));
            }
            }

        }
}
