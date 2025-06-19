using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// 発狂の種類の1つ
    /// 1.ミニマップにグリッチノイズが走り見えなくなる
    /// 2.視野が狭くなる（PostProcessing）
    /// </summary>
    public class EyeParalyze : MonoBehaviour, IInsanity
    {
        private Volume _volume;
        private Vignette _vignette;
        private PlayerGUIPresenter _playerGUIPresenter;
        public void Setup()
        {
            _volume = FindObjectOfType<Volume>();
            if (!_volume.profile.TryGet<Vignette>(out _vignette))
            {
                _vignette = _volume.profile.Add<Vignette>(false);
            }

            _playerGUIPresenter = FindObjectOfType<PlayerGUIPresenter>();
        }

        public void Active()
        {
            //視野狭める
            _vignette.active = true;//Vignetteの有効化

            //Mapにノイズを走らせる(MiniMapは非表示)
            _playerGUIPresenter.MiniMapSetting(false);
            _playerGUIPresenter.NoiseFilterSetting(true);
        }

        public void Hide()
        {
            _vignette.active = false;//Vignetteの無効化
            _playerGUIPresenter.MiniMapSetting(true);
            _playerGUIPresenter.NoiseFilterSetting(false);
        }
    }
}