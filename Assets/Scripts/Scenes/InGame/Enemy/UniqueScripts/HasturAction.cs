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
        [SerializeField][Tooltip("���[�v����ɂ������Ĉړ����nM�����Ƀv���C���[�������烏�[�v�𒆎~���邩�Ƃ��������̂��߂̃G���A")] private float _checkPlayerRadius;
        [SerializeField][Tooltip("���[�v��̃X�e�[�W��F�����邽�߂�OverlapSphere��")] private float _checkStageRadius;
        [SerializeField][Tooltip("���[�v��̃X�e�[�W��F�����邽�߂�OverlapSphere�̃X�^�[�g�ʒu")]private Vector3 _checkStageStart;
        [SerializeField][Tooltip("���[�v��̃X�e�[�W��F�����邽�߂�OverlapSphere�̏I���ʒu")] private Vector3 _checkStageEnd;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private bool _debugMode;
       
        public override void Init(int actionCoolDown)
        {
            _actionCoolDownTime = (float)actionCoolDown;
        }


        protected override void Action()//�㉺�ւ̈ړ������݂�
        {
            if (_enemyStatus.State == EnemyState.Patrolling|| _enemyStatus.State == EnemyState.Searching) {
                //�����ŏ㉺�ړ����s��
                Vector3 warpPosition;
                if (_enemyStatus.MyEnemyVisivilityMap.GetYPosition(this.transform.position) == 0)
                { //��K�̏ꍇ
                    warpPosition = this.transform.position + new Vector3(0, _enemyStatus.MyEnemyVisivilityMap.gridRange, 0);
                }
                else
                {//��K�̏ꍇ
                    warpPosition = this.transform.position + new Vector3(0, -_enemyStatus.MyEnemyVisivilityMap.gridRange, 0);
                }
                if (_debugMode) {
                    Debug.Log("�ړ��̖ڕW�ʒu" + warpPosition);
                }

                if (!CheckPlyer(warpPosition)) //���[�v���Player�����Ȃ����Ƃ��m�F
                {
                    if (!CheckWall(warpPosition))
                    { //���[�v��ɏ�Q�����Ȃ����
                        
                        _networkTransform.Teleport(warpPosition);
                        _agent.Warp(warpPosition);
                        if (_debugMode) { Debug.Log("�ړ����܂��A�ړ����" + transform.position); }

                    }
                    else
                    {
                        if (_debugMode) { Debug.Log("��Q��������ړ��ł��Ȃ�����"); }
                    }
                }
                else {
                    if (_debugMode) { Debug.Log("�v���C���[������ړ��ł��Ȃ�����"); }
                }
            }
        }

        /// <summary>
        /// ���[�v��Ƀv���C���[�����Ȃ����ǂ����𒲂ׂ�
        /// </summary>
        /// <param name="warpPosition">�ړ���</param>
        /// <returns>�v���C���[�����͂ɂ�����true���Ȃ����false</returns>
        private bool CheckPlyer(Vector3 warpPosition) {
            return Physics.OverlapSphere(warpPosition, _checkPlayerRadius, LayerMask.GetMask("Player")).Length > 0;
        }

        /// <summary>
        /// ���[�v��ɕǂ����邩�ǂ����𒲂ׂ�
        /// </summary>
        /// <param name="warpPosition">�ړ���</param>
        /// <returns>�ړ���ɕǂ���������True��Ԃ�</returns>
        private bool CheckWall(Vector3 warpPosition) {
            return Physics.OverlapCapsule(warpPosition + _checkStageStart, warpPosition + _checkStageEnd, _checkStageRadius, LayerMask.GetMask(new string[] { "Ignore Raycast", "Player", "Enemy" })).Length > 0;
        }


    }
}