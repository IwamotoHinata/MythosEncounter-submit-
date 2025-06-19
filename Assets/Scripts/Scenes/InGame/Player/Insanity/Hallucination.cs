using System.Collections;
using UnityEngine;
using UniRx;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// �����̎�ނ�1��
    /// 1.���܂ɑ��݂��Ȃ���������������
    /// 2.���܂ɑ��݂��鑫�����������Ȃ��Ȃ�
    /// 3.���܂ɑ��݂��Ȃ�������������(�_�ł�����)
    /// 4.������3���Ɉ�x�����_���ŕt�^�i�v���C���[�͂ǂ̌��ʂ������̌�����܂ł킩��Ȃ��j
    /// </summary>
    public class Hallucination : MonoBehaviour,IInsanity
    {
        private PlayerSoundManager _soundManager;
        private AudioListener _audioListener;
        private GameObject _HallucinationPrefab;//���o�p��GameObject�̃v���n�u

        private ReactiveProperty<bool> _decideCoroutineBool = new ReactiveProperty<bool>(true);


        public void Setup()
        {
            _soundManager = GetComponent<PlayerSoundManager>();
            _audioListener = GetComponentInChildren<AudioListener>();
            //_HallucinationPrefab = Resources.Load();

            _decideCoroutineBool
                .Where(x => x == true)
                .Subscribe(x => DecideCoroutine()).AddTo(this);
        }

        public void Active()
        {

        }

        public void Hide()
        {
            
        }

        /// <summary>
        /// �ǂ̌��ʂ�t�^���邩���肷��֐�
        /// </summary>
        private void DecideCoroutine()
        {
            int random = Random.Range(0, 3);
            switch (random)
            {
                case 0:
                    StartCoroutine(HearingHallucination());
                    Debug.Log("�������������n�߂�");
                    break;
                case 1:
                    StartCoroutine(AppearHallucination());
                    Debug.Log("���o�������n�߂�");
                    break;
                case 2:
                    StartCoroutine(TemporarilyLostHearing());
                    Debug.Log("���܂ɒ��͂��Ȃ��Ȃ�");
                    break;
                default:
                    Debug.Log("�z��O�̐��l�ł�");
                    break;
            }

            _decideCoroutineBool.Value = false;
        }

        /// <summary>
        /// ��������������悤�ɂȂ�R���[�`��
        /// </summary>
        /// <returns></returns>
        private IEnumerator HearingHallucination()
        {
            //3���ԑ�����
            for (int i = 0; i < 18; i++)
            {
                int random = Random.Range(0, 5);

                //20%�̊m���Ō�������������
                if (random == 0)
                { 
                    //�����p�̃N���b�v�𗬂��B
                }
                yield return new WaitForSeconds(10.0f);
            }

            _decideCoroutineBool.Value = true;
        }

        /// <summary>
        /// ���o��������悤�ɂȂ�R���[�`��
        /// </summary>
        /// <returns></returns>
        private IEnumerator AppearHallucination()
        {
            GameObject HallucinationObject = null;//���o���i�[����ϐ�
            //3���ԑ�����
            for (int i = 0; i < 18; i++)
            {
                int random = Random.Range(0, 5);

                //20%�̊m���Ō��o��������悤�ɂȂ�
                if (random == 0)
                {
                    //���o���o��������
                }
                else
                { 
                    //���o��j�󂷂�
                }
                yield return new WaitForSeconds(10.0f);
            }

            _decideCoroutineBool.Value = true;
        }

        /// <summary>
        /// ���܂ɒ��͂������ĉ����������Ȃ��Ȃ�R���[�`��
        /// </summary>
        /// <returns></returns>
        private IEnumerator TemporarilyLostHearing()
        {
            //3���ԑ�����
            for (int i = 0; i < 18; i++)
            {
                int random = Random.Range(0, 5);

                //20%�̊m���Ŏ����������Ȃ��Ȃ�
                if (random == 0)
                {
                    _audioListener.enabled = false;
                }
                else
                {
                    _audioListener.enabled = true;
                }
                yield return new WaitForSeconds(10.0f);
            }

            _decideCoroutineBool.Value = true;
        }
    }
}
