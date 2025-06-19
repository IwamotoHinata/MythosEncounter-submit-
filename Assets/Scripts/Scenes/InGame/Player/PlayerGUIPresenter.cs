using System.Collections;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Scenes.Ingame.Manager;
using Fusion;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// PlayerStatusとDisplayPlayerStatusManagerの橋渡しを行うクラス
    /// MV(R)PにおけるPresenterの役割を想定
    /// </summary>
    public class PlayerGUIPresenter : MonoBehaviour
    {
        //Instance
        public static PlayerGUIPresenter Instance;

        //model
        [SerializeField] private PlayerStatus _playerStatus;
        [SerializeField] private PlayerItem _playerItem;//マルチの時はスクリプト内でinputAuthority持ってるplayerのを代入させる
        [SerializeField] private PlayerInsanityManager _playerInsanityManager;//マルチの時はスクリプト内でinputAuthority持ってるplayerのを代入させる

        [Header("カーソル設定")][SerializeField] private bool _isCurcleSetting = false;

        [Header("ゲーム内UI(オンライン)")]
        [SerializeField] private bool _isOnlineMode;
        [SerializeField] private Slider _healthSlider;//プレイヤーのHPバー
        [SerializeField] private Image _healthFillGauge;//プレイヤーのHPバーのフィルゲージ
        [SerializeField] private Slider _sanValueSlider;//プレイヤーのSAN値バー
        [SerializeField] private Slider _bleedingHealthSlider;//プレイヤーの出血処理用HPバー
        [SerializeField] private TMP_Text _healthText;//プレイヤーのHP残量表示テキスト
        [SerializeField] private TMP_Text _sanValueText;//プレイヤーのSAN値残量表示テキスト
        [SerializeField] private Sprite _itemEmptySprite;//アイテムスロットが空のときにいれる画像

        [Header("ゲーム内UI(オフライン)")]
        [Header("スタミナ関係")]//スタミナゲージ系
        [SerializeField] private RectMask2D _staminaGaugeMask;//個人のスタミナゲージマスク
        [SerializeField] private RectTransform _staminaGaugeFrontRect;//個人のスタミナゲージ
        [SerializeField] private Image _staminaGaugeFrontImage;//個人のスタミナゲージ
        [SerializeField] private Image _staminaGaugeBackGround;

        [SerializeField] private GameObject _pop;//アイテムポップ
        [SerializeField] private TMP_Text _pop_Text;//アイテムポップ

        [Header("アイテム関係")]//アイテム系
        [SerializeField] private Image[] _itemSlots;//アイテムスロット(7個)
        [SerializeField] private Image[] _itemImages;//アイテムの画像(7個)
        [SerializeField] private ItemGetNotificationListView _itemGetNotificationListView;//アイテム取得時のポップアップ
        [SerializeField] private MethodView _methodView;

        [Header("発狂関係")]
        [SerializeField] private Image[] _insanityIcons;//発狂アイコン(5個)
        [SerializeField] private Sprite[] _insanityIconSprites;//発狂アイコンの元画像.EyeParalyze,BodyParalyze,IncreasePulsation,Scream,Hallucination の順番で

        [Header("呪文詠唱関係")]
        [SerializeField] private Canvas _castGauge;
        [SerializeField] private Image _castGaugeImage;
        [SerializeField] private TMP_Text _castTimeText;
        private Sequence castSequence;
        private bool _isCasting = false;//呪文の詠唱 or 脱出の詠唱を行っているか否か

        [Header("マップ関係")]
        [SerializeField] private GameObject _miniMap;
        [SerializeField] private GameObject _noiseFilter;

        [Header("ダメージ関係")]
        [SerializeField] private Image[] _bloodFrame;
        //スタミナゲージ関連のフィールド
        private float _defaultStaminaGaugeWidth;
        private float _lastHp = 100;
        [SerializeField] private TextMeshProUGUI _deadText;

        [Header("ゲーム全体")]
        [SerializeField] private TargetActionView targetActionView;
        [SerializeField] private Light _visionLight;

        private void Init()
        {
            _methodView.Init();
            //プレイヤーのHPやSAN値が変更されたときの処理を追加する。
            _playerStatus.OnPlayerHealthChange
                .Subscribe(x =>
                {
                    //viewに反映
                    ChangeSliderValue(x, "Health");
                }).AddTo(this);

            _playerStatus.OnPlayerBleedingHealthChange
                .Subscribe(x =>
                {
                    //viewに反映
                    ChangeSliderValue(x, "Bleeding");
                }).AddTo(this);

            _playerStatus.OnPlayerSanValueChange
                .Subscribe(x =>
                {
                    //viewに反映
                    ChangeSliderValue(x, "SanValue");
                }).AddTo(this);
            _playerStatus.OnGetDamange
                .Subscribe(_ =>
                {
                    int nowPlayerHealth = _playerStatus.nowPlayerHealth;
                    if (nowPlayerHealth < _lastHp)
                    {
                        DamegeFrame(nowPlayerHealth).Forget();
                    }
                    _lastHp = nowPlayerHealth;
                }).AddTo(this);


            //操作するキャラクターのスタミナゲージにだけ、スタミナゲージを変更させる処理を追加する。
            _playerStatus.OnPlayerStaminaChange
                 .Subscribe(x =>
                 {
                     ChangeStaminaGauge(x);
                     if (x == 100)
                     {
                         _staminaGaugeBackGround.DOFade(endValue: 0f, duration: 1f);
                         _staminaGaugeFrontImage.DOFade(endValue: 0f, duration: 1f);
                     }

                     else
                     {
                         _staminaGaugeBackGround.DOFade(endValue: 1f, duration: 0f);
                         _staminaGaugeFrontImage.DOFade(endValue: 1f, duration: 0f);
                     }

                 }).AddTo(this);

            //呪文の詠唱・脱出地点の詠唱開始時にゲージを表示
            _playerStatus.OnCastEvent
            .Subscribe(time =>
            {
                _castGauge.enabled = true;
                _isCasting = true;
                StartCoroutine(DisPlayRemainCastTime(time));

                //シークエンス初期化
                castSequence = DOTween.Sequence();
                castSequence
                    .Append(_castGaugeImage.DOFillAmount(1, time))
                    .SetDelay(0.5f)
                    .Append(_castGaugeImage.DOFillAmount(0, 0))
                    .OnComplete(() =>
                    {
                        _castGauge.enabled = false;
                        _isCasting = false;
                    });

                castSequence.Play()
                .OnKill(() =>
                {
                    Debug.Log("Sequence is Killed");
                    _castGaugeImage.fillAmount = 0f;
                    _castGauge.enabled = false;
                    _isCasting = false;
                });

            }).AddTo(this);

            _playerStatus.OnCancelCastEvent
            .Subscribe(_ =>
            {
                castSequence.Kill();
            }).AddTo(this);

            //アイテム関係の処理の追加

            //現在選択されているスロットを強調表示
            _playerItem.OnNowIndexChange
                .Skip(1)
                .Subscribe(x =>
                {
                    //全部のスロットの色を元の灰色に戻す
                    for (int i = 0; i < _itemSlots.Length; i++)
                    {
                        if (_playerItem.ItemSlots[i].myItemSlotStatus == ItemSlotStatus.available)
                            _itemSlots[i].color = Color.white;
                    }

                    //選択されているスロットだけ赤色に変化
                    _itemSlots[x].color = Color.red;

                    if (_playerItem.ItemSlots[x].myItemData != null)
                    {
                        _methodView.ShowPanel(_playerItem.ItemSlots[x].myItemData);
                    }
                    else
                    {
                        _methodView.HidePanel();
                    }

                }).AddTo(this);

            _playerItem.OnUseItem.Subscribe(_ => _methodView.HidePanel());

            //目線の先にアイテムかStageIntractがあるとポップを表示させる
            _playerItem.OnPopActive
                .Subscribe(x =>
                {
                    if (x != "")
                    {
                        _pop_Text.text = x;
                        _pop.SetActive(true);
                    }
                    else
                    {
                        _pop_Text.text = null;
                        _pop.SetActive(false);
                    }

                });

            //アイテム取得・破棄時にアイテムスロットの画像を変更させる。
            _playerItem.OnItemSlotReplace
                .Subscribe(replaceEvent =>
                {
                    if (_playerItem.ItemSlots[replaceEvent.Index].myItemData != null)
                    {
                        _methodView.ShowPanel(_playerItem.ItemSlots[replaceEvent.Index].myItemData);
                        _itemImages[replaceEvent.Index].sprite = _playerItem.ItemSlots[replaceEvent.Index].myItemData.itemSprite;
                        _itemGetNotificationListView.PopUp(_playerItem.ItemSlots[replaceEvent.Index].myItemData);
                    }
                    else
                    {
                        _methodView.HidePanel();
                        _itemImages[replaceEvent.Index].sprite = _itemEmptySprite;
                    }

                    //利用不可のスロットの枠を青に変化
                    if (_playerItem.ItemSlots[replaceEvent.Index].myItemSlotStatus == ItemSlotStatus.unavailable)
                        _itemSlots[replaceEvent.Index].color = Color.blue;
                    else
                    {
                        //先に基本色に戻す
                        _itemSlots[replaceEvent.Index].color = Color.white;

                        //もし選択中のアイテムスロットなら赤色に戻す
                        if (replaceEvent.Index == _playerItem.nowIndex)
                            _itemSlots[replaceEvent.Index].color = Color.red;

                    }

                }).AddTo(this);
            _itemGetNotificationListView.Init();
            targetActionView.Init();


            //発狂のスクリプトを管理するListに要素が追加されたときに、アイコンを変化させる。
            _playerInsanityManager.OnInsanitiesAdd
                .Subscribe(addEvent =>
                {
                    _insanityIcons[addEvent.Index].color += new Color(0, 0, 0, 1.0f);
                    switch (_playerInsanityManager.Insanities[addEvent.Index])
                    {
                        case EyeParalyze:
                            _insanityIcons[addEvent.Index].sprite = _insanityIconSprites[0];
                            break;
                        case BodyParalyze:
                            _insanityIcons[addEvent.Index].sprite = _insanityIconSprites[1];
                            break;
                        case IncreasePulsation:
                            _insanityIcons[addEvent.Index].sprite = _insanityIconSprites[2];
                            break;
                        case Scream:
                            _insanityIcons[addEvent.Index].sprite = _insanityIconSprites[3];
                            break;
                        case Hallucination:
                            _insanityIcons[addEvent.Index].sprite = _insanityIconSprites[4];
                            break;
                        default:
                            break;
                    }
                }).AddTo(this);

            //発狂のスクリプトを管理するListの要素が削除されたときに、アイコンを変化させる。
            _playerInsanityManager.OnInsanitiesRemove
                .Subscribe(removeEvent =>
                {
                    _insanityIcons[removeEvent.Index].color -= new Color(0, 0, 0, 1.0f);//透明にする
                    _insanityIcons[removeEvent.Index].sprite = null;
                }).AddTo(this);

            //洗脳状態に応じてアイコンを変化させる。
            _playerInsanityManager.OnPlayerBrainwashedChange
                .Skip(1)//初期化の時は無視
                .Subscribe(x =>
                {
                    if (x)//洗脳状態になったとき
                    {
                        for (int i = 0; i < _insanityIcons.Length; i++)
                            _insanityIcons[i].color -= new Color(0, 0, 0, 1.0f);//透明にする
                    }
                    else//洗脳状態が解除されたとき
                    {
                        for (int i = 0; i < _insanityIcons.Length; i++)
                            _insanityIcons[i].color += new Color(0, 0, 0, 1.0f);//不透明にする
                    }
                }).AddTo(this);

            _deadText.DOFade(0, 0);
            _playerStatus.OnPlayerSurviveChange
                   .Skip(1)
                   .Subscribe(x =>
                   {
                       if (x)//生き返ったとき
                       {
                           _deadText.DOFade(0f, 1).SetDelay(0).SetEase(Ease.InOutSine);
                       }
                       else //死んだとき
                       {
                           _deadText.DOFade(1,1).SetDelay(1.5f).SetEase(Ease.InOutSine);
                       }
                   }).AddTo(this);
            _playerStatus.OnPlayerSurviveChange
                   .Skip(1)
                   .Subscribe(async x =>
                   {
                       if (x)//生き返ったとき
                       {
                           await UniTask.Delay(TimeSpan.FromSeconds(10f));
                           _visionLight.intensity = 2000;
                       }
                       else //死んだとき
                       {
                           _visionLight.intensity = 15000;
                       }
                   }).AddTo(this);
        }

        // Start is called before the first frame update
        void Awake()
        {
            _castGauge.enabled = false;
            _defaultStaminaGaugeWidth = _staminaGaugeFrontRect.sizeDelta.x;

            if (_isCurcleSetting)
                CursorSetting(true);

            //プレイヤーの作成が終わり、配列のソートが終了したら叩かれる
            //オフラインの時
            if (!_isOnlineMode)
            {
                IngameManager.Instance.OnPlayerSpawnEvent
                    .Subscribe(_ =>
                    {
                        //自身が操作するPlayerのPlayerStatusを取得
                        //ここのコメントはFusion対応時に外せ
                        /*
                        PlayerStatus[] playerStatuses = FindObjectsOfType<PlayerStatus>();
                        foreach (PlayerStatus playerStatus in playerStatuses)
                        {
                            if (playerStatus.gameObject.GetComponent<NetworkObject>().HasInputAuthority)
                                _playerStatus = playerStatus;
                            else
                                continue;
                        }
                        */


                        _playerStatus = FindObjectOfType<PlayerStatus>();
                        _playerItem = _playerStatus.gameObject.GetComponent<PlayerItem>();
                        _playerInsanityManager = _playerStatus.gameObject.GetComponent<PlayerInsanityManager>();

                        Init();
                    }).AddTo(this);
            }
            
            

            //インスタンスの設定
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        public void SetInputAuthorityPlayer(NetworkObject player)
        {
            if (player.HasInputAuthority)
            {
                _playerStatus = player.GetComponent<PlayerStatus>();
                _playerItem = player.GetComponent<PlayerItem>();
                _playerInsanityManager = player.GetComponent<PlayerInsanityManager>();

                Init();
            }
            CursorSetting(true);
        }

        /// <summary>
        /// カーソルの設定を行う関数
        /// </summary>
        /// <param name="WannaLock">Lockしたいか否か</param>
        public void CursorSetting(bool WannaLock)
        {
            if (WannaLock)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }           
        }

        private const float HEALTHFADETIME = 0.015f;//HPゲージの明滅アニメーションの時間
        private const float INITIALVALUE = 0.5f;//HPゲージのα値の初期値
        /// <summary>
        /// Sliderの値を変える為の関数.Slider,Textに直接参照している
        /// </summary>
        /// <param name="value">Slinder.Valueに代入する値</param>
        /// <param name="mode">Health(体力), SanValue(SAN値)どちらを変更するのかを決定</param>
        public void ChangeSliderValue(int value, string mode)
        {
            if (mode == "Health")
            {
                _healthFillGauge.DOComplete();
                _healthFillGauge.DOFade(0, HEALTHFADETIME).SetLoops(2,loopType: LoopType.Yoyo).SetEase(Ease.InOutCirc).From(INITIALVALUE);
                _healthSlider.value = value;
            }

            if (mode == "Bleeding")
            {
                _bleedingHealthSlider.value = value;
            }

            else if (mode == "SanValue")
            {
                _sanValueSlider.value = value;
            }
        }
        public void ChangeStaminaGauge(int value)
        {
            //  DoTweenの動作を破棄
            _staminaGaugeFrontImage.DOKill();
            _staminaGaugeBackGround.DOKill();
            
            //スタミナの値を0～1の値に補正
            float fillAmount = (float)value / _playerStatus.stamina_max;
            _staminaGaugeFrontImage.fillAmount = fillAmount;

            //float maskValue = _defaultStaminaGaugeWidth * (1 - fillAmount) / 2;

            // RectMask2Dのleftとrightの値を更新
            //_staminaGaugeMask.padding = new Vector4(maskValue,0, maskValue, 0);

            //スタミナゲージの色変更
            if (0 <= fillAmount && fillAmount <= 0.2)
            {
                _staminaGaugeFrontImage.DOColor(Color.red, 0f);
            }
            else if (0.1 < fillAmount && fillAmount <= 0.5)
            {
                _staminaGaugeFrontImage.DOColor(new Color(1.0f, 0.5f, 0.0f), 0f);
            }
            else
            {
                _staminaGaugeFrontImage.DOColor(Color.white, 0f);
            }
        }

        /// <summary>
        /// 被ダメージ時のダメージ演出
        /// </summary>
        private const float STARTURATION = 0.3f;
        private const float ENDDURATION = 0.3f;
        private const float BLOODTIME = 0.5f;
        private async UniTaskVoid DamegeFrame(int currentHp)
        {
            _bloodFrame[0].DOFade(1, STARTURATION);
            if (currentHp < 50)
            {
                _bloodFrame[1].DOFade(1, STARTURATION);
            }
            if(currentHp < 30)
            {
                _bloodFrame[2].DOFade(1, STARTURATION);
            }
            await Task.Delay(TimeSpan.FromSeconds(BLOODTIME + STARTURATION));
            foreach (var frame in _bloodFrame)
            {
                frame.DOFade(0, ENDDURATION);
            }
        }

        private IEnumerator DisPlayRemainCastTime(float castTime)
        {
            float timer = castTime;
            while (_isCasting) 
            {
                yield return null;
                timer = Mathf.Max(0, timer -= Time.deltaTime);
                _castTimeText.text = "Cast：" + timer.ToString("F1");
            }
            yield break;
        }

        public void MiniMapSetting(bool value)
        {
            _miniMap.SetActive(value);
        }

        public void NoiseFilterSetting(bool value)
        { 
            _noiseFilter.SetActive(value);
        }
    }
}
