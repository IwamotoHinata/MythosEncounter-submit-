using TMPro;
using UnityEngine;
using UniRx;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System.Linq;

namespace Scenes.Ingame.Journal
{
    public class ItemView : ViewBase
    {
        [SerializeField] private TextMeshProUGUI _rightPage;
        [SerializeField] private Transform _content;
        [SerializeField] private NameButtonView _itemButton;
        [SerializeField] private Image _itemImage;
        [SerializeField] private ItemData[] _itemData;

        public override void Init()
        {
            _itemImage.color = Color.clear;
            WebDataRequest.instance.OnEndLoad.Subscribe(_ =>
            {
                var itemList = WebDataRequest.GetItemDataArrayList;
                foreach (var item in itemList)
                {
                    var itemButton = Instantiate(_itemButton, _content);
                    itemButton.NameSet(item.Name);
                    itemButton.button.OnClickAsObservable().Subscribe(_ => _rightPage.text = text(item)).AddTo(this);
                    itemButton.button.OnClickAsObservable().Subscribe(_ => SetItemImage(item)).AddTo(this);
                }
            }).AddTo(this);
        }

        public string text(ItemDataStruct detail)
        {
            return $"<size=20>{detail.Name}</size>\n\n<size=18>ê‡ñæ</size>\n{Regex.Unescape(detail.Description)}";
        }

        public void SetItemImage(ItemDataStruct detail)
        {
            try
            {
                var data = _itemData.FirstOrDefault(d => d.itemName == detail.Name).itemSprite;
                _itemImage.color = Color.white;
                _itemImage.sprite = data;
            }
            catch (System.Exception)
            {
                _itemImage.color = Color.clear;
            }
        }
    }
}