using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace Scenes.Ingame.InGameSystem.UI
{
    public class TitleThirdLayerView : MonoBehaviour
    {
        [SerializeField] private Button _type1Button;
        [SerializeField] private Button _type2Button;
        [SerializeField] private GameObject _typeWindow;

        public IObservable<Unit> Type1ButtonClick => _type1Button.OnClickAsObservable();
        public IObservable<Unit> Type2ButtonClick => _type2Button.OnClickAsObservable();

        public void ThirdLayerChange(bool value)
        {
            _typeWindow.SetActive(value);
        }
    }
}