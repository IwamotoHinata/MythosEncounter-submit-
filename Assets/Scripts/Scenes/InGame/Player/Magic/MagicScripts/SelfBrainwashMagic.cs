using System.Collections;
using UnityEngine;
using Fusion;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// 自身に洗脳をかけて一時的に正気に戻り，状態異常を無効化する呪文
    /// 効果時間 60秒
    /// </summary>
    public class SelfBrainwashMagic : Magic
    {
        private PlayerInsanityManager _myPlayerInsanityManager;
        private bool _debugMode = false;
        public override void ChangeFieldValue()
        {
            chantTime = 20f;
            consumeSanValue = 10;
            Debug.Log("装備している呪文名：SelfBrainwashMagic" + "\n装備してる呪文の詠唱時間：" + chantTime + "\n装備してる呪文のSAN値消費量：" + consumeSanValue);
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

                //攻撃を食らった際にこのコルーチンを破棄              
                if (cancelMagic == true)
                {
                    cancelMagic = false;
                    yield break;
                }

                //呪文発動
                if (Time.time - startTime >= chantTime)
                {
                    //SAN値減少
                    myPlayerStatus.ChangeSanValue(consumeSanValue, ChangeValueMode.Damage);

                    RPC_SelfBrainWash();
                    yield break;
                }
            }
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_SelfBrainWash()
        {
            Debug.Log("呪文発動！");

            _myPlayerInsanityManager = this.GetComponent<PlayerInsanityManager>();
            //効果発動
            StartCoroutine(_myPlayerInsanityManager.SelfBrainwash());

            //呪文を使えないようにする
            myPlayerMagic.ChangeCanUseMagicBool(false);

            //成功した詠唱の終了を通知
            myPlayerMagic.OnPlayerFinishUseMagic.OnNext(default);
        }
    }
}
