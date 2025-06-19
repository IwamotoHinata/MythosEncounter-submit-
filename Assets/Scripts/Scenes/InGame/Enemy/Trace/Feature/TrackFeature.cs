using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace Scenes.Ingame.Enemy.Trace.Feature
{
    public class TrackFeature : FeatureBase
    {
        CancellationTokenSource _cancellationTokenSource;
        FeatureView _view;

        public override void Init(FeatureView view)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _view = view;
            Track(_cancellationTokenSource.Token).Forget();
        }
        private async UniTaskVoid Track(CancellationToken token)
        {
            while (true)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(30), cancellationToken: token);
                _view.InstanceTrackSprite();
            }
        }

        public override void Cancel()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}