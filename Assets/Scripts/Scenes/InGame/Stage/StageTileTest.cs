using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Ingame.Stage
{
    public class StageTileTest : MonoBehaviour
    {
        private float _Test;
        [SerializeField] private int _count;
        public StageTile stagetile;

        void Start()
        {
            StartCoroutine(Test());
            StartCoroutine(Count());
        }

        IEnumerator Count()
        {
            while (true)
            {
                _count += 1;
                if (_count == 140)
                    _count = 0;
                yield return new WaitForSeconds(1f);
            }
        }

        IEnumerator Test()
        {
            while (true)
            {
                _Test = stagetile.Temperature - 10;
                stagetile.TemperatureChange(_Test);
                yield return new WaitForSeconds(140f);
            }
        }
    }
}
