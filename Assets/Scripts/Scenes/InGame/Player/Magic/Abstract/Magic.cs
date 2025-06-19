using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// �����̐��`�ƂȂ钊�ۃN���X.
    /// �q�N���X�̖��O�́Z�ZMagic�Ƃ��邱��
    /// �e�q�N���X�͒���player�ɃA�^�b�`����\��.
    /// </summary>
    public abstract class Magic : NetworkBehaviour
    {
        [HideInInspector] public float chantTime;//�r������
        [HideInInspector] public float startTime;//�r���J�n����
        [HideInInspector] public int consumeSanValue;//�����SAN�l

        [Networked] public bool cancelMagic { get; set; } = false;

        [HideInInspector] public PlayerStatus myPlayerStatus;
        [HideInInspector] public PlayerMagic myPlayerMagic;
        public void Start()
        {
            ChangeFieldValue();
        }

        
        /// <summary>
        /// �����̌��ʂ���������֐�
        /// </summary>
        public abstract void MagicEffect();

        /// <summary>
        /// �ϐ���ݒ肷��֐�
        /// </summary>
        public abstract void ChangeFieldValue();
    }
}
