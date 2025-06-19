using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using UniRx.Triggers;

namespace Scenes.Ingame.InGameSystem.UI
{
    public class TitleFirstLayerView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _buttonText;
        [SerializeField] private Button _gameStartButton;
        [SerializeField] private Button _optionButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _settingButton;
        [SerializeField] private Button _settingApplyButton;
        [SerializeField] private GameObject _optionWindow;
        [SerializeField] bool _optionWindowState = false;

        public IObservable<Unit> GameStartButtonClick => _gameStartButton.OnClickAsObservable();
        public IObservable<Unit> OptionButtonClick => _optionButton.OnClickAsObservable();
        public IObservable<Unit> QuitButtonClick => _quitButton.OnClickAsObservable();
        public IObservable<Unit> SettingButtonClick => _settingButton.OnClickAsObservable();
        public IObservable<Unit> SettingApplyButtonClick => _settingApplyButton.OnClickAsObservable();

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            _gameStartButton.OnPointerEnterAsObservable().Where(_ => !_optionWindowState).Subscribe(_ => _buttonText.text = "ゲームを開始します。").AddTo(this);
            _gameStartButton.OnPointerExitAsObservable().Where(_ => !_optionWindowState).Subscribe(_ => _buttonText.text = "").AddTo(this);

            _optionButton.OnPointerEnterAsObservable().Where(_ => !_optionWindowState).Subscribe(_ => _buttonText.text = "設定を変更・確認します。").AddTo(this);
            _optionButton.OnPointerExitAsObservable().Where(_ => !_optionWindowState).Subscribe(_ => _buttonText.text = "").AddTo(this);

            _quitButton.OnPointerEnterAsObservable().Where(_ => !_optionWindowState).Subscribe(_ => _buttonText.text = "ゲームを終了しウィンドウを閉じます。").AddTo(this);
            _quitButton.OnPointerExitAsObservable().Where(_ => !_optionWindowState).Subscribe(_ => _buttonText.text = "").AddTo(this);
        }

        public void SetOptionWindow(bool value)
        {
            _optionWindowState = value;
            if (_optionWindowState)
            {
                _optionWindow.SetActive(true);
                _buttonText.text = "";
            }
            else{
                _optionWindow.SetActive(false);
            }
        }

        public void FirstLayerChange(bool value)
        {
            if (!_optionWindowState)
            {
                _buttonText.text = "";
                _gameStartButton.gameObject.SetActive(value);
                _optionButton.gameObject.SetActive(value);
                _quitButton.gameObject.SetActive(value);
            }
        }
    }
}