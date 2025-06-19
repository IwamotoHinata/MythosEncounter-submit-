using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using EPOOutline;
using Scenes.Ingame.InGameSystem;

namespace Scenes.Ingame.Player
{
    public class DetectionBall : MonoBehaviour
    {
        /// <summary>
        /// 光の玉をアイテムの場所まで移動させる
        /// </summary>
        /// <param name="itemPosition">アイテムがある座標</param>
        /// <param name="duration">移動にかける時間</param>
        public void DetectionItem(Vector3 itemPosition, float duration)
        {
            this.transform.DOMove(itemPosition, duration).SetDelay(1.0f);
        }

        public void OnTriggerEnter(Collider other)
        {
            //脱出アイテムに触れたらアウトラインを表示させる（デモ版）
            if (other.gameObject.CompareTag("Item") && other.gameObject.GetComponent<EscapeItem>())
            {
                var outline = other.gameObject.GetComponent<Outlinable>();
                outline.enabled = true;

                this.gameObject.SetActive(false);
            }

            //実際は恐らくこのようになる
            /*
            if (other.gameObject.CompareTag("Item") && other.gameObject.GetComponent<EscapeItem>())
            {
                var outline = other.gameObject.GetComponent<Outlinable>();
                outline.enabled = true;
            }
            */
        }
    }
}