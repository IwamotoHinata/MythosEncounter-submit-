using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// �Ώۂ̏o����Ԃ������������
    /// 0.5�^�C��(5.85 / 2)�ȓ�
    /// </summary>
    public class StopBleedingMagic : Magic
    {
        private bool _debugMode = false;

        public override void ChangeFieldValue()
        {
            chantTime = 5f;
            consumeSanValue = 5;
            Debug.Log("�������Ă���������FStopBleedingMagic" + "\n�������Ă�����̉r�����ԁF" + chantTime + "\n�������Ă������SAN�l����ʁF" + consumeSanValue);
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
                    //���ʔ͈͂����߂Ď��͂�Player��Collider���擾����
                    var myCol = GetComponent<CapsuleCollider>();
                    var direction = new Vector3 { [myCol.direction] = 1 };
                    var offset = myCol.height / 2 - myCol.radius;
                    var Point0 = transform.TransformPoint(myCol.center - direction * offset);
                    var Point1 = transform.TransformPoint(myCol.center + direction * offset);
                    Collider[] results = new Collider[4];//Player�ɂ��Ă���R���C�_�[���i�[����i�ő�4�l�܂Łj
                    int layerMask = LayerMask.GetMask("Player");//Player�Ƃ������C���[�ɂ���GameObject�ɂ���Overlap��������Ȃ��悤�ɂ���
                    var num = Physics.OverlapCapsuleNonAlloc(Point0, Point1, (float)(5.85 / 2), results , layerMask);

                    //�擾�����R���C�_�[�����Ɏ��ӂ�Player�̏o����Ԃ�����
                    for (int i = 0; i < num; i++)
                    {
                        PlayerStatus playerStatus;
                        results[i].gameObject.TryGetComponent<PlayerStatus>(out playerStatus);

                        if(playerStatus != null)
                            playerStatus.ChangeBleedingBool(false);
                    }

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
    }
}