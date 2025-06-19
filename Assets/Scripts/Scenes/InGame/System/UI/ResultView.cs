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
        [SerializeField, Tooltip("���U���g�p�̃L�����o�X")]
        private GameObject _resultCanvas;
        [SerializeField, Tooltip("�E�o�����̃e�L�X�g")]
        private TextMeshProUGUI _successEscape;
        [SerializeField, Tooltip("�E�o���Ԃ̃e�L�X�g")]
        private TextMeshProUGUI _escapeTime;
        [SerializeField, Tooltip("��Փx�̃e�L�X�g")]
        private TextMeshProUGUI _diffculty;
        [SerializeField, Tooltip("���j�[�N�A�C�e���擾�̃e�L�X�g")]
        private TextMeshProUGUI _getUniqueItem;
        [SerializeField, Tooltip("�������̃e�L�X�g")]
        private TextMeshProUGUI _firstContact;
        [SerializeField, Tooltip("���v���z�̃e�L�X�g")]
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
                 _escapeTime.text = $"�E�o���ԃ{�[�i�X�@�@�@�@�@{timeBouns}$";
                 _diffculty.text = $"��Փx�{�[�i�X�@�@�@�@ �@�@{resultValue.level * 20}$";
                 _getUniqueItem.text = $"���j�[�N�A�C�e���̎擾�@{(resultValue.getUnique ? "50$" : "0$")}";
                 _firstContact.text = $"�������@�@�@�@�@�@�@�@�@{(resultValue.firstContact ? "100$" : "0$")}";
                 _totalMoney.text = $"���v���z�@�@�@�@�@�@�@�@�@{resultValue.totalMoney}$";
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