using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scenes.Ingame.Enemy;

public class TrapFoodSensor : MonoBehaviour
{
    private bool _isActiveSensor = true;//�A���ŃZ���T�[���쓮�����Ȃ����߂̕ϐ�
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.transform.root.gameObject.TryGetComponent<EnemyStatus>(out EnemyStatus enemyStatus))
        {
            Debug.Log("���m");
            Vector3 startingPoint = this.transform.position + transform.up * 2f;//���C�̔��˓_�������������邽��
            Vector3 direction = (collider.transform.position + transform.up * 2f ) - startingPoint;
            int layerMask = LayerMask.GetMask("Floor") | LayerMask.GetMask("Wall")�@| LayerMask.GetMask("StageIntract");

            if (!Physics.Raycast(startingPoint, direction, direction.magnitude, layerMask)�@&& _isActiveSensor )// �G�Ƃ̊Ԃɕǂ�{�I�Ȃǂ�������Ώ����J�n
            {
                Debug.Log("��Q���Ȃ�");
                float random = Random.value * 100f;
                if (random <= 30f)
                {
                    Destroy(collider.gameObject);
                    MoveEnemy();
                    transform.parent.gameObject.layer = 0;//���C���[��default�ɖ߂��ďE���Ȃ�����
                    Destroy(this.gameObject);
                }
                _isActiveSensor = false;
                Invoke("ActiveSensor", 1f);
            }
        }

    }

    /// <summary>
    ///�G�L������U�����邽�߂̊֐�
    /// </summary>
    private void MoveEnemy()
    {

    }

    private void ActiveSensor()
    {
        _isActiveSensor = true;
    }
}
