using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scenes.Ingame.Enemy;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// 敵の移動速度を15秒間90%カットする呪文
    /// </summary>
    public class BindMagic : Magic
    {

        private EnemyStatus[] enemyStatuses;
        public override void ChangeFieldValue()
        {
            chantTime = 10f;
            consumeSanValue = 10;
            Debug.Log("装備している呪文名：BindMagic" + "\n装備してる呪文の詠唱時間：" + chantTime + "\n装備してる呪文のSAN値消費量：" + consumeSanValue);
        }

        public override void MagicEffect()
        {
            startTime = Time.time;
            StartCoroutine(Magic());
        }

        private IEnumerator Magic()
        { 
            while (true) 
            {
                yield return null;
                //攻撃を食らった際にこのコルーチンを破棄              
                if (cancelMagic == true)
                {
                    cancelMagic = false;
                    yield break;
                }

                //呪文発動
                if (Time.time - startTime >= chantTime)
                {
                    Debug.Log("呪文発動！");
                    //効果発動
                    enemyStatuses = FindObjectsOfType<EnemyStatus>();
                    for (int i = 0; i < enemyStatuses.Length; i++)
                    {
                        enemyStatuses[i].SetBind(true);
                    }

                    //拘束状態を解除するためのコルーチンを発動
                    StartCoroutine(CancelBind());

                    //SAN値減少
                    myPlayerStatus.ChangeSanValue(consumeSanValue, ChangeValueMode.Damage);

                    //呪文を使えないようにする
                    myPlayerMagic.ChangeCanUseMagicBool(false);
                    yield break;
                }
            }
        }

        private IEnumerator CancelBind()
        { 
            yield return new WaitForSeconds(15f);
            for (int i = 0; i < enemyStatuses.Length; i++)
            {
                enemyStatuses[i].SetBind(false);
            }
        }
    }
}
