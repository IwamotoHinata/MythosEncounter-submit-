using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Scenes.Ingame.Manager;
using Fusion;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// PlayerStatus��DisplayPlayerStatusManager�̋��n�����s���N���X
    /// MV(R)P�ɂ�����Presenter�̖�����z��
    /// </summary>
    public class PlayerUITest : MonoBehaviour
    {
        //Instance
        public static PlayerUITest Instance;

        //model
        [SerializeField] private PlayerStatus _playerStatus;
        [SerializeField] private PlayerItem _playerItem;//�}���`�̎��̓X�N���v�g����inputAuthority�����Ă�player�̂���������
        [SerializeField] private PlayerInsanityManager _playerInsanityManager;//�}���`�̎��̓X�N���v�g����inputAuthority�����Ă�player�̂���������

        [Header("�J�[�\���ݒ�")][SerializeField] private bool _isCurcleSetting = false;

        [Header("�Q�[����UI(�I�����C��)")]
        [SerializeField] private Slider _healthSlider;//�v���C���[��HP�o�[
        [SerializeField] private Slider _sanValueSlider;//�v���C���[��SAN�l�o�[
        [SerializeField] private Slider _bleedingHealthSlider;//�v���C���[�̏o�������pHP�o�[
        [SerializeField] private TMP_Text _healthText;//�v���C���[��HP�c�ʕ\���e�L�X�g
        [SerializeField] private TMP_Text _sanValueText;//�v���C���[��SAN�l�c�ʕ\���e�L�X�g
        [SerializeField] private Sprite _itemEmptySprite;//�A�C�e���X���b�g����̂Ƃ��ɂ����摜

        [Header("�Q�[����UI(�I�t���C��)")]
        [Header("�X�^�~�i�֌W")]//�X�^�~�i�Q�[�W�n
        [SerializeField] private RectMask2D _staminaGaugeMask;//�l�̃X�^�~�i�Q�[�W�}�X�N
        [SerializeField] private RectTransform _staminaGaugeFrontRect;//�l�̃X�^�~�i�Q�[�W
        [SerializeField] private Image _staminaGaugeFrontImage;//�l�̃X�^�~�i�Q�[�W
        [SerializeField] private Image _staminaGaugeBackGround;

        [SerializeField] private GameObject _pop;//�A�C�e���|�b�v
        [SerializeField] private TMP_Text _pop_Text;//�A�C�e���|�b�v

        [Header("�A�C�e���֌W")]//�A�C�e���n
        [SerializeField] private Image[] _itemSlots;//�A�C�e���X���b�g(7��)
        [SerializeField] private Image[] _itemImages;//�A�C�e���̉摜(7��)

        [Header("�����֌W")]
        [SerializeField] private Image[] _insanityIcons;//�����A�C�R��(5��)
        [SerializeField] private Sprite[] _insanityIconSprites;//�����A�C�R���̌��摜.EyeParalyze,BodyParalyze,IncreasePulsation,Scream,Hallucination �̏��Ԃ�

        [Header("�����r���֌W")]
        [SerializeField] private Canvas _castGauge;
        [SerializeField] private Image _castGaugeImage;
        [SerializeField] private TMP_Text _castTimeText;
        private Sequence castSequence;
        private bool _isCasting = false;//�����̉r�� or �E�o�̉r�����s���Ă��邩�ۂ�

        [Header("�}�b�v�֌W")]
        [SerializeField] private GameObject _miniMap;
        [SerializeField] private GameObject _noiseFilter;

        //�X�^�~�i�Q�[�W�֘A�̃t�B�[���h
        private float _defaultStaminaGaugeWidth;

        // Start is called before the first frame update
        void Awake()
        {
            _castGauge.enabled = false;
            _defaultStaminaGaugeWidth = _staminaGaugeFrontRect.sizeDelta.x;

            if (_isCurcleSetting)
                CursorSetting(true);
            _playerStatus = FindObjectOfType<PlayerStatus>();

            //�v���C���[��HP��SAN�l���ύX���ꂽ�Ƃ��̏�����ǉ�����B
            _playerStatus.OnPlayerHealthChange
                .Subscribe(x =>
                {
                    //view�ɔ��f
                    ChangeSliderValue(x, "Health");
                }).AddTo(this);

            _playerStatus.OnPlayerBleedingHealthChange
                .Subscribe(x =>
                {
                    //view�ɔ��f
                    ChangeSliderValue(x, "Bleeding");
                }).AddTo(this);

            _playerStatus.OnPlayerSanValueChange
                .Subscribe(x =>
                {
                    //view�ɔ��f
                    ChangeSliderValue(x, "SanValue");
                }).AddTo(this);


            //���삷��L�����N�^�[�̃X�^�~�i�Q�[�W�ɂ����A�X�^�~�i�Q�[�W��ύX�����鏈����ǉ�����B
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

            //�����̉r���E�E�o�n�_�̉r���J�n���ɃQ�[�W��\��
            _playerStatus.OnCastEvent
            .Subscribe(time =>
            {
                _castGauge.enabled = true;
                _isCasting = true;
                StartCoroutine(DisPlayRemainCastTime(time));

                //�V�[�N�G���X������
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

            //�A�C�e���֌W�̏����̒ǉ�
            //PlayerItem�X�N���v�g�̎擾.�}���`�����̂Ƃ���inputAuthority�����L�����N�^�[�݂̂Ɏw��
            _playerItem = _playerStatus.gameObject.GetComponent<PlayerItem>();

            //���ݑI������Ă���X���b�g�������\��
            _playerItem.OnNowIndexChange
                .Skip(1)
                .Subscribe(x =>
                {
                    //�S���̃X���b�g�̐F�����̊D�F�ɖ߂�
                    for (int i = 0; i < _itemSlots.Length; i++)
                    {
                        if (_playerItem.ItemSlots[i].myItemSlotStatus == ItemSlotStatus.available)
                            _itemSlots[i].color = Color.white;
                    }

                    //�I������Ă���X���b�g�����ԐF�ɕω�
                    _itemSlots[x].color = Color.red;
                }).AddTo(this);

            //�ڐ��̐�ɃA�C�e����StageIntract������ƃ|�b�v��\��������
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

            //�A�C�e���擾�E�j�����ɃA�C�e���X���b�g�̉摜��ύX������B
            _playerItem.OnItemSlotReplace
                .Subscribe(replaceEvent =>
                {
                    if (_playerItem.ItemSlots[replaceEvent.Index].myItemData != null)
                    {
                        _itemImages[replaceEvent.Index].sprite = _playerItem.ItemSlots[replaceEvent.Index].myItemData.itemSprite;
                    }
                    else
                    {
                        _itemImages[replaceEvent.Index].sprite = _itemEmptySprite;
                    }

                    //���p�s�̃X���b�g�̘g��ɕω�
                    if (_playerItem.ItemSlots[replaceEvent.Index].myItemSlotStatus == ItemSlotStatus.unavailable)
                        _itemSlots[replaceEvent.Index].color = Color.blue;
                    else
                    {
                        //��Ɋ�{�F�ɖ߂�
                        _itemSlots[replaceEvent.Index].color = Color.white;

                        //�����I�𒆂̃A�C�e���X���b�g�Ȃ�ԐF�ɖ߂�
                        if (replaceEvent.Index == _playerItem.nowIndex)
                            _itemSlots[replaceEvent.Index].color = Color.red;

                    }

                }).AddTo(this);

            //PlayerInsanityManager�X�N���v�g�̎擾.�}���`�����̂Ƃ���inputAuthority�����L�����N�^�[�݂̂Ɏw��
            _playerInsanityManager = _playerStatus.gameObject.GetComponent<PlayerInsanityManager>();

            //�����̃X�N���v�g���Ǘ�����List�ɗv�f���ǉ����ꂽ�Ƃ��ɁA�A�C�R����ω�������B
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

            //�����̃X�N���v�g���Ǘ�����List�̗v�f���폜���ꂽ�Ƃ��ɁA�A�C�R����ω�������B
            _playerInsanityManager.OnInsanitiesRemove
                .Subscribe(removeEvent =>
                {
                    _insanityIcons[removeEvent.Index].color -= new Color(0, 0, 0, 1.0f);//�����ɂ���
                    _insanityIcons[removeEvent.Index].sprite = null;
                }).AddTo(this);

            //���]��Ԃɉ����ăA�C�R����ω�������B
            _playerInsanityManager.OnPlayerBrainwashedChange
                .Skip(1)//�������̎��͖���
                .Subscribe(x =>
                {
                    if (x)//���]��ԂɂȂ����Ƃ�
                    {
                        for (int i = 0; i < _insanityIcons.Length; i++)
                            _insanityIcons[i].color -= new Color(0, 0, 0, 1.0f);//�����ɂ���
                    }
                    else//���]��Ԃ��������ꂽ�Ƃ�
                    {
                        for (int i = 0; i < _insanityIcons.Length; i++)
                            _insanityIcons[i].color += new Color(0, 0, 0, 1.0f);//�s�����ɂ���
                    }
                }).AddTo(this);

            //�C���X�^���X�̐ݒ�
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        /// <summary>
        /// �J�[�\���̐ݒ���s���֐�
        /// </summary>
        /// <param name="WannaLock">Lock���������ۂ�</param>
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

        /// <summary>
        /// Slider�̒l��ς���ׂ̊֐�.Slider,Text�ɒ��ڎQ�Ƃ��Ă���
        /// </summary>
        /// <param name="value">Slinder.Value�ɑ������l</param>
        /// <param name="mode">Health(�̗�), SanValue(SAN�l)�ǂ����ύX����̂�������</param>
        public void ChangeSliderValue(int value, string mode)
        {
            if (mode == "Health")
            {
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
            //  DoTween�̓����j��
            _staminaGaugeFrontImage.DOKill();
            _staminaGaugeBackGround.DOKill();

            //�X�^�~�i�̒l��0�`1�̒l�ɕ␳
            float fillAmount = (float)value / _playerStatus.stamina_max;
            _staminaGaugeFrontImage.fillAmount = fillAmount;

            //float maskValue = _defaultStaminaGaugeWidth * (1 - fillAmount) / 2;

            // RectMask2D��left��right�̒l���X�V
            //_staminaGaugeMask.padding = new Vector4(maskValue,0, maskValue, 0);

            //�X�^�~�i�Q�[�W�̐F�ύX
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

        private IEnumerator DisPlayRemainCastTime(float castTime)
        {
            float timer = castTime;
            while (_isCasting)
            {
                yield return null;
                timer = Mathf.Max(0, timer -= Time.deltaTime);
                _castTimeText.text = "Cast�F" + timer.ToString("F1");
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

