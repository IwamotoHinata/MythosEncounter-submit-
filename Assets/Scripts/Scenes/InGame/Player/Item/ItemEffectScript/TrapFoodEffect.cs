using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Scenes.Ingame.Player
{
    public class TrapFoodEffect : ItemEffect
    {
        public override void OnPickUp()
        {
            StartCoroutine(ownerPlayerItem.CreateTrapFood());
        }

        public override void OnThrow()
        {

        }

        public override void Effect()
        {
            if (ownerPlayerItem.IsCanCreateTrapFood && ownerPlayerItem.CreatedTrapFood != null)
            {
                Debug.Log("アイテム設置");

                ownerPlayerItem.PutTrapFood();
            }
            else
            {
                Debug.Log("アイテム設置不可");
            }
        
        }
    }
}
