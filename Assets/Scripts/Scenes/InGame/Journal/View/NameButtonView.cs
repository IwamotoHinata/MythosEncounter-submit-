using UnityEngine;
using TMPro;
using UnityEngine.UI;
namespace Scenes.Ingame.Journal
{
    public class NameButtonView : ViewBase
    {
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private Button _button;
        public Button button { get { return _button; } }

        public void NameSet(string name)
        {
            _name.text = name;
        }
    }
}