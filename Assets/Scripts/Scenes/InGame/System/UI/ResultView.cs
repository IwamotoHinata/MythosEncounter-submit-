using Common.UI;
using DG.Tweening;
using System;
using UniRx;
using UniRx.Triggers;
using TMPro;
using UnityEngine;
namespace Scenes.Ingame.InGameSystem.UI
{
    public class ResultView : MonoBehaviour
    {
        private Sequence _sequence;
        [SerializeField, Tooltip("リザルト用のキャンバス")]
        private GameObject _resultCanvas;
        [SerializeField, Tooltip("脱出成功のテキスト")]
        private TextMeshProUGUI _successEscape;
        [SerializeField, Tooltip("脱出時間のテキスト")]
        private TextMeshProUGUI _escapeTime;
        [SerializeField, Tooltip("難易度のテキスト")]
        private TextMeshProUGUI _diffculty;
        [SerializeField, Tooltip("ユニークアイテム取得のテキスト")]
        private TextMeshProUGUI _getUniqueItem;
        [SerializeField, Tooltip("初遭遇のテキスト")]
        private TextMeshProUGUI _firstContact;
        [SerializeField, Tooltip("合計金額のテキスト")]
        private TextMeshProUGUI _totalMoney;
        [SerializeField]
        private SceneTransition _sceneTransition;

        public void Display(ResultValue resultValue)
        {
            _sequence = DOTween.Sequence();
            _sequence
             .AppendCallback(() =>
             {
                 _successEscape.color = new Color(0, 0, 0, 0);
                 _escapeTime.color = new Color(0, 0, 0, 0);
                 _diffculty.color = new Color(0, 0, 0, 0);
                 _getUniqueItem.color = new Color(0, 0, 0, 0);
                 _firstContact.color = new Color(0, 0, 0, 0);
                 _totalMoney.color = new Color(0, 0, 0, 0);
                 string min = (resultValue.time / 60).ToString("D2");
                 string sec = (resultValue.time % 60).ToString("D2");
                 int timeBouns = (20 - resultValue.time / 60) * 5 > 0 ? (20 - resultValue.time / 60) * 5 : 0;
                 _escapeTime.text = $"脱出時間ボーナス　　　　　{timeBouns}$";
                 _diffculty.text = $"難易度ボーナス　　　　 　　{resultValue.level * 20}$";
                 _getUniqueItem.text = $"ユニークアイテムの取得　{(resultValue.getUnique ? "50$" : "0$")}";
                 _firstContact.text = $"初遭遇　　　　　　　　　{(resultValue.firstContact ? "100$" : "0$")}";
                 _totalMoney.text = $"合計金額　　　　　　　　　{resultValue.totalMoney}$";
             })
            .Append(_successEscape.DOFade(1, 0.5f))
            .AppendInterval(0.2f)
            .Append(_escapeTime.DOFade(1, 0.5f))
            .AppendInterval(0.2f)
            .Append(_diffculty.DOFade(1, 0.5f))
            .AppendInterval(0.2f)
            .Append(_getUniqueItem.DOFade(1, 0.5f))
            .AppendInterval(0.2f)
            .Append(_firstContact.DOFade(1, 0.5f))
            .AppendInterval(0.2f)
            .Append(_totalMoney.DOFade(1, 0.5f))
            .AppendInterval(0.2f)
            .AppendCallback(() =>
            {
                BackLobby();
            });
        }

        private void BackLobby()
        {
            this.UpdateAsObservable().Delay(TimeSpan.FromSeconds(1)).Where(_ => Input.anyKey).Subscribe(_ => _sceneTransition.Action()).AddTo(this);
        }
    }
}