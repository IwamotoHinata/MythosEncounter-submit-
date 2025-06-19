using System.Collections;
using UnityEngine;
using Fusion;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// ���g�ɐ��]�������Ĉꎞ�I�ɐ��C�ɖ߂�C��Ԉُ�𖳌����������
    /// ���ʎ��� 60�b
    /// </summary>
    public class SelfBrainwashMagic : Magic
    {
        private PlayerInsanityManager _myPlayerInsanityManager;
        private bool _debugMode = false;
        public override void ChangeFieldValue()
        {
            chantTime = 20f;
            consumeSanValue = 10;
            Debug.Log("�������Ă���������FSelfBrainwashMagic" + "\n�������Ă�����̉r�����ԁF" + chantTime + "\n�������Ă������SAN�l����ʁF" + consumeSanValue);
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
                    //SAN�l����
                    myPlayerStatus.ChangeSanValue(consumeSanValue, ChangeValueMode.Damage);

                    RPC_SelfBrainWash();
                    yield break;
                }
            }
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_SelfBrainWash()
        {
            Debug.Log("���������I");

            _myPlayerInsanityManager = this.GetComponent<PlayerInsanityManager>();
            //���ʔ���
            StartCoroutine(_myPlayerInsanityManager.SelfBrainwash());

            //�������g���Ȃ��悤�ɂ���
            myPlayerMagic.ChangeCanUseMagicBool(false);

            //���������r���̏I����ʒm
            myPlayerMagic.OnPlayerFinishUseMagic.OnNext(default);
        }
    }
}
