using TMPro;
using UnityEngine;
using UniRx;
using System.Text.RegularExpressions;

namespace Scenes.Ingame.Journal
{
    public class SpellView : ViewBase
    {
        [SerializeField] private TextMeshProUGUI _rightPage;
        [SerializeField] private Transform _content;
        [SerializeField] private NameButtonView _spellButton;

        public override void Init()
        {
            WebDataRequest.instance.OnEndLoad.Subscribe(_ =>
            {
                var spellList = WebDataRequest.GetSpellDataArrayList;
                foreach (var spell in spellList)
                {
                    var spellButton = Instantiate(_spellButton, _content);
                    spellButton.NameSet(spell.Name);
                    spellButton.button.OnClickAsObservable().Subscribe(_ => _rightPage.text = text(spell)).AddTo(this);
                }
            }).AddTo(this);
        }

        public string text(SpellStruct detail)
        {
            return $"<size=22>{detail.Name}</size>\n\n<size=18>à–¾</size>\n{Regex.Unescape(detail.Explanation)}\n\n<size=18>‰ğ•úğŒ</size>\n{Regex.Unescape(detail.unlockExplanation)}";
        }
    }
}