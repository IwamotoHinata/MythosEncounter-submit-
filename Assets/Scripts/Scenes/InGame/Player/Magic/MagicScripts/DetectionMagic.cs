using Scenes.Ingame.InGameSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPOOutline;
using Fusion;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// 脱出アイテムを探知し、アウトラインを発生させるためのスクリプト
    /// </summary>
    public class DetectionMagic : Magic
    {
        private bool _debugMode = false;
        private GameObject _detectionBallPrefab;
        [SerializeField] private List<GameObject> _createdDetectionBalls;
        public override void ChangeFieldValue()
        {
            chantTime = 5f;
            consumeSanValue = 5;
            Debug.Log("装備している呪文名：DetectionMagic" + "\n装備してる呪文の詠唱時間：" + chantTime + "\n装備してる呪文のSAN値消費量：" + consumeSanValue);
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
                    RPC_InstantiateBall();

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

        /// <summary>
        /// 呪文を使用したプレイヤーのみにボールを複製。アウトラインを可視化
        /// </summary>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
        public void RPC_InstantiateBall()
        {
            _detectionBallPrefab = (GameObject)Resources.Load("Prefab/Magic/DetectionBall");

            EscapeItem[] escapeItems = FindObjectsOfType<EscapeItem>();

            Vector3 spawnPosition = this.transform.position + this.transform.forward * 2 + Vector3.up * 2;
            //光の玉を脱出アイテムの数だけ生成
            for (int i = 0; i < escapeItems.Length; i++)
            {
                _createdDetectionBalls.Add(Instantiate(_detectionBallPrefab, spawnPosition, Quaternion.identity));
                _createdDetectionBalls[i].GetComponent<DetectionBall>().DetectionItem(escapeItems[i].transform.position, 5.0f);
            }

            //アウトラインを無効化するためのコルーチンをスタート
            StartCoroutine(CancelOutline(escapeItems));
        }

        /// <summary>
        /// 効果時間が切れたらアウトラインを無効化させる
        /// </summary>
        /// <param name="escapeItems">脱出アイテムの配列</param>
        /// <returns></returns>
        private IEnumerator CancelOutline(EscapeItem[] escapeItems)
        {
            yield return new WaitForSeconds(60f);
            for (int i = 0; i < escapeItems.Length; i++)
            {
                //既にプレイヤーが取得済みであれば継続
                if (escapeItems[i] == null)
                    continue;
                else
                    escapeItems[i].gameObject.GetComponent<Outlinable>().enabled = false;
            }
        }

    }
}
