using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scenes.Ingame.Enemy;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// �G�̈ړ����x��15�b��90%�J�b�g�������
    /// </summary>
    public class BindMagic : Magic
    {

        private EnemyStatus[] enemyStatuses;
        public override void ChangeFieldValue()
        {
            chantTime = 10f;
            consumeSanValue = 10;
            Debug.Log("�������Ă���������FBindMagic" + "\n�������Ă�����̉r�����ԁF" + chantTime + "\n�������Ă������SAN�l����ʁF" + consumeSanValue);
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
                    enemyStatuses = FindObjectsOfType<EnemyStatus>();
                    for (int i = 0; i < enemyStatuses.Length; i++)
                    {
                        enemyStatuses[i].SetBind(true);
                    }

                    //�S����Ԃ��������邽�߂̃R���[�`���𔭓�
                    StartCoroutine(CancelBind());

                    //SAN�l����
                    myPlayerStatus.ChangeSanValue(consumeSanValue, ChangeValueMode.Damage);

                    //�������g���Ȃ��悤�ɂ���
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
