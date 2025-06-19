using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// 呪文の雛形となる抽象クラス.
    /// 子クラスの名前は〇〇Magicとすること
    /// 各子クラスは直接playerにアタッチする予定.
    /// </summary>
    public abstract class Magic : NetworkBehaviour
    {
        [HideInInspector] public float chantTime;//詠唱時間
        [HideInInspector] public float startTime;//詠唱開始時間
        [HideInInspector] public int consumeSanValue;//消費するSAN値

        [Networked] public bool cancelMagic { get; set; } = false;

        [HideInInspector] public PlayerStatus myPlayerStatus;
        [HideInInspector] public PlayerMagic myPlayerMagic;
        public void Start()
        {
            ChangeFieldValue();
        }

        
        /// <summary>
        /// 呪文の効果を実装する関数
        /// </summary>
        public abstract void MagicEffect();

        /// <summary>
        /// 変数を設定する関数
        /// </summary>
        public abstract void ChangeFieldValue();
    }
}
