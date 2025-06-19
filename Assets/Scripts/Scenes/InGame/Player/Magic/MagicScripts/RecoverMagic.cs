using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Scenes.Ingame.Player
{
    public class RecoverMagic : Magic
    {
        private GameObject _recoverEffect;

        public override void ChangeFieldValue()
        {
            chantTime = 15f;
            consumeSanValue = 10;
            Debug.Log("�������Ă���������FRecoverMagic" + "\n�������Ă�����̉r�����ԁF" + chantTime + "\n�������Ă������SAN�l����ʁF" + consumeSanValue);

            _recoverEffect = Resources.Load<GameObject>("Effect/RecoverEffect/RecoverEffect");
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
                    myPlayerStatus.ChangeHealth(0.5f, ChangeValueMode.Heal);
                    var effect = RunnerSpawner.RunnerInstance.Spawn(_recoverEffect, myPlayerStatus.gameObject.transform.position, Quaternion.identity);
                    effect.transform.SetParent(myPlayerStatus.gameObject.transform);

                    //SAN�l����
                    myPlayerStatus.ChangeSanValue(consumeSanValue, ChangeValueMode.Damage);

                    //�������g���Ȃ��悤�ɂ���
                    myPlayerMagic.ChangeCanUseMagicBool(false);

                    //���������r���̏I����ʒm
                    myPlayerMagic.OnPlayerFinishUseMagic.OnNext(default);

                    //�w�莞�Ԍ�G�t�F�N�g��j��
                    yield return new WaitForSeconds(effect.GetComponent<VisualEffect>().GetFloat("LifeTime"));

                    if(effect != null)
                        RunnerSpawner.RunnerInstance.Despawn(effect);

                    yield break;
                }
            }
        }
    }

}