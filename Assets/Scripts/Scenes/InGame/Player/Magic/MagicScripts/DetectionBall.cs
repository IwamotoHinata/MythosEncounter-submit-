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
        /// ���̋ʂ��A�C�e���̏ꏊ�܂ňړ�������
        /// </summary>
        /// <param name="itemPosition">�A�C�e����������W</param>
        /// <param name="duration">�ړ��ɂ����鎞��</param>
        public void DetectionItem(Vector3 itemPosition, float duration)
        {
            this.transform.DOMove(itemPosition, duration).SetDelay(1.0f);
        }

        public void OnTriggerEnter(Collider other)
        {
            //�E�o�A�C�e���ɐG�ꂽ��A�E�g���C����\��������i�f���Łj
            if (other.gameObject.CompareTag("Item") && other.gameObject.GetComponent<EscapeItem>())
            {
                var outline = other.gameObject.GetComponent<Outlinable>();
                outline.enabled = true;

                this.gameObject.SetActive(false);
            }

            //���ۂ͋��炭���̂悤�ɂȂ�
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