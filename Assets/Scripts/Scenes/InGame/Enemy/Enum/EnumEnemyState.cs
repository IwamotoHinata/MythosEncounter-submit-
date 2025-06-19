using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Ingame.Enemy
{
    public enum EnemyState
    {
        /// <summary>
        /// ‚È‚É‚à“o˜^‚³‚ê‚Ä‚¢‚È‚¢ó‘Ô
        /// </summary>
        None,
        /// <summary>
        /// „‰ñ
        /// </summary>
        Patrolling,
        /// <summary>
        /// õ“G
        /// </summary>
        Searching,
        /// <summary>
        /// ’ÇÕ
        /// </summary>
        Chase,
        /// <summary>
        /// UŒ‚
        /// </summary>
        Attack,
        /// <summary>
        /// ‘ŞU
        /// </summary>
        FallBack,
        /// <summary>
        /// “Áês“®
        /// </summary>
        Special,
        /// <summary>
        /// ”­Œ©
        /// </summary>
        Discover,
        /// <summary>
        /// ƒXƒ^ƒ“
        /// </summary>
        Stun,
    }


}
