using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Scenes.Ingame.Enemy
{
    public class HasturAction : EnemyUniqueAction
    {
        [SerializeField] private EnemyStatus _enemyStatus;
        [SerializeField] private NetworkTransform _networkTransform;
        [SerializeField][Tooltip("ワープするにあたって移動先のnM県内にプレイヤーがいたらワープを中止するかという処理のためのエリア")] private float _checkPlayerRadius;
        [SerializeField][Tooltip("ワープ先のステージを認識するためのOverlapSphereの")] private float _checkStageRadius;
        [SerializeField][Tooltip("ワープ先のステージを認識するためのOverlapSphereのスタート位置")]private Vector3 _checkStageStart;
        [SerializeField][Tooltip("ワープ先のステージを認識するためのOverlapSphereの終了位置")] private Vector3 _checkStageEnd;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private bool _debugMode;
       
        public override void Init(int actionCoolDown)
        {
            _actionCoolDownTime = (float)actionCoolDown;
        }


        protected override void Action()//上下への移動を試みる
        {
            if (_enemyStatus.State == EnemyState.Patrolling|| _enemyStatus.State == EnemyState.Searching) {
                //ここで上下移動を行う
                Vector3 warpPosition;
                if (_enemyStatus.MyEnemyVisivilityMap.GetYPosition(this.transform.position) == 0)
                { //一階の場合
                    warpPosition = this.transform.position + new Vector3(0, _enemyStatus.MyEnemyVisivilityMap.gridRange, 0);
                }
                else
                {//二階の場合
                    warpPosition = this.transform.position + new Vector3(0, -_enemyStatus.MyEnemyVisivilityMap.gridRange, 0);
                }
                if (_debugMode) {
                    Debug.Log("移動の目標位置" + warpPosition);
                }

                if (!CheckPlyer(warpPosition)) //ワープ先にPlayerがいないことを確認
                {
                    if (!CheckWall(warpPosition))
                    { //ワープ先に障害物がなければ
                        
                        _networkTransform.Teleport(warpPosition);
                        _agent.Warp(warpPosition);
                        if (_debugMode) { Debug.Log("移動します、移動先は" + transform.position); }

                    }
                    else
                    {
                        if (_debugMode) { Debug.Log("障害物があり移動できなかった"); }
                    }
                }
                else {
                    if (_debugMode) { Debug.Log("プレイヤーがおり移動できなかった"); }
                }
            }
        }

        /// <summary>
        /// ワープ先にプレイヤーがいないかどうかを調べる
        /// </summary>
        /// <param name="warpPosition">移動先</param>
        /// <returns>プレイヤーが周囲にいたらtrueいなければfalse</returns>
        private bool CheckPlyer(Vector3 warpPosition) {
            return Physics.OverlapSphere(warpPosition, _checkPlayerRadius, LayerMask.GetMask("Player")).Length > 0;
        }

        /// <summary>
        /// ワープ先に壁があるかどうかを調べる
        /// </summary>
        /// <param name="warpPosition">移動先</param>
        /// <returns>移動先に壁があったらTrueを返す</returns>
        private bool CheckWall(Vector3 warpPosition) {
            return Physics.OverlapCapsule(warpPosition + _checkStageStart, warpPosition + _checkStageEnd, _checkStageRadius, LayerMask.GetMask(new string[] { "Ignore Raycast", "Player", "Enemy" })).Length > 0;
        }


    }
}