using UnityEngine;
using UniRx;
using Scenes.Ingame.Player;
using Fusion;

namespace Scenes.Ingame.Enemy
{
    public class EnemyShootingAtackBehaviour : EnemyAttackBehaviour
    {
        [SerializeField] private GameObject _bullet;
        [SerializeField][Tooltip("’e‚Ìo‚Ä‚­‚éêŠ")] private GameObject _hand;

        public override void Behaviour(PlayerStatus targetStatus)
        {
            Debug.Log("‰“ŠuUŒ‚I");
            Runner.Spawn(_bullet, _hand.transform.position, _bullet.transform.rotation).GetComponent<IEnemyRangeAttack>().Init(targetStatus.gameObject.GetComponent<NetworkObject>().Id,_myNetworkId);
            /*
            target.ChangeHealth(_damage, "Damage");
            target.OnEnemyAttackedMeEvent.OnNext(Unit.Default);
            */
        }
    }
}
