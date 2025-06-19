using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// 対象の出血状態を解除する呪文
    /// 0.5タイル(5.85 / 2)以内
    /// </summary>
    public class StopBleedingMagic : Magic
    {
        private bool _debugMode = false;

        public override void ChangeFieldValue()
        {
            chantTime = 5f;
            consumeSanValue = 5;
            Debug.Log("装備している呪文名：StopBleedingMagic" + "\n装備してる呪文の詠唱時間：" + chantTime + "\n装備してる呪文のSAN値消費量：" + consumeSanValue);
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

                if (_debugMode)
                    Debug.Log(Time.time - startTime);

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
                    //効果範囲を求めて周囲のPlayerのColliderを取得する
                    var myCol = GetComponent<CapsuleCollider>();
                    var direction = new Vector3 { [myCol.direction] = 1 };
                    var offset = myCol.height / 2 - myCol.radius;
                    var Point0 = transform.TransformPoint(myCol.center - direction * offset);
                    var Point1 = transform.TransformPoint(myCol.center + direction * offset);
                    Collider[] results = new Collider[4];//Playerについているコライダーを格納する（最大4人まで）
                    int layerMask = LayerMask.GetMask("Player");//PlayerというレイヤーにあるGameObjectにしかOverlapが当たらないようにする
                    var num = Physics.OverlapCapsuleNonAlloc(Point0, Point1, (float)(5.85 / 2), results , layerMask);

                    //取得したコライダーを元に周辺のPlayerの出血状態を解除
                    for (int i = 0; i < num; i++)
                    {
                        PlayerStatus playerStatus;
                        results[i].gameObject.TryGetComponent<PlayerStatus>(out playerStatus);

                        if(playerStatus != null)
                            playerStatus.ChangeBleedingBool(false);
                    }

                    //SAN値減少
                    myPlayerStatus.ChangeSanValue(consumeSanValue, ChangeValueMode.Damage);

                    //呪文を使えないようにする
                    myPlayerMagic.ChangeCanUseMagicBool(false);

                    //成功した詠唱の終了を通知
                    myPlayerMagic.OnPlayerFinishUseMagic.OnNext(default);
                    yield break;
                }
            }
        }
    }
}