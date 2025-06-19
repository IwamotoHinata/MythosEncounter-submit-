using Scenes.Ingame.Stage;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Scenes.Ingame.Player
{
    public class GeigerCounterMove : MonoBehaviour
    {
        [SerializeField] Transform _playerTransform;
        [SerializeField] PlayerItem _playerItem;
        [SerializeField] TextMeshPro _textMeshProUnit;
        [SerializeField] TextMeshPro _textMeshProNumber;

        public IEnumerator MeasureGeigerCounter()
        {
            RaycastHit hit;
            int floorlayerMask = LayerMask.GetMask("Floor");//床にだけ反応するようにする
            _textMeshProUnit.text = "mSv";
            _textMeshProNumber.text = string.Empty;

            while (true)
            {
                yield return new WaitForSeconds(0.1f);
                Physics.Raycast(_playerTransform.position, -transform.up, out hit, 20f, floorlayerMask);
                Debug.DrawRay(_playerTransform.position, -transform.up * 20f, Color.black);

                if (hit.collider != null)
                {
                    Debug.Log(hit.collider.gameObject.name);
                    float msv = hit.collider.gameObject.GetComponent<StageTile>().Msv;//　真下にあるタイルから温度データ取得
                    _textMeshProNumber.text = msv.ToString();//　温度計に表示される数字を変更
                }
                else
                {
                    Debug.Log("タイルを検知できませんでした");
                }
            }
        }

        //電源オフ時に実行
        public void TurnOffGeigerCounter()
        {
            _textMeshProUnit.text = string.Empty;
            _textMeshProNumber.text = string.Empty;
        }

        /// <summary>
        /// 必要な変数を取得する関数
        /// </summary>
        /// <param name="myPlayer">自分が操作するPlayerのTransform</param>
        /// <param name="playerItem">自分が操作するPlayerのPlayerItem</param>
        public void Init(Transform myPlayer, PlayerItem playerItem)
        {
            if (myPlayer == null || playerItem == null)
            {
                throw new ArgumentNullException("myPlayer or playerItem", "These parameters cannot be null");
            }

            _playerTransform = myPlayer;
            _playerItem = playerItem;
        }
    }
}

