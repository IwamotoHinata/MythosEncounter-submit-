using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using Fusion;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// プレイヤーの発狂関係を管理するスクリプト
    /// </summary>
    public class PlayerInsanityManager : NetworkBehaviour
    {
        private ReactiveCollection<IInsanity> _insanities = new ReactiveCollection<IInsanity>(); //現在の発狂スクリプトをまとめたList
        public IObservable<CollectionAddEvent<IInsanity>> OnInsanitiesAdd => _insanities.ObserveAdd();//外部に__insanitiesの要素が追加されたときに行う処理を登録できるようにする
        public IObservable<CollectionRemoveEvent<IInsanity>> OnInsanitiesRemove => _insanities.ObserveRemove();//外部に__insanitiesの要素が削除されたときに行う処理を登録できるようにする
        public List<IInsanity> Insanities { get { return _insanities.ToList(); } }//外部に_insanitiesの内容を公開する

        private List<int> _numbers = Enumerable.Range(0, 5).ToList();//0,1,2,3,4のリストを生成(発狂スクリプトが重複しない為に用いる)
        /*
         対応表
         0.EyeParalyze
         1.BodyParalyze
         2.IncreasePulsation
         3.Scream
         4.Hallucination
         */

        [SerializeField] private BoolReactiveProperty _isBrainwashed = new BoolReactiveProperty(false);//洗脳中か否か
        public IObservable<bool> OnPlayerBrainwashedChange { get { return _isBrainwashed; } }//洗脳状態が変化した際にイベントが発行

        private PlayerStatus _myPlayerStatus;
        IInsanity InsanityScript = null;

        private Animator _animator;

        public override void Spawned()
        {
            _myPlayerStatus = GetComponent<PlayerStatus>();
            TryGetComponent<Animator>(out _animator);
            if (!HasStateAuthority)
                return;

            //現在のSAN値が50以下かつSAN値が減った時に発狂スクリプトを付与
            _myPlayerStatus.OnPlayerSanValueChange
                .Where(x => x <= 50 && x < _myPlayerStatus.lastSanValue && _myPlayerStatus.NetworkFinishInit)
                .Subscribe(x =>
                {
                    if (40 < x && x <= 50)
                        AddRandomInsanity(1 - _insanities.Count);

                    else if (30 < x && x <= 40)
                        AddRandomInsanity(2 - _insanities.Count);

                    else if (20 < x && x <= 30)
                        AddRandomInsanity(3 - _insanities.Count);

                    else if (10 < x && x <= 20)
                        AddRandomInsanity(4 - _insanities.Count);

                    else if (0 < x && x <= 10)
                        AddRandomInsanity(5 - _insanities.Count);

                }).AddTo(this);

            _myPlayerStatus.OnPlayerSanValueChange
                .Subscribe(x =>
                {
                    Debug.Log("NowSanValue：" + x + "\nlastSanValue：" + _myPlayerStatus.lastSanValue);
                }).AddTo(this);

            //変更前のSAN値が50以下かつSAN値が回復したときに発狂を回復
            _myPlayerStatus.OnPlayerSanValueChange
                .Where(x => _myPlayerStatus.lastSanValue <= 50 && x > _myPlayerStatus.lastSanValue && _myPlayerStatus.NetworkFinishInit)
                .Subscribe(x => RecoverInsanity(x / 10 - _myPlayerStatus.lastSanValue / 10))
                .AddTo(this);
        }

        public override void FixedUpdateNetwork()
        {

        }

        /// <summary>
        /// ランダムで発狂スクリプトを付与させる 
        /// </summary>
        /// /// <param name="times">関数を叩く回数</param>
        private void AddRandomInsanity(int times)
        {
            if (times == 0 || !HasStateAuthority)
                return;

            for (int i = 0; i < times; i++)
            {
                int random = _numbers[UnityEngine.Random.Range(0, _numbers.Count)];
                //任意のIInsanity関連のスクリプトをアタッチ
                InsanityScript = null;
                RPC_AddInsanityScript(random);

                //洗脳状態で無ければ即座に発狂効果を発揮
                //ホスト側でのみ効果を発動させる。結果をクライアント側に返す
                if (InsanityScript != null && !_isBrainwashed.Value)
                {
                    //EyeParalyzeとHallucination以外の発狂効果のみActive化
                    if (!InsanityScript.GetType().Equals(typeof(EyeParalyze)) && !InsanityScript.GetType().Equals(typeof(Hallucination)))
                        InsanityScript.Active();
                }
                
            }           
        }

        
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_AddInsanityScript(int number)
        {
            switch (number)
            {
                case 0:
                    InsanityScript = this.AddComponent<EyeParalyze>();
                    _insanities.Add(InsanityScript);
                    break;
                case 1:
                    InsanityScript = this.AddComponent<BodyParalyze>();
                    _insanities.Add(InsanityScript);
                    break;
                case 2:
                    InsanityScript = this.AddComponent<IncreasePulsation>();
                    _insanities.Add(InsanityScript);
                    break;
                case 3:
                    InsanityScript = this.AddComponent<Scream>();
                    _insanities.Add(InsanityScript);
                    break;
                case 4:
                    InsanityScript = this.AddComponent<Hallucination>();
                    _insanities.Add(InsanityScript);
                    break;
                default:
                    Debug.Log("想定外の値です。");
                    break;
            }

            //付与された発狂スクリプトを冠した数字が削除
            _numbers.Remove(number);
            InsanityScript.Setup();
            Debug.Log("発狂スクリプト実行");

            //個人の画面上にしか効果のない物（Fusion処理が一度も必要ないやつ）はActive関数を実行
            if (InsanityScript != null && !_isBrainwashed.Value && HasInputAuthority)
            {
                if (InsanityScript.GetType().Equals(typeof(EyeParalyze))|| InsanityScript.GetType().Equals(typeof(Hallucination)))
                    InsanityScript.Active();
                else
                    return;
            }
        }
        

        /// <summary>
        /// 最後に付与された発狂スクリプトを取り除く
        /// </summary>
        /// /// /// <param name="times">関数を叩く回数</param>
        private void RecoverInsanity(int times)
        {
            if (times == 0 || !HasStateAuthority)
                return;

            int number = 0;

            for (int i = 0; i < times; i++)
            {
                switch (_insanities.Last())
                {
                    case EyeParalyze:
                        number = 0;
                        break;
                    case BodyParalyze:
                        number = 1;
                        break;
                    case IncreasePulsation:
                        number = 2;
                        break;
                    case Scream:
                        number = 3;
                        break;
                    case Hallucination:
                        number = 4;
                        break;
                    default:
                        number = -1;
                        break;
                }

                _insanities.Last().Hide();
                RPC_RemoveInsanityScript(number);

                //もうこれ以上回復する必要がないときは終了
                if (_insanities.Count == 0)
                    break;
            }
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_RemoveInsanityScript(int number)
        {
            if (number == -1)
            {
                Debug.LogError("想定外の値です。");
                return;
            }

            //アタッチされていない発狂スクリプトのリスト更新
            _numbers.Add(number);
            _numbers.Sort();

            //発狂スクリプトの削除, 発狂スクリプトのリスト更新
            Destroy((UnityEngine.Object)_insanities.Last());//発狂スクリプトを削除
            _insanities.Remove(_insanities.Last());

        }

        /// <summary>
        /// 洗脳状態になった際に行う処理をまとめたコルーチン
        /// </summary>
        /// <returns></returns>
        public IEnumerator SelfBrainwash()
        {
            //全ての発狂スクリプトを無効化
            for (int i = 0; i < _insanities.Count; i++)
            {
                _insanities[i].Hide();
            }
            _isBrainwashed.Value = true;

            Debug.Log("全ての発狂効果を無効化しました");
            //洗脳効果は60秒続く
            yield return new WaitForSeconds(60f);

            //全ての発狂スクリプトを有効にする
            for (int i = 0; i < _insanities.Count; i++)
            {
                _insanities[i].Active();
            }
            _isBrainwashed.Value = false;

            Debug.Log("全ての発狂効果を有効化しました");
        }

        /// <summary>
        /// 現在付与されている発狂スクリプトの番号をまとめたListを取得できる関数(順番も保存可能)
        /// </summary>
        /// <returns></returns>
        public List<int> GetMyNumbers()
        {
            List<int> numbers = new List<int>();
            for (int i = 0; i < _insanities.Count; i++)
            {
                switch (_insanities[i])
                {
                    case EyeParalyze:
                        numbers.Add(0);
                        break;
                    case BodyParalyze:
                        numbers.Add(1);
                        break;
                    case IncreasePulsation:
                        numbers.Add(2);
                        break;
                    case Scream:
                        numbers.Add(3);
                        break;
                    case Hallucination:
                        numbers.Add(4);
                        break;
                    default:
                        break;
                }
            }

            return numbers;
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_SetAnimBool(string animName, bool value)
        {
            _animator.SetBool(animName, value);
        }

#if UNITY_EDITOR
        private void Update()
        {
            //確認用
            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (_insanities.Count == 0)
                {
                    Debug.Log("発狂してません");
                }

                Debug.Log(this.gameObject.name + "：現在ついている発狂スクリプトの列挙");
                for (int i = 0; i < _insanities.Count; i++)
                {
                    Debug.Log(_insanities[i]);
                }
            }
        }
#endif
    }
}

