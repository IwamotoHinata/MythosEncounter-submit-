using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Scenes.Ingame.Player
{
    public class BandageEffect : ItemEffect
    {
        private float _startTime;
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
            if (ownerPlayerStatus.nowBleedingValue)
                StartCoroutine(UseBandage());
            else
                Debug.Log("出血状態ではないので使用しませんでした。");
        }

        public IEnumerator UseBandage()
        {
            Debug.Log("包帯使う");
            SoundManager.Instance.PlaySe("se_bandage00", transform.position);

            //アイテム使用直後にステータス変更を行う
            ownerPlayerStatus.UseItem(true);
            ownerPlayerStatus.ChangeSpeed();
            ownerPlayerItem.ChangeCanChangeBringItem(false);
            //ToDo:Playerに包帯を巻くエフェクトを発生させる

            _startTime = Time.time;
            Debug.Log(_startTime);

            while (true)
            {
                yield return null;
                //攻撃を食らった際にこのコルーチンを破棄              
                if (_stopCoroutineBool == true)
                {
                    Debug.Log("包帯使用のコルーチンを破棄");
                    _stopCoroutineBool = false;
                    ownerPlayerStatus.UseItem(false);
                    ownerPlayerStatus.ChangeSpeed();
                    ownerPlayerItem.ChangeCanChangeBringItem(true);
                    yield break;
                }

                if (Time.time - _startTime >= 5.0f)
                {
                    ownerPlayerStatus.ChangeBleedingBool(false);
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

