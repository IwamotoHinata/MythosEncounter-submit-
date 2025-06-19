using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UniRx;
using Data;//Player�֘A�̃f�[�^������namespace
using Scenes.Ingame.Player;
using Scenes.Ingame.Manager;

namespace Common.Player
{
    /// <summary>
    /// �X�y���̏K���\�����𖞂����Ă���̂��𔻒f����ׂ̊֐�
    /// </summary>
    // Player�ɃA�^�b�`
    // ������������p�̕ϐ���ύX�ł���̂�InputAuthority�������Ă���Player�̂�(�e�X�N���v�g�ŕϐ��ύX�̊֐����Ăяo���O�Ɋm�F����)
    public class SpellUnlockSystem : MonoBehaviour
    {
        public static SpellUnlockSystem Instance;
        private Dictionary<int, bool> _spellUnlockInfo = new Dictionary<int, bool>();
        /*
         ������ID�ɂ��Ă͈ȉ��̃T�C�g���Q��
         https://www.notion.so/tokushima-igc/d61c460df71542ad8c531cc22199ce5f
         */

        //������������p�̕ϐ�
        private int _healedHealth = 0;//Ingame���ŉ񕜂����̗͂̑���
        private int _useDollTimes = 0;//�g����l�`���g������
        private int _useFirecrackerTimes = 0;//���|���g������
        private int _stopBleedingTimes = 0;//�~�����s������
        private int _escapeTimes = 0; //�E�o���s������
        private int _multiEscapeTimes = 0;// �}���`�ŒE�o���s������

        private void Start()
        {
            if (Instance != null)
                Destroy(this);
            else
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);

                //Dictionary�̏�����(�ڐA��ɏ���)
                for(int i = 1; i <= 10; i++)
                    _spellUnlockInfo.Add(i, false);

            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Space))
            {
                for (int i = 1; i <= _spellUnlockInfo.Count; i++)
                {
                    Debug.Log("�X�y��ID:" + i + "�K���\�H:" + _spellUnlockInfo[i]);
                } 
            }


#endif
        }

        /// <summary>
        /// _spellUnlockInfo�̒��g������������֐�.
        /// </summary>
        /// <param name="spellID">�ύX��������������ID</param>
        /// <param name="value">true: �������, false: ���������</param>
        public void SetSpellUnlockInfoData(int spellID, bool value = true)
        {
            _spellUnlockInfo[spellID] = value;
            Debug.Log("Spell No." + spellID + " is unlocked.");
        }

        /// <summary>
        /// _spellUnlockInfo�̒��g���f�[�^�x�[�X�ɑ��M����֐�
        /// </summary>
        public void SendSpellUnlockInfoData()
        { 
            //���S�� or �N���A���ɌĂяo���B
            //ToDo�F���M���鏈��
        }

        public void IncreaseUseFirecrackerTimes()
        {
            _useFirecrackerTimes++;
            if (_useFirecrackerTimes == 1)
            {
                SetSpellUnlockInfoData(2, true);
            }
        }

        public void IncreaseEscapeTimes()
        {
            _escapeTimes++;
            if (_escapeTimes == 3)
            {
                SetSpellUnlockInfoData(4, true);
            }
        }

        public void IncreaseStopBleedingTimes()
        {
            _stopBleedingTimes++;
            if (_stopBleedingTimes == 10)
            {
                SetSpellUnlockInfoData(5, true);
            }
        }

        public void IncreaseUseDollTimes()
        {
            _useDollTimes++;
            if (_useDollTimes == 3)
            {
                SetSpellUnlockInfoData(6, true);
            }
        }

        public void IncreaseMultiEscapeTimes()
        {
            _multiEscapeTimes++;
            if (_multiEscapeTimes == 1)
            {
                SetSpellUnlockInfoData(7, true);
            }
        }

        public void IncreaseHealedHealth(int value)
        {
            _healedHealth += value;
            Debug.Log("���݂̉񕜑��ʁF" + _healedHealth);
            if (_healedHealth >= 1000&& _spellUnlockInfo[8] == false)
            {
                SetSpellUnlockInfoData(8, true);
            }
        }
    }

}