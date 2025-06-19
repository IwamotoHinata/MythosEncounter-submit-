using Scenes.Ingame.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Ingame.Player
{
    public class HealOrbEffect : ItemEffect
    {
        public override void OnPickUp()
        {
            
        }

        public override void OnThrow()
        {
            
        }

        public override void Effect()
        {
            ownerPlayerStatus.ChangeHealth(100, ChangeValueMode.Heal);
            ownerPlayerItem.ConsumeItem(ownerPlayerItem.nowIndex);
        }

    }
}

