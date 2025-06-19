using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;
using Scenes.Ingame.Enemy.Trace;
using Scenes.Ingame.Manager;

namespace Scenes.Ingame.Journal
{
    public class CompatibilityView : ViewBase
    {
        [SerializeField] private GridView _girdView;
        private TraceFeatureController _traceFeatureController;

        public override void Init()
        {
            _traceFeatureController = FindObjectOfType<TraceFeatureController>();
            if (_traceFeatureController == null) 
            {
                Debug.LogWarning("_traceFeatureControllerƒkƒ‹‚ª”­¶");
                return; 
            }
            WebDataRequest.instance.OnEndLoad.Subscribe(_ =>
            {
                _girdView.Init(WebDataRequest.GetEnemyDataArrayList);
            }).AddTo(this);

            IngameManager.Instance.OnGetJournal.Subscribe(_ => _girdView.UpdateJournalList(_traceFeatureController.traceModel.usedCombinations)).AddTo(this);
        }
    }
}