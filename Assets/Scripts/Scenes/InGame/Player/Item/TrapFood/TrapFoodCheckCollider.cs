using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Scenes.Ingame.Player
{
    public class TrapFoodCheckCollider : MonoBehaviour
    {
        private bool _isTriggered = false;
        public bool IsTriggered { get { return _isTriggered; } }
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag != "TrapFoodSensor")
            {
                _isTriggered = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            _isTriggered = false;
        }

        public void ChangeTrigger(bool value)
        {
            _isTriggered = value;
        }
    }

}
