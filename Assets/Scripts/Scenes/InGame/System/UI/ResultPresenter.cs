using UnityEngine;
using UniRx;
using Scenes.Ingame.Manager;
using DG.Tweening;

namespace Scenes.Ingame.InGameSystem.UI
{
    public class ResultPresenter : MonoBehaviour
    {
        private Sequence _sequence;
        private ResultView _view;
        [SerializeField, Tooltip("リザルト用のキャンバス")]
        private GameObject _resultCanvas;
        void Start()
        {
            _view = GetComponent<ResultView>();
            _resultCanvas.SetActive(false);
            IngameManager.Instance.OnResult
                .Subscribe(_ =>
                {
                    _resultCanvas.SetActive(true);
                }).AddTo(this);
            ResultManager.Instance.OnResultValue
                .Subscribe(value =>
                {
                    _view.Display(value);
                }).AddTo(this);
        }
    }
}