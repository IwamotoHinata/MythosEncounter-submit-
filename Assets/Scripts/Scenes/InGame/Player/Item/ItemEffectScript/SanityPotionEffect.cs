using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Scenes.Ingame.Player
{
    public class SanityPotionEffect : ItemEffect
    {
        private bool _stopCoroutineBool = false;

        public override void OnPickUp()
        {
            //攻撃くらったときを示すBoolがTrueになったときにアイテム使用を中断
            ownerPlayerStatus.OnEnemyAttackedMe
                .Subscribe(_ => _stopCoroutineBool = true).AddTo(this);
        }

        public override void OnThrow()
        {

        }

        public override void Effect()
        {
            if(ownerPlayerStatus.nowPlayerSanValue != 100)
            {
                StartCoroutine(UseSanityPotion());
            }
            else
            {
                Debug.Log("SAN値が最大なので使用不可");
            }
        }

        public IEnumerator UseSanityPotion()
        {
            Debug.Log("SAN値回復薬使う");
            SoundManager.Instance.PlaySe("se_tranquilizer00", transform.position);

            //アイテム使用直後にステータス変更を行う
            ownerPlayerStatus.UseItem(true);
            ownerPlayerStatus.ChangeSpeed();
            ownerPlayerItem.ChangeCanChangeBringItem(false);

            int count = 0;

            while (true)
            {
                //攻撃を食らった際にこのコルーチンを破棄              
                if (_stopCoroutineBool == true)
                {
                    Debug.Log("san回復薬使用のコルーチンを破棄");
                    _stopCoroutineBool = false;
                    ownerPlayerStatus.UseItem(false);
                    ownerPlayerStatus.ChangeSpeed();
                    ownerPlayerItem.ConsumeItem(ownerPlayerItem.nowIndex);
                    ownerPlayerItem.ChangeCanChangeBringItem(true);
                    yield break;
                }

                yield return new WaitForSeconds(0.5f);
                ownerPlayerStatus.ChangeSanValue(1, ChangeValueMode.Heal);
                count++;

                //SAN値が10回復または最大となった場合にアイテムを消費
                if (count >= 10 || ownerPlayerStatus.nowPlayerSanValue >= 100)
                {
                    ownerPlayerStatus.UseItem(false);
                    ownerPlayerStatus.ChangeSpeed();
                    ownerPlayerItem.ConsumeItem(ownerPlayerItem.nowIndex);
                    ownerPlayerItem.ChangeCanChangeBringItem(true);
                    yield break;
                }
            }
        }
    }
}

