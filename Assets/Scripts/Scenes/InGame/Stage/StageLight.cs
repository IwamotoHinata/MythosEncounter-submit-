using Scenes.Ingame.Player;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Scenes.Ingame.Stage
{
    public class StageLight : MonoBehaviour, IInteractable
    {
        [SerializeField] private List<GameObject> _lightObject = new List<GameObject>();
        private float[] _lightStrength = new float[10];
        private Light[] _light = new Light[10];
        private bool _isOn = false;
        private bool _isAnimation = false;
        private Vector3 SWICH = new Vector3(0, -40, 0);
        private BoxCollider _lightCollider;
        private bool _initialStateOpen = true;
        private GameObject[] _child;
        private float ANIMATIONTIME = 0.3f;//switchのアニメーション時間

        void Awake()
        {
            _lightCollider = GetComponent<BoxCollider>();

            for (int i = 0; i < _lightObject.Count; i++)
            {
                _lightStrength[i] = _lightObject[i].GetComponent<Light>().intensity;
                _light[i] = _lightObject[i].GetComponent<Light>();
            }
            
            _child = new GameObject[]{ transform.GetChild(1).gameObject, transform.GetChild(2).gameObject};

            if (_initialStateOpen)
            {
                _lightCollider.isTrigger = false;
                LightOn();
                _isOn = true;
            }
        }
        public void Intract(PlayerStatus status, bool processWithConditionalBypass)
        {
            if (Input.GetMouseButtonDown(1) || processWithConditionalBypass)
            {
                SoundManager.Instance.PlaySe("se_switch00", transform.position);
                _lightCollider.isTrigger = true;
                if (_isAnimation != true)
                {
                    switch (_isOn)
                    {
                        case true:
                            LightOff();
                            _isOn = false;
                            break;
                        case false:
                            LightOn();
                            _isOn = true;
                            break;
                    }
                }
            }
        }

        private void AnimationComplete()
        {
            _lightCollider.isTrigger = false;
            _isAnimation = false;
        }

        private void LightOn()
        {
            _isAnimation = true;
            for (int i = 0; i < 2; i++)
            {
                _child[i].transform.DORotate(SWICH, ANIMATIONTIME).SetRelative(true).SetEase(Ease.InOutSine).OnComplete(AnimationComplete);
            }
            for (int i = 0; i < _lightObject.Count; i++)
            {
                _light[i].intensity = _lightStrength[i];
            }
        }

        private void LightOff()
        {
            _isAnimation = true;
            for (int i = 0;i < 2; i++){
                _child[i].transform.DORotate(-SWICH, ANIMATIONTIME).SetRelative(true).SetEase(Ease.InOutSine).OnComplete(AnimationComplete);
            }
            for (int i = 0; i < _lightObject.Count; i++)
            {
                _light[i].intensity = 0;
            } 
        }

        public string ReturnPopString()
        {
            if (_isOn)
                return "消灯";
            else
                return "点灯";
        }
    }
}