using UnityEngine;
using UniRx;

namespace Scenes.Ingame.InGameSystem.UI
{
    public class TitlePresenter : MonoBehaviour
    {

        [SerializeField] TitleModel _titleModel;
        [SerializeField] TitleFirstLayerView _titleFirstLayerView;
        [SerializeField] TitleSecondLayerView _titleSecondLayerView;
        [SerializeField] TitleThirdLayerView _titleThirdLayerView;
        [SerializeField] TitleForthLayerView _titleForthLayerView;
        [SerializeField] bool _optionWindowState = false;
        [SerializeField] int _layerNumber = 1;

        void Start()
        {
            //model‚©‚çview
            _titleModel.OptionWindowStatus.Subscribe(x =>
            {
                _optionWindowState = x;
                _titleFirstLayerView.SetOptionWindow(x);
            }).AddTo(this);

            _titleModel.LayerNumber.Subscribe(x =>
            {
                switch (x)
                {
                    case 1:
                        _layerNumber = x;
                        _titleFirstLayerView.FirstLayerChange(true);
                        break;
                    case 2:
                        _layerNumber = x;
                        _titleFirstLayerView.FirstLayerChange(false);
                        _titleSecondLayerView.SecondLayerChange(true);
                        break;
                    case 3:
                        _layerNumber = x;
                        _titleSecondLayerView.SecondLayerChange(false);
                        _titleThirdLayerView.ThirdLayerChange(true);
                        break;
                    case 4:
                        _layerNumber = x;
                        _titleThirdLayerView.ThirdLayerChange(false);
                        _titleForthLayerView.ForthLayerChange(true);
                        break;
                    case 5:
                        _layerNumber = x;
                        _titleForthLayerView.LobbyChange();
                        break;
                }
            }).AddTo(this);

            //view‚©‚çmodel
            _titleFirstLayerView.GameStartButtonClick.Where(_ => !_optionWindowState && _layerNumber == 1).Subscribe(_ =>
            {
                _titleModel.SetLayerNumber(2);
            }).AddTo(this);
            _titleFirstLayerView.OptionButtonClick.Where(_ => !_optionWindowState && _layerNumber == 1).Subscribe(_ =>
            {
                _titleModel.SetOptionWindow(true);
            }).AddTo(this);
            _titleFirstLayerView.QuitButtonClick.Where(_ => !_optionWindowState && _layerNumber == 1).Subscribe(_ =>
            {
                _titleModel.GameQuit();
            }).AddTo(this);
            _titleFirstLayerView.SettingButtonClick.Where(_ => _layerNumber == 1).Subscribe(_ =>
            {
                _titleModel.SetOptionWindow(false);
            }).AddTo(this);
            _titleFirstLayerView.SettingApplyButtonClick.Where(_ => _layerNumber == 1).Subscribe(_ =>
            {
                _titleModel.SetOptionWindow(false);
            }).AddTo(this);

            _titleSecondLayerView.ContinueButtonClick.Where(_ => _layerNumber == 2).Subscribe(_ =>
            {

            }).AddTo(this);
            _titleSecondLayerView.NewGameButtonClick.Where(_ => _layerNumber == 2).Subscribe(_ =>
            {
                _titleModel.SetLayerNumber(3);
            }).AddTo(this);

            _titleThirdLayerView.Type1ButtonClick.Where(_ => _layerNumber == 3).Subscribe(_ =>
            {
                _titleModel.SetTypeNumber(1);
                _titleModel.SetLayerNumber(4);
            }).AddTo(this);
            _titleThirdLayerView.Type2ButtonClick.Where(_ => _layerNumber == 3).Subscribe(_ =>
            {
                _titleModel.SetTypeNumber(2);
                _titleModel.SetLayerNumber(4);
            }).AddTo(this);

            _titleForthLayerView.NameApplyButtonClick.Where(_ => _layerNumber == 4).Subscribe(_ =>
            {
                _titleModel.SetLayerNumber(5);
            }).AddTo(this);
        }
    }
}