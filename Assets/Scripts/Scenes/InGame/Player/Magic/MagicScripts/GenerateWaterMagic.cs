using Scenes.Ingame.Enemy;
using System.Collections;
using UnityEngine;
using Fusion;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// ���𐶐��������
    /// </summary>
    public class GenerateWaterMagic : Magic
    {
        private EnemyStatus[] _enemyStatuses;
        private NetworkObject _waterEffect;

        public override void ChangeFieldValue()
        {
            chantTime = 20f;
            consumeSanValue = 10;
            Debug.Log("�������Ă���������FGenerateWaterMagic" + "\n�������Ă�����̉r�����ԁF" + chantTime + "\n�������Ă������SAN�l����ʁF" + consumeSanValue);
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
                    Vector3 spawnPos = GameObject.Find("Stage/Mid").transform.position + new Vector3(0, myPlayerStatus.gameObject.transform.position.y, 0);
                    _waterEffect = RunnerSpawner.RunnerInstance.Spawn(Resources.Load<GameObject>("Prefab/Magic/Water"), spawnPos, Quaternion.identity);

                    _enemyStatuses = FindObjectsOfType<EnemyStatus>();

                    if (_enemyStatuses.Length != 0)
                    { 
                        for (int i = 0; i < _enemyStatuses.Length; i++)
                        {
                            _enemyStatuses[i].SetCheckWaterEffect(true);
                        }
                    }
                    
                    StartCoroutine(CancelWaterEffect());

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

        private IEnumerator CancelWaterEffect()
        {
            yield return new WaitForSeconds(60f);
            RunnerSpawner.RunnerInstance.Despawn(_waterEffect);
            Debug.Log("���̉e���������Ȃ�܂��B");
            if (_enemyStatuses.Length != 0)
            {
                for (int i = 0; i < _enemyStatuses.Length; i++)
                {
                    _enemyStatuses[i].SetCheckWaterEffect(false);
                }
            }
        }
    }
}