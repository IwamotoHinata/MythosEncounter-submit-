using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Ingame.Player
{
    public class BulletMagic : Magic
    {
        private GameObject _bullet;//銃弾のプレハブ

        private void Awake()
        {
            _bullet = (GameObject)Resources.Load("Prefab/Magic/Bullet");
        }
        public override void ChangeFieldValue()
        {
            consumeSanValue = 10;
            
        }

        public override void MagicEffect()
        {
            //球を射出する処理
            Vector3 shotPosition = this.transform.position + this.transform.forward * 2 + Vector3.up * 2;
            GameObject bullet = Instantiate(_bullet , shotPosition, Quaternion.identity);
            bullet.GetComponent<Rigidbody>().AddForce(this.transform.forward * 500f);
        }
    }
}
