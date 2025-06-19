using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using UniRx.Triggers;

namespace Scenes.Ingame.InGameSystem.UI
{
    public class TitleSecondLayerView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _buttonText;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] int _layerNumber = 1;

        public IObservable<Unit> ContinueButtonClick => _continueButton.OnClickAsObservable();
        public IObservable<Unit> NewGameButtonClick => _newGameButton.OnClickAsObservable();

        void Start()
        {
            _continueButton.OnPointerEnterAsObservable().Where(_ => _layerNumber != 3).Subscribe(_ => _buttonText.text = "�O��̃L�����N�^�[�ōĊJ���܂��B").AddTo(this);
            _continueButton.OnPointerExitAsObservable().Where(_ => _layerNumber != 3).Subscribe(_ => _buttonText.text = "").AddTo(this);
            _newGameButton.OnPointerEnterAsObservable().Where(_ => _layerNumber != 3).Subscribe(_ => _buttonText.text = "�L�����N�^�[��V�K�쐬���܂��B").AddTo(this);
            _newGameButton.OnPointerExitAsObservable().Where(_ => _layerNumber != 3).Subscribe(_ => _buttonText.text = "").AddTo(this);
        }

        public void SecondLayerChange(bool value)
        {
            if (value == true)
            {
                _buttonText.text = "";
                _continueButton.gameObject.SetActive(true);
                _newGameButton.gameObject.SetActive(true);
            }
            else if (value == false)
            {
                _buttonText.text = "";
                _layerNumber = 3;
            }
        }

    }
}