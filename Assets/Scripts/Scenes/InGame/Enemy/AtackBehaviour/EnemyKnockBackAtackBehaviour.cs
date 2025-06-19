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
        [SerializeField][Tooltip("�m�b�N�o�b�N�����鋗��")] private float _nockBackDistance;
        [SerializeField][Tooltip("Box�̉��̃X�P�[��")] private float _boxlCastHorizontalSize;
        [SerializeField][Tooltip("Box�̏c�̃X�P�[��")] private float _boxCastVerticalSize;
            
        //[SerializeField][Tooltip("�J�v�Z���L���X�g����ۂ̃^�[�Q�b�g���W�̏���ւ̕␳")] private float _targetPositionUpwardCorrection;
        [SerializeField][Tooltip("�J�v�Z���L���X�g����ۂ̎��g�̍��W�̏���ւ̕␳")] private float _myPositionUpwardCorrection;

        public override void Behaviour(PlayerStatus targetStatus)
        {
            Debug.Log("�X�^���U���I");
            targetStatus.ChangeHealth(_damage, ChangeValueMode.Damage);
            targetStatus.SetlastAtackEnemyId(_myNetworkId);
            targetStatus.OnEnemyAttackedMeEvent.OnNext(Unit.Default);
            
            //rayCast�̏���
            Vector3 direction = (targetStatus.transform.position - this.transform.position).normalized;
            Vector3 horizontalInnerProduct = new Vector3(direction.x, 0, direction.z);//���������ւ̕ύX

            if (Physics.BoxCast(this.transform.position + new Vector3(0, _myPositionUpwardCorrection, 0),//BoxCast
                new Vector3(_boxlCastHorizontalSize / 2, _boxCastVerticalSize / 2, _boxlCastHorizontalSize / 2),
                horizontalInnerProduct,
                out RaycastHit hitInfo,
                targetStatus.transform.rotation,
                _nockBackDistance * horizontalInnerProduct.magnitude,
                -1 ^ LayerMask.GetMask(new string[] { "Ignore Raycast", "Player", "Enemy" })
                ))
            {
                Debug.Log("�v���C���[�ɃX�^��������܂��I�l�b�g���[�N�������v���C���[�̂���u�����`�Ƀ}�[�W�����̂��ɒǉ����܂�");
                Vector3 hitPoint = hitInfo.point;
                Vector3 localHitPoint = hitPoint - this.transform.position;
                Vector3 localCastPoint = Vector3.Project(localHitPoint,horizontalInnerProduct.normalized);//�ٓ�������ׂ��_���擾
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
