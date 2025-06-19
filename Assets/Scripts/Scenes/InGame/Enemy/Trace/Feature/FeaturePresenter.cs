using UnityEngine;
using UniRx;

namespace Scenes.Ingame.Enemy.Trace.Feature
{
    public class FeaturePresenter : MonoBehaviour
    {
        FeatureView _view;
        FeatureModel _model;

        public void Init(TraceType[] features)
        {
            _view = GetComponent<FeatureView>();
            _view.Init();
            _model = new FeatureModel();
            _model.Init(features, _view);
            _view.onDestroy
                .Subscribe(_ => _model.FaturesCancel()).AddTo(this);
        }
    }
}