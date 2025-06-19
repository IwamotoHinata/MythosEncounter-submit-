using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Ingame.Stage
{
    public class StageManager : MonoBehaviour
    {
        public int StandardTemperature;
        void Awake()
        {
            StandardTemperature = Random.Range(15, 25);
        }
    }
}
