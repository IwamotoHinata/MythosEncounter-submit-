using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Ingame.Player
{
    public interface IInsanity
    {
        /// <summary>
        /// ‰Šú‰»ˆ—
        /// </summary>
        void Setup();

        /// <summary>
        /// Œø‰Ê‚Ì“K‰
        /// </summary>
        void Active();

        /// <summary>
        /// Œø‰Ê‚Ì–³Œø‰»
        /// </summary>
        void Hide();
    }
}