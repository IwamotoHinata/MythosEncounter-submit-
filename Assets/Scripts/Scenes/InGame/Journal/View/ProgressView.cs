using Scenes.Ingame.Manager;
using UnityEngine;
using UniRx;
using TMPro;

namespace Scenes.Ingame.Journal
{
    public class ProgressView : ViewBase
    {
        [SerializeField] private TextMeshProUGUI _leftText;
        private bool _getJournalPiece = false;
        private int _currentEscapeItem = 0;
        private int _maxEscapeItem = 0;

        public override void Init()
        {
            if(IngameManager.Instance == null) return;
            IngameManager.Instance.OnGetJournal.Subscribe(_ =>
            {
                _getJournalPiece = true;
                UpdateText();
            }).AddTo(this);
            _maxEscapeItem = IngameManager.Instance.GetEscapeItemMaxCount;
            IngameManager.Instance.OnEscapeCount.Subscribe(currentCount =>
            {
                _currentEscapeItem = currentCount;
                UpdateText();
            }).AddTo(this);
            UpdateText();
        }

        private void UpdateText()
        {
            string journalPiece = _getJournalPiece ? "■" : "□";
            _leftText.text = $"<size=36>進捗</size>\n\n{journalPiece}【痕跡対応表】を取得する\n\n□脱出アイテムを集める（{_currentEscapeItem}/{_maxEscapeItem}）\n\n□脱出する\n";
        }
    }
}