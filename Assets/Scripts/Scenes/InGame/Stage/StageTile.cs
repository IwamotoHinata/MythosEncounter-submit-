using System.Collections;
using UnityEngine;

namespace Scenes.Ingame.Stage
{
    public class StageTile : MonoBehaviour
    {
        [SerializeField] private float _temperature;
        [SerializeField] private float _keep;
        [SerializeField] private int _msv;
        private const float TIME = 120;
        private float _count = 0;
        private bool _flag = false;
        private float _over;
        private float _max;
        private float _min;


        void Start()
        {
            _temperature = FindObjectOfType<StageManager>().StandardTemperature;
            _keep = _temperature;
            _max = _temperature + 3;
            _min = _temperature - 3;
            StartCoroutine(InMsvChange());
            StartCoroutine(InTemperatureChange());
        }

        //温度が設定温度の範囲外になった際の処理
        private void OverTemperature()
        {
            _count += 1;

            if (_count < TIME)
            {
                _temperature = Mathf.Lerp(_over, _keep, _count / TIME);
            }
            else
            {
                _temperature = _keep;
                _count = 0;
                _flag = false;
            }
       
           
        }

        //msvの変化の処理
        IEnumerator InMsvChange()
        {
            while (true)
            {
                if (100 < _msv)
                {
                    _msv -= 1;
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    _msv = Random.Range(90, 101);
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        //温度の変化の処理
        IEnumerator InTemperatureChange()
        {
            while (true)
            {
                if (_keep != _temperature)
                {
                    if (!_flag)
                    {
                        _over = _temperature;
                        _flag = true;
                    }
                    OverTemperature();
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        _temperature += 3;
                        if (_max < _temperature)
                            _temperature = _max;
                    }
                    else
                    {
                        _temperature -= 3;
                        if (_temperature < _min)
                            _temperature = _min;
                    }
                    _keep = _temperature;
                    yield return new WaitForSeconds(10f);
                }
            }
        }

        //外部からの変更
        public void TemperatureChange(float value)
        {
            _temperature = value;
        }

        public void MsvChange(int value)
        {
            _msv = value;
        }

        //外部からの参照
        public float Temperature
        {
            get { return _temperature; } 
        }

        public int Msv
        {
            get { return _msv; } 
        }

        public float Keep
        {
            get { return _keep; }
        }
    }
}

