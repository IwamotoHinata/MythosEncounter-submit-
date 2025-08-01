using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Ingame.Enemy
{
    public enum EnemyState
    {
        /// <summary>
        /// なにも登録されていない状態
        /// </summary>
        None,
        /// <summary>
        /// 巡回
        /// </summary>
        Patrolling,
        /// <summary>
        /// 索敵
        /// </summary>
        Searching,
        /// <summary>
        /// 追跡
        /// </summary>
        Chase,
        /// <summary>
        /// 攻撃
        /// </summary>
        Attack,
        /// <summary>
        /// 退散
        /// </summary>
        FallBack,
        /// <summary>
        /// 特殊行動
        /// </summary>
        Special,
        /// <summary>
        /// 発見
        /// </summary>
        Discover,
        /// <summary>
        /// スタン
        /// </summary>
        Stun,
    }


}
