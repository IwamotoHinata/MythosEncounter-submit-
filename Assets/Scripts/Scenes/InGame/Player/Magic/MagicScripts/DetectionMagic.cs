using Scenes.Ingame.InGameSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPOOutline;
using Fusion;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// �E�o�A�C�e����T�m���A�A�E�g���C���𔭐������邽�߂̃X�N���v�g
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
            Debug.Log("�������Ă���������FDetectionMagic" + "\n�������Ă�����̉r�����ԁF" + chantTime + "\n�������Ă������SAN�l����ʁF" + consumeSanValue);
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

                //�U����H������ۂɂ��̃R���[�`����j��              
                if (cancelMagic == true)
                {
                    cancelMagic = false;
                    yield break;
                }

                //��������
                if (Time.time - startTime >= chantTime)
                {
                    Debug.Log("���������I");
                    //���ʔ���
                    RPC_InstantiateBall();

                    //SAN�l����
                    myPlayerStatus.ChangeSanValue(consumeSanValue, ChangeValueMode.Damage);

                    //�������g���Ȃ��悤�ɂ���
                    myPlayerMagic.ChangeCanUseMagicBool(false);

                    //���������r���̏I����ʒm
                    myPlayerMagic.OnPlayerFinishUseMagic.OnNext(default);
                    yield break;
                }
            }
        }

        /// <summary>
        /// �������g�p�����v���C���[�݂̂Ƀ{�[���𕡐��B�A�E�g���C��������
        /// </summary>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
        public void RPC_InstantiateBall()
        {
            _detectionBallPrefab = (GameObject)Resources.Load("Prefab/Magic/DetectionBall");

            EscapeItem[] escapeItems = FindObjectsOfType<EscapeItem>();

            Vector3 spawnPosition = this.transform.position + this.transform.forward * 2 + Vector3.up * 2;
            //���̋ʂ�E�o�A�C�e���̐���������
            for (int i = 0; i < escapeItems.Length; i++)
            {
                _createdDetectionBalls.Add(Instantiate(_detectionBallPrefab, spawnPosition, Quaternion.identity));
                _createdDetectionBalls[i].GetComponent<DetectionBall>().DetectionItem(escapeItems[i].transform.position, 5.0f);
            }

            //�A�E�g���C���𖳌������邽�߂̃R���[�`�����X�^�[�g
            StartCoroutine(CancelOutline(escapeItems));
        }

        /// <summary>
        /// ���ʎ��Ԃ��؂ꂽ��A�E�g���C���𖳌���������
        /// </summary>
        /// <param name="escapeItems">�E�o�A�C�e���̔z��</param>
        /// <returns></returns>
        private IEnumerator CancelOutline(EscapeItem[] escapeItems)
        {
            yield return new WaitForSeconds(60f);
            for (int i = 0; i < escapeItems.Length; i++)
            {
                //���Ƀv���C���[���擾�ς݂ł���Όp��
                if (escapeItems[i] == null)
                    continue;
                else
                    escapeItems[i].gameObject.GetComponent<Outlinable>().enabled = false;
            }
        }

    }
}
