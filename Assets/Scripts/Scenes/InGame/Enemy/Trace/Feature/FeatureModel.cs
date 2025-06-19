namespace Scenes.Ingame.Enemy.Trace.Feature
{
    public class FeatureModel
    {
        FeatureBase[] _features = new FeatureBase[3];
        public void Init(TraceType[] features, FeatureView view)
        {
            for (int i = 0; i < features.Length; i++)
            {
                switch (features[i])
                {
                    case TraceType.Interact:
                        _features[i] = new InteractFeature();
                        break;
                    case TraceType.Breath:
                        _features[i] = new BreathFeature();
                        break;
                    case TraceType.Growl:
                        _features[i] = new GrowlFeature();
                        break;
                    case TraceType.Appetitte:
                        _features[i] = new AppetitteFeature();
                        break;
                    case TraceType.Radiation:
                        _features[i] = new RadiationFeature();
                        break;
                    case TraceType.Temperature:
                        _features[i] = new TemperatureFeature();
                        break;
                    case TraceType.Track:
                        _features[i] = new TrackFeature();
                        break;
                    default:
                        break;
                }
                _features[i].Init(view);
            }
        }

        public void FaturesCancel()
        {
            for (int i = 0; i < 3; i++)
            {
                _features[i].Cancel();
            }
        }
    }
}