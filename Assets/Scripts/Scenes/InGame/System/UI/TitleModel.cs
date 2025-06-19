using UnityEngine;
using UniRx;

namespace Scenes.Ingame.InGameSystem.UI
{

    public class TitleModel : MonoBehaviour
    {
        [SerializeField] private ReactiveProperty<bool> _optionWindowStatus = new ReactiveProperty<bool>(false);
        public IReadOnlyReactiveProperty<bool> OptionWindowStatus => _optionWindowStatus;
        [SerializeField] private ReactiveProperty<int> _layerNumber = new ReactiveProperty<int>(1);
        public IReadOnlyReactiveProperty<int> LayerNumber => _layerNumber;
        [SerializeField] private ReactiveProperty<int> _typeNumber = new ReactiveProperty<int>(0);
        public IReadOnlyReactiveProperty<int> TypeNumber => _typeNumber;

        public void SetOptionWindow(bool value)
        {
            _optionWindowStatus.Value = value;
        }

        public void SetLayerNumber(int value)
        {
            _layerNumber.Value = value;
        }

        public void SetTypeNumber(int value)
        {
            _typeNumber.Value = value;
        }

        public void GameQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
            Application.Quit();//ゲームプレイ終了
#endif
        }
    }
}