using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Scenes.Ingame.Enemy.Trace.Feature
{
    public class TemperatureFeature : FeatureBase
    {
        CancellationTokenSource _cancellationTokenSource;
        FeatureView _view;
        private float _change;
      
        public override void Init(FeatureView view)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _view = view;
            _view.OnStageTileChange.Skip(1).Subscribe(tile =>
            {
               _change = tile.Temperature - 10;
                if (_change < tile.Keep - 10)
                    _change = tile.Keep - 10;
                _view.Temperature(_change);
            }).AddTo(view.gameObject); ;
            
        }


    public override void Cancel()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}