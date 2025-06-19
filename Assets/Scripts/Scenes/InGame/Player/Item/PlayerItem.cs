using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Linq;
using EPOOutline;
using Scenes.Ingame.InGameSystem;
using Fusion;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// アイテムに関する処理をまとめたクラス
    /// 1.アイテムスロットにあるアイテムを使用する
    /// 2.所持アイテムの管理
    /// 3.アイテムスロットの位置の管理
    /// </summary>
    public class PlayerItem : NetworkBehaviour
    {
        [SerializeField] private bool _isOnlineMode;

        private PlayerStatus _myPlayerStatus;

        //アイテム関係
        private ReactiveProperty<int> _nowIndex = new ReactiveProperty<int>();//選択中のアイテムスロット番号
        public GameObject myRightHand;//手のこと
        public GameObject nowBringItem;//現在手に持っているアイテム

        //Ray関連
        [SerializeField] GameObject _eyePoint;//playerの目線
        [SerializeField] private float _getItemRange;//アイテムを入手できる距離
        private bool _debugMode = false;

        //アイテムスロット（UI）の操作関連
        private float scrollValue;
        [SerializeField] private float scrollSense = 10;//マウスホイールの感度

        //アイテムを使用したイベントを発行
        private Subject<Unit> _useItem = new Subject<Unit>();

        //UniRx関係
        private Subject<String> _popActive = new Subject<String>();
        private ReactiveCollection<ItemSlotStruct> _itemSlot = new ReactiveCollection<ItemSlotStruct>();//現在所持しているアイテムのリスト

        //アイテムクラス関連
        [SerializeField] private GameObject _compass;
        [SerializeField] private Light _spotLight;
        [SerializeField] private ThermometerMove _thermometerMove;
        [SerializeField] private GeigerCounterMove _geigerCounterMove;

        [SerializeField] public ShotgunBurst _shotgunBurst;
        
        //TrapFood関連
        [SerializeField] private GameObject _trapFood;
        private const float TILELENGTH = 5.85f;
        private GameObject _createdTrapFood;
        private bool _isCanCreateTrapFood;
        public bool IsCanCreateTrapFood { get => _isCanCreateTrapFood; }
        public GameObject CreatedTrapFood { get => _createdTrapFood; }

        //アイテムデバッグ用
        [SerializeField] private GameObject _itemForDebug;

        private List<HandLightState> _switchHandLight = new List<HandLightState>();//懐中電灯のon/off状態保存用
        private List<bool> _switchGeigerCounter = new List<bool>();//放射線測定器のon/off状態保存用  

        public List<ItemSlotStruct> ItemSlots { get { return _itemSlot.ToList(); } }//外部に_itemSlotの内容を公開する
        public int nowIndex { get => _nowIndex.Value; }
        public List<HandLightState> SwitchHandLights { get { return _switchHandLight.ToList(); } }
        public List<bool> SwitchGeigerCounter { get { return _switchGeigerCounter.ToList(); } }

        public IObservable<int> OnNowIndexChange { get { return _nowIndex; } }//外部で_nowIndexの値が変更されたときに行う処理を登録できるようにする
        public IObservable<String> OnPopActive { get { return _popActive; } }
        public IObservable<CollectionReplaceEvent<ItemSlotStruct>> OnItemSlotReplace => _itemSlot.ObserveReplace();//外部に_itemSlotの要素が変更されたときに行う処理を登録できるようにする
        public IObservable<Unit> OnUseItem { get { return _useItem; } }
        private Outlinable _lastOutlinable = null;
        private GameObject _lastGameobject = null;


        //ネットワーク同期用
        private ChangeDetector _changeDetector;
        [Networked] private int _nowIndexNetworked { get; set; }
        [Networked] private bool _isCanChangeBringItem { get; set; } = true;//手に持つアイテムの変更を許可するか否か
        [Networked] private bool _isCanUseItem { get; set; } = true;
        private TickTimer _intractInterval = TickTimer.None;
        private TickTimer _itemInterval = TickTimer.None;

        private Subject<GameObject> _forwardItemCheck = new Subject<GameObject>();//目の前にあるものに応じて処理を行うイベント


        int _layerMask;//アイテム,ステージインタラクト, 壁のみ検知 
        RaycastHit hit = new RaycastHit();

        public override void Spawned()
        {
            _myPlayerStatus = GetComponent<PlayerStatus>();

            _layerMask = LayerMask.GetMask("Item") | LayerMask.GetMask("StageIntract") | LayerMask.GetMask("Wall");//Item, StageIntract,WallというレイヤーにあるGameObjectにしかrayが当たらないようにする

            //今後はingame前のアイテムの所持状況を代入させる。α版は初期化
            ItemSlotStruct init = new ItemSlotStruct();
            for (int i = 0; i < 7; i++)
            {
                init.ChangeInfo();
                _itemSlot.Add(init);
            }

            //懐中電灯の状態をNotActiveでスロット分作っておく
            HandLightState LightSwitch = HandLightState.NotActive;
            for (int i = 0; i < 7; i++)
            {
                _switchHandLight.Add(LightSwitch);
                _switchGeigerCounter.Add(false);
            }

            //アイテムスロットの選択状態が変わったときに、手元に適切なアイテムを出現させる
            _nowIndex
                .Skip(1)
                .Where(_ => _isCanChangeBringItem)
                .Subscribe(_ =>
                {
                    ChangeBringItem();
                }).AddTo(this);

            _itemSlot.ObserveReplace()
                .Where(_ => HasStateAuthority)
                .Subscribe(replaced =>
                {
                    if (replaced.NewValue.myItemData != null)
                    {
                        int itemID = replaced.NewValue.myItemData.itemID;
                        int isAvailable = (int)replaced.NewValue.myItemSlotStatus;
                        RPC_SetPlayerUI(itemID, isAvailable, replaced.Index);
                    }
                    else
                    {
                        int isAvailable = (int)replaced.NewValue.myItemSlotStatus;
                        RPC_SetPlayerUI(-1, isAvailable, replaced.Index);
                    }
                }).AddTo(this);

            //入力権限のある者のみ出来る処理を記述
            if (this.Object.HasInputAuthority)
            {
                //一部アイテム用の処理（今後消えるかも）
                _compass = GameObject.Find("UsingCompass");
                if (_compass != null)
                {
                    _compass.SetActive(false);
                }
                if(GameObject.Find("HandLight"))
                {
                    _spotLight = GameObject.Find("HandLight").GetComponent<Light>();
                    _spotLight.enabled = false;
                }
                _thermometerMove = FindObjectOfType<ThermometerMove>();
                _geigerCounterMove = FindObjectOfType<GeigerCounterMove>();
                _shotgunBurst = FindObjectOfType<ShotgunBurst>();
                //一部アイテムについたスクリプトの変数設定
                _thermometerMove.Init(this.gameObject.transform, this);
                _geigerCounterMove.Init(this.gameObject.transform, this);
                _shotgunBurst.Init(_myPlayerStatus);

                //アイテムの非表示
                _thermometerMove.gameObject.SetActive(false);
                _geigerCounterMove.gameObject.SetActive(false);
                _shotgunBurst.gameObject.SetActive(false);

                _forwardItemCheck
                    .Subscribe(forwardObj =>
                    {
                        if (!HasInputAuthority)
                            return;

                        if (forwardObj == null)
                        {
                            IntractEvent(false, "");
                            return;
                        }



                        if (forwardObj.TryGetComponent(out IInteractable interactable))
                        {
                            //interactable.Intract(_myPlayerStatus);

                            if (forwardObj.CompareTag("Item") && forwardObj.TryGetComponent(out EscapeItem escapeItem))
                            {
                                //脱出アイテムだった時
                                IntractEvent(false, "");
                                _lastOutlinable = forwardObj.GetComponent<Outlinable>();
                                IntractEvent(true, "拾う");//アウトライン表示
                            }
                            else if (forwardObj.CompareTag("Item") && forwardObj.TryGetComponent(out ItemEffect item))
                            {
                                //脱出アイテム以外のアイテムの時
                                IntractEvent(false, "");
                                //　string name = item.GetItemData().itemName;
                                // アイテムの場合、「拾う」に統一
                                _lastOutlinable = forwardObj.GetComponent<Outlinable>();
                                IntractEvent(true, "拾う");//アウトライン表示
                            }
                            else if (forwardObj.CompareTag("StageIntract"))
                            {
                                //StageIntract（ドアなど）のとき
                                IntractEvent(false, "");
                                _lastOutlinable = forwardObj.GetComponent<Outlinable>();
                                IntractEvent(true, interactable.ReturnPopString());//アウトライン表示
                            }
                        }
                    }).AddTo(this);
            }

            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);


            _shotgunBurst.OnChangeItemStatus
                .Skip(1)
                .Subscribe(value =>
                {
                    ChangeCanUseItem(value);
                    ChangeCanChangeBringItem(value);
                }).AddTo(this);
        }

        private void IntractEvent(bool outlineValue, string popString)
        {
            if (_lastOutlinable != null)
                _lastOutlinable.enabled = outlineValue;

            _popActive.OnNext(popString);
        }

        /// <summary>
        /// ホスト上で管理してるアイテムスロットが変更された際にクライアント側のUIを変更させる関数
        /// クライアント側でもアイテムスロットの値は更新される
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="isAvailable"></param>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
        public void RPC_SetPlayerUI(int itemID, int isAvailable, int index)
        {
            if (HasStateAuthority)
                return;

            if (itemID == -1)//アイテムが無い場合
            {
                _itemSlot[index] = new ItemSlotStruct(null, (ItemSlotStatus)isAvailable);
                //_itemSlot[index] = new ItemSlotStruct(null, ItemSlotStatus.unavailable);//デバッグ用
                return;
            }

            ItemData tmpData = Resources.Load<ItemDataBase>("ItemDataBase").GetItemData(itemID - 1);
            ItemSlotStatus tmpStatus = (ItemSlotStatus)isAvailable;

            _itemSlot[index] = new ItemSlotStruct(tmpData, tmpStatus);
        }


        /// <summary>
        ///手に持つアイテムを生成するための関数
        /// </summary>
        public void ChangeBringItem()
        {
            if (!HasStateAuthority)//ホスト側にのみ処理を実行
                return;

            //他にアイテムを手に持っていたら、それを破壊
            if (nowBringItem != null)
            {
                Runner.Despawn(nowBringItem.GetComponent<NetworkObject>());
            }

            //手に選択したアイテムを出現させる
            if (_itemSlot[_nowIndex.Value].myItemData != null)
            {
                var obj = Runner.Spawn(_itemSlot[_nowIndex.Value].myItemData.prefab, myRightHand.transform.position, _itemSlot[_nowIndex.Value].myItemData.prefab.transform.rotation);
                SettingBringItem(obj);
            }
        }

        /// <summary>
        ///手に持つアイテムに情報を渡すための関数
        /// </summary>
        public void SettingBringItem(NetworkObject spawnedItem)
        {
            if (!HasStateAuthority)//ホスト側にのみ情報を渡す
                return;

            nowBringItem = spawnedItem.gameObject;
            nowBringItem.transform.position = myRightHand.transform.position;
            nowBringItem.transform.SetParent(myRightHand.transform);//親子関係の設定      

            //視覚上のバグを無くすために手に持っている間はColliderを消す
            nowBringItem.GetComponent<Collider>().enabled = false;

            //アイテムを切り替えた際に情報を流す           
            var effect = nowBringItem.GetComponent<ItemEffect>();
            effect.ownerPlayerStatus = _myPlayerStatus;
            effect.ownerPlayerItem = this;
            effect.OnPickUp();//各アイテムの拾った時の処理を実行させる
            nowBringItem.GetComponent<Rigidbody>().useGravity = false;//アイテムを持った時に重力の影響を受けないようにする
        }


        public override void FixedUpdateNetwork()
        {
            /*--- 全員共通で行わせる処理 ---*/
            //前方にあるアイテム or 障害物を認識する。
            if (Physics.Raycast(_eyePoint.transform.position, _eyePoint.transform.forward, out hit, _getItemRange, _layerMask))//設定した距離にあるアイテムを認知
            {

                //デバッグモード処理
                if (_debugMode)
                {
                    Debug.LogError(hit.collider.gameObject.name);
                    Debug.DrawRay(_eyePoint.transform.position, _eyePoint.transform.forward, Color.black);
                }

                //光線に当たったアイテムが変わっていた場合
                if (hit.collider.gameObject != _lastGameobject)
                {
                    _lastGameobject = hit.collider.gameObject;
                    _forwardItemCheck.OnNext(hit.collider.gameObject);
                }
            }
            else
            {
                _lastGameobject = null;
                _forwardItemCheck.OnNext(null);
            }


            /* --- サーバ(ホスト)側のみの処理に ---*/
            if (HasStateAuthority)
            {
                //入力に基づいた処理を実行
                var input = GetInput<GameplayInput>();
                ProcessInput(input.GetValueOrDefault());


                //アイテムを手元に固定させる
                if (nowBringItem != null)
                    nowBringItem.transform.position = myRightHand.transform.position;
            }

            /*--- クライアント側かつInputAuthority持ってるやつの処理 ---*/
            if (!HasStateAuthority && HasInputAuthority)
            {
                foreach (var change in _changeDetector.DetectChanges(this))
                {
                    switch (change)
                    {
                        case nameof(_nowIndexNetworked):
                            _nowIndex.Value = _nowIndexNetworked;
                            break;
                        case nameof(_isCanChangeBringItem):
                            break;
                        case nameof(_isCanUseItem):
                            break;
                        default:
                            break;
                    }
                }
            }

        }


        private void ProcessInput(GameplayInput input)
        {
            if (_itemSlot[(int)input.MouseWheelValue].myItemSlotStatus != ItemSlotStatus.unavailable && _isCanChangeBringItem)
            {
                _nowIndexNetworked = (int)input.MouseWheelValue;
                _nowIndex.Value = _nowIndexNetworked;
            }


            //アイテム拾う or インタラクト処理
            if (input.Buttons.IsSet(EInputButton.GetItemOrIntract) && _lastGameobject != null && _intractInterval.ExpiredOrNotRunning(Runner))
            {
                if (_lastGameobject.TryGetComponent(out IInteractable interactable))
                {
                    interactable.Intract(_myPlayerStatus);
                    //_intractInterval = TickTimer.CreateFromSeconds(Runner, 1.0f);
                    Debug.Log("インタラクト処理");
                }
                else
                {
                    Debug.LogWarning($"{_lastGameobject.name} からIInteractableを取得できませんでした");

                }
            }


            //アイテムを使用する処理
            if (input.Buttons.IsSet(EInputButton.UseItem) && _itemSlot[_nowIndex.Value].myItemData != null && _isCanUseItem && !_myPlayerStatus.IsUseItem && _itemInterval.ExpiredOrNotRunning(Runner))
            {
                Debug.Log(this.gameObject.name + "がアイテムを使用します。");

                //何回も使えるアイテムであれば、0.5秒だけインターバルを挟む
                if (!_itemSlot[_nowIndex.Value].myItemData.isSingleUse)
                {
                    //_itemInterval = TickTimer.CreateFromSeconds(Runner, 0.5f);
                }

                //アイテムを使用
                _useItem.OnNext(default);
                nowBringItem.GetComponent<ItemEffect>().Effect();
            }

            //アイテムを捨てる処理
            if (input.Buttons.IsSet(EInputButton.ThrowItem) && _itemSlot[_nowIndex.Value].myItemData != null)
            {
                //アイテム捨てるときの処理
                nowBringItem.GetComponent<ItemEffect>().OnThrow();

                //アイテムを近くに投げ捨てる
                var rb = nowBringItem.GetComponent<Rigidbody>();
                nowBringItem.GetComponent<Collider>().enabled = true;
                nowBringItem.transform.parent = null;
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.AddForce(_eyePoint.transform.forward * 300);

                //アイテムスロットのListを更新
                nowBringItem = null;
                _itemSlot[_nowIndex.Value] = new ItemSlotStruct();
            }

        }


#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                for (int i = 0; i < _itemSlot.Count; i++)
                {
                    if (_itemSlot[i].myItemData != null)
                        Debug.LogWarning("スロット番号：" + i + "アイテム名" + _itemSlot[i].myItemData.itemName);
                    else
                    {
                        if (_itemSlot[i].myItemSlotStatus == ItemSlotStatus.available)
                            Debug.LogWarning("スロット番号：" + i + "にアイテムはありません。スロットは有効");
                        else
                            Debug.LogWarning("スロット番号：" + i + "にアイテムはありません。スロットは無効");
                    }

                }
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                int y = 0;
                foreach (var x in _itemSlot)
                {
                    if (x.myItemData != null)
                    {
                        y += 1;
                    }
                }
                Debug.Log($"アイテム所持数：{y}");
            }


        }
#endif



        /// <summary>
        /// アイテムスロットのリストを変更。
        /// </summary>
        /// <param name="index">変更したいリストの順番</param>
        /// <param name="value">代入する構造体</param>

        public void ChangeListValue(int index, ItemSlotStruct value)
        {
            _itemSlot[index] = value;
        }

        /// <summary>
        /// アイテムを使い切るときに呼び出す。Listの変更（初期化）に使う
        /// </summary>
        /// <param name="index">変更したいリストの順番</param>
        public void ConsumeItem(int index)
        {
            if (nowBringItem != null)
                Runner.Despawn(nowBringItem.GetComponent<NetworkObject>());

            ItemSlotStruct temp = new ItemSlotStruct();
            _itemSlot[index] = temp;

            Debug.Log("ConsumeItem");
        }

        public void ChangeCanUseItem(bool value)
        {
            _isCanUseItem = value;
        }

        public void ChangeCanChangeBringItem(bool value)
        {
            _isCanChangeBringItem = value;
        }

        public void CheckHaveDoll()
        {
            if (!Object.HasStateAuthority)
                return;

            for (int i = 0; i < 7; i++)
            {
                if (_itemSlot[i].myItemData != null)
                {
                    if (_itemSlot[i].myItemData.itemID == 7)
                    {
                        //仮のアイテムを生成して、死亡時の効果を起動させる
                        NetworkObject Item = Runner.Spawn(_itemSlot[i].myItemData.prefab);
                        Item.GetComponent<DollEffect>().UniqueEffect(_myPlayerStatus);

                        //アイテム破壊とアイテムスロットの初期化
                        Runner.Despawn(Item);
                        if (_nowIndex.Value == i && nowBringItem != null)
                        {
                            Runner.Despawn(nowBringItem.GetComponent<NetworkObject>());
                        }
                        ItemSlotStruct temp = new ItemSlotStruct();
                        _itemSlot[i] = temp;

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 懐中電灯を起動・停止するための関数
        /// </summary>
        /// <param name="value">懐中電灯を起動するか, 停止するか</param>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
        public void RPC_ActiveHandLight(bool value)
        {
            _spotLight.enabled = value;
            _myPlayerStatus.ChangeLightRange(value);
        }


        /// <summary>
        /// 懐中電灯のON/OFFを切り替える関数
        /// </summary>
        /// <param name="state">ON/OFF</param>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
        public void RPC_ChangeSwitchHandLight(HandLightState state)
        {
            _switchHandLight[_nowIndex.Value] = state;
        }

        /// <summary>
        /// コンパスを持つかどうか切り替える関数
        /// </summary>
        /// <param name="value">コンパスの表示状態</param>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
        public void RPC_ActiveCompass(bool value)
        {
            _compass.SetActive(value);
        }


        /// <summary>
        /// 気温計を持つかどうか切り替える関数
        /// </summary>
        /// <param name="value">気温計の表示状態</param>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
        public void RPC_ActiveThermometer(bool value)
        {
            _thermometerMove.gameObject.SetActive(value);
        }

        /// <summary>
        /// 気温計を使い測定を開始させる関数
        /// </summary>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
        public void RPC_UseThermometer()
        {
            _thermometerMove.StartCoroutine("MeasureTemperature");
        }

        /// <summary>
        /// 放射線測定器を持つかどうか切り替える関数
        /// </summary>
        /// <param name="value">放射線測定器の表示状態</param>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
        public void RPC_ActiveGeigerCounter(bool value)
        {
            _geigerCounterMove.gameObject.SetActive(value);
        }

        /// <summary>
        /// 放射線測定器の電源のon/offを変更する関数
        /// </summary>
        /// <param name="value">on/off</param>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
        public void RPC_ChangeSwitchGeigerCounter(bool value)
        {
            _switchGeigerCounter[_nowIndex.Value] = value;
        }

        /// <summary>
        /// 放射線測定器の動作状態を変更する関数
        /// </summary>
        /// <param name="value">放射線測定器の動作状態</param>
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
        public void RPC_UseGeigerCounter(bool value)
        {
            if (value)//測定を開始させる場合
            {
                _geigerCounterMove.StartCoroutine("MeasureGeigerCounter");
            }
            else//測定を止める場合
            {
                _geigerCounterMove.StopCoroutine("MeasureGeigerCounter");
                _geigerCounterMove.TurnOffGeigerCounter();
            }
        }

        //地面判定を確認し、餌を生成する処理
        public IEnumerator CreateTrapFood()
        {
            RaycastHit hit;
            int floorlayerMask = LayerMask.GetMask("Floor");

            while (_itemSlot[_nowIndex.Value].myItemData != null && _itemSlot[_nowIndex.Value].myItemData.itemID == 20)
            {
                yield return null;

                Physics.Raycast(_eyePoint.transform.position, _eyePoint.transform.forward, out hit, TILELENGTH, floorlayerMask);
                Debug.DrawRay(_eyePoint.transform.position, _eyePoint.transform.forward * TILELENGTH, Color.black);

                if (hit.collider != null)
                {
                    //プレビューの作成・移動
                    if (_createdTrapFood == null)//プレビューがないときは作成
                    {
                        _createdTrapFood = Runner.Spawn(_trapFood, hit.point, _trapFood.transform.rotation).gameObject;
                    }
                    else if (_createdTrapFood.activeInHierarchy)// プレビューがありかつアクティブ状態の時
                    {
                        _createdTrapFood.transform.position = hit.point;
                    }
                    else// プレビューがあるがアクティブ状態でない時
                    {
                        _createdTrapFood.SetActive(true);
                        _createdTrapFood.GetComponent<TrapFoodCheckCollider>().ChangeTrigger(false);
                        _createdTrapFood.transform.position = hit.point;
                    }

                    if (_createdTrapFood.GetComponent<TrapFoodCheckCollider>().IsTriggered && _isCanCreateTrapFood)
                    {
                        _isCanCreateTrapFood = false;
                    }
                    else if (!_createdTrapFood.GetComponent<TrapFoodCheckCollider>().IsTriggered && !_isCanCreateTrapFood)
                    {
                        _isCanCreateTrapFood = true;
                    }
                }
                else
                {
                    _isCanCreateTrapFood = false;
                    if (_createdTrapFood != null)//プレビューが作成されていたら非アクティブにする
                    {
                        _createdTrapFood.SetActive(false);
                    }
                }
            }

            //コルーチンが終わるときの処理を記述
            DestroyTrapFood();
        }

        //餌を設置する関数
        public void PutTrapFood()
        {
            RPC_TrapFoodInit(_createdTrapFood.GetComponent<NetworkObject>());
            _createdTrapFood = null;
            ConsumeItem(nowIndex);
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
        public void RPC_TrapFoodInit(NetworkObject food)
        {
            food.gameObject.transform.GetChild(0).gameObject.SetActive(true);
            food.GetComponent<ItemInstract>().enabled = true;
            food.gameObject.layer = 10;// レイヤーをItemに変更し、拾えるようにする
        }

        public void DestroyTrapFood()
        {
            if (_createdTrapFood != null)
            {
                Debug.Log("破壊");
                Runner.Despawn(_createdTrapFood.GetComponent<NetworkObject>());
            }
        }
    }
}