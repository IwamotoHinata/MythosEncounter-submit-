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
                Debug.Log("�A�C�e���ݒu");

                ownerPlayerItem.PutTrapFood();
            }
            else
            {
                Debug.Log("�A�C�e���ݒu�s��");
            }
        
        }
    }
}
