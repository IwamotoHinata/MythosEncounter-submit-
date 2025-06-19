using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Scene.Ingame.GlobalSettings
{
    public class CausesChromaticAberration : MonoBehaviour
    {
        [SerializeField] Volume _volume;
        ChromaticAberration _chromaticAberration;

        [SerializeField][Tooltip("色収差エフェクトかかった後に何秒かけて無効化されるか")]private float _chromaticAberrationTime;

        private float _chromaticAberrationTimeCount = 0;

        // Start is called before the first frame update
        void Start()
        {
            _volume.profile.TryGet<ChromaticAberration>(out _chromaticAberration);
        }

        // Update is called once per frame
        void Update()
        {
            Debug.Log("_chromaticAberration.intensity.value" + _chromaticAberration.intensity.value);
            if (_chromaticAberrationTimeCount > 0)
            {
                _chromaticAberrationTimeCount -= Time.deltaTime;
                _chromaticAberration.intensity.value = 0;
            }
            else {
                _chromaticAberration.intensity.value = 1;
            }           
        }

        public void AddIntensity() {
            _chromaticAberrationTimeCount = _chromaticAberrationTime;
        }
    }
}
