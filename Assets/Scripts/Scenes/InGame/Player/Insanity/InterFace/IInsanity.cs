using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Ingame.Player
{
    public interface IInsanity
    {
        /// <summary>
        /// ����������
        /// </summary>
        void Setup();

        /// <summary>
        /// ���ʂ̓K��
        /// </summary>
        void Active();

        /// <summary>
        /// ���ʂ̖�����
        /// </summary>
        void Hide();
    }
}