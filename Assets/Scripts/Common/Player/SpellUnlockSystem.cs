using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UniRx;
using Data;//Player関連のデータを扱うnamespace
using Scenes.Ingame.Player;
using Scenes.Ingame.Manager;

namespace Common.Player
{
    /// <summary>
    /// スペルの習得可能条件を満たしているのかを判断する為の関数
    /// </summary>
    // Playerにアタッチ
    // 呪文解放条件用の変数を変更できるのはInputAuthorityを持っているPlayerのみ(各スクリプトで変数変更の関数を呼び出す前に確認せよ)
    public class SpellUnlockSystem : MonoBehaviour
    {
        public static SpellUnlockSystem Instance;
        private Dictionary<int, bool> _spellUnlockInfo = new Dictionary<int, bool>();
        /*
         呪文やIDについては以下のサイトを参照
         https://www.notion.so/tokushima-igc/d61c460df71542ad8c531cc22199ce5f
         */

        //呪文解放条件用の変数
        private int _healedHealth = 0;//Ingame内で回復した体力の総量
        private int _useDollTimes = 0;//身代わり人形を使った回数
        private int _useFirecrackerTimes = 0;//爆竹を使った回数
        private int _stopBleedingTimes = 0;//止血を行った回数
        private int _escapeTimes = 0; //脱出を行った回数
        private int _multiEscapeTimes = 0;// マルチで脱出を行った回数

        private void Start()
        {
            if (Instance != null)
                Destroy(this);
            else
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);

                //Dictionaryの初期化(移植後に消す)
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
                    Debug.Log("スペルID:" + i + "習得可能？:" + _spellUnlockInfo[i]);
                } 
            }


#endif
        }

        /// <summary>
        /// _spellUnlockInfoの中身を書き換える関数.
        /// </summary>
        /// <param name="spellID">変更させたい呪文のID</param>
        /// <param name="value">true: 呪文解放, false: 呪文未解放</param>
        public void SetSpellUnlockInfoData(int spellID, bool value = true)
        {
            _spellUnlockInfo[spellID] = value;
            Debug.Log("Spell No." + spellID + " is unlocked.");
        }

        /// <summary>
        /// _spellUnlockInfoの中身をデータベースに送信する関数
        /// </summary>
        public void SendSpellUnlockInfoData()
        { 
            //死亡時 or クリア時に呼び出す。
            //ToDo：送信する処理
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
            Debug.Log("現在の回復総量：" + _healedHealth);
            if (_healedHealth >= 1000&& _spellUnlockInfo[8] == false)
            {
                SetSpellUnlockInfoData(8, true);
            }
        }
    }

}