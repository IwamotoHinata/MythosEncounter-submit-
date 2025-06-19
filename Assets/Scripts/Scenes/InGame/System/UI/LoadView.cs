using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Scenes.Ingame.Manager;
using UniRx;

namespace Scenes.Ingame.InGameSystem.UI
{
    public class LoadView : MonoBehaviour
    {
        Canvas _laodCanvas;
        [SerializeField]
        Slider _progressSlider;
        Canvas _progressSlderCanvas;
        [SerializeField]
        Image _backgroundImage;
        private float _progress = 0;
        private const int MAXPROGRESS = 4;//êiíªÇÃêîÅAOnInitial,OnStageGenerateEvent,OnPlayerSpawnEvent,OnIngame
        private const float FADESPEED = 0.5f;//îwåiÇ™ìßâﬂÇ∑ÇÈÇ‹Ç≈ÇÃéûä‘
        void Awake()
        {
            _laodCanvas = GetComponent<Canvas>();
            _progressSlderCanvas = _progressSlider.GetComponent<Canvas>();
        }
        private void Start()
        {
            IngameManager.Instance.OnInitial
                .First()
                .Subscribe(_ =>
                {
                    UpdateSlider();
                }).AddTo(this);
            IngameManager.Instance.OnStageGenerateEvent
                .First()
                .Subscribe(_ =>
                {
                    UpdateSlider();
                    //TODO:âºíuÇ´
                    _progressSlderCanvas.enabled = false;
                    _backgroundImage.DOFade(0, FADESPEED)
                    .OnComplete(CanvasFade);
                }).AddTo(this);
            IngameManager.Instance.OnPlayerSpawnEvent
                .First()
                .Subscribe(_ =>
                {
                    UpdateSlider();
                }).AddTo(this);
            IngameManager.Instance.OnIngame
                .First()
                .Subscribe(_ =>
                {
                    UpdateSlider();
                    /*
                    _progressSlderCanvas.enabled = false;
                    _backgroundImage.DOFade(0, FADESPEED)
                    .OnComplete(CanvasFade);
                    */

                }).AddTo(this);
        }
        private void UpdateSlider()
        {
            _progress++;
            _progressSlider.value = _progress / MAXPROGRESS;
        }
        private void CanvasFade()
        {
            _laodCanvas.enabled = false;
        }
    }
}