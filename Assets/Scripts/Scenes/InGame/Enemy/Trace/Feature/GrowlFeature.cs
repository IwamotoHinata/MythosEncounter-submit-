using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Scenes.Ingame.Enemy.Trace.Feature
{
    public class GrowlFeature : FeatureBase
    {
        CancellationTokenSource _cancellationTokenSource;
        FeatureView _view;
        private const int MINTIME = 40;
        private const int MAXTIME = 60;

        public override void Init(FeatureView view)
        {
            _view = view;
            _cancellationTokenSource = new CancellationTokenSource();
            GrowlLoop(_cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid GrowlLoop(CancellationToken token)
        {
            while (true)
            {
                var interval = UnityEngine.Random.Range(MINTIME, MAXTIME);
                await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: token);
                _view.Grow();
            }
        }

        public override void Cancel()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}