using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Ingame.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class Bullet : MonoBehaviour
    {
        private float _nowTime;
        private float _destroyTime = 3.0f;
        [SerializeField] private float speed;

        // Update is called once per frame
        void Update()
        {
            _nowTime += Time.deltaTime;

            if (_nowTime >= _destroyTime)
                Destroy(this.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Enemy")
            {
                //“G‚Éƒ_ƒ[ƒW‚ğ—^‚¦‚éˆ—
            }
            Destroy(this.gameObject);
        }
    }
}

