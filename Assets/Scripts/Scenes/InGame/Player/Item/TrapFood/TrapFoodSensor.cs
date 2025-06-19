using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scenes.Ingame.Enemy;

public class TrapFoodSensor : MonoBehaviour
{
    private bool _isActiveSensor = true;//連続でセンサーを作動させないための変数
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.transform.root.gameObject.TryGetComponent<EnemyStatus>(out EnemyStatus enemyStatus))
        {
            Debug.Log("検知");
            Vector3 startingPoint = this.transform.position + transform.up * 2f;//レイの発射点を少し高くするため
            Vector3 direction = (collider.transform.position + transform.up * 2f ) - startingPoint;
            int layerMask = LayerMask.GetMask("Floor") | LayerMask.GetMask("Wall")　| LayerMask.GetMask("StageIntract");

            if (!Physics.Raycast(startingPoint, direction, direction.magnitude, layerMask)　&& _isActiveSensor )// 敵との間に壁や本棚などが無ければ処理開始
            {
                Debug.Log("障害物なし");
                float random = Random.value * 100f;
                if (random <= 30f)
                {
                    Destroy(collider.gameObject);
                    MoveEnemy();
                    transform.parent.gameObject.layer = 0;//レイヤーをdefaultに戻して拾えなくする
                    Destroy(this.gameObject);
                }
                _isActiveSensor = false;
                Invoke("ActiveSensor", 1f);
            }
        }

    }

    /// <summary>
    ///敵キャラを誘導するための関数
    /// </summary>
    private void MoveEnemy()
    {

    }

    private void ActiveSensor()
    {
        _isActiveSensor = true;
    }
}
