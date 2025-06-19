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
            int floorlayerMask = LayerMask.GetMask("Floor");//���ɂ�����������悤�ɂ���
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
                    float msv = hit.collider.gameObject.GetComponent<StageTile>().Msv;//�@�^���ɂ���^�C�����牷�x�f�[�^�擾
                    _textMeshProNumber.text = msv.ToString();//�@���x�v�ɕ\������鐔����ύX
                }
                else
                {
                    Debug.Log("�^�C�������m�ł��܂���ł���");
                }
            }
        }

        //�d���I�t���Ɏ��s
        public void TurnOffGeigerCounter()
        {
            _textMeshProUnit.text = string.Empty;
            _textMeshProNumber.text = string.Empty;
        }

        /// <summary>
        /// �K�v�ȕϐ����擾����֐�
        /// </summary>
        /// <param name="myPlayer">���������삷��Player��Transform</param>
        /// <param name="playerItem">���������삷��Player��PlayerItem</param>
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

