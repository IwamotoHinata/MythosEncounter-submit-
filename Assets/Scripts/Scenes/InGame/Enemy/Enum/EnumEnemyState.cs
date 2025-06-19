using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Ingame.Enemy
{
    public enum EnemyState
    {
        /// <summary>
        /// �Ȃɂ��o�^����Ă��Ȃ����
        /// </summary>
        None,
        /// <summary>
        /// ����
        /// </summary>
        Patrolling,
        /// <summary>
        /// ���G
        /// </summary>
        Searching,
        /// <summary>
        /// �ǐ�
        /// </summary>
        Chase,
        /// <summary>
        /// �U��
        /// </summary>
        Attack,
        /// <summary>
        /// �ގU
        /// </summary>
        FallBack,
        /// <summary>
        /// ����s��
        /// </summary>
        Special,
        /// <summary>
        /// ����
        /// </summary>
        Discover,
        /// <summary>
        /// �X�^��
        /// </summary>
        Stun,
    }


}
