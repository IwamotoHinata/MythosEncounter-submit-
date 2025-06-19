using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Scenes.Ingame.Enemy.Trace.Feature
{
    public class BreathFeature : FeatureBase
    {
        CancellationTokenSource _cancellationTokenSource;
        FeatureView _view;
        private const int MINTIME = 15;
        private const int MAXTIME = 20;

        public override void Init(FeatureView view)
        {
            _view = view;
            _cancellationTokenSource = new CancellationTokenSource();
            BreathLoop(_cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid BreathLoop(CancellationToken token)
        {
            while (true)
            {
                var interval = UnityEngine.Random.Range(MINTIME, MAXTIME);
                await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: token);
                _view.Breath();
            }
        }

        public override void Cancel()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}