using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Scenes.Ingame.Enemy
{
    public class EnemyUniqueAction : NetworkBehaviour
    {
        protected float _actionCoolDownTime;
        protected float _actionCoolDownTimeCount = 0;

        protected virtual void Start() { }

        public virtual void Init(int actionCoolDown) {
            _actionCoolDownTime = (float)actionCoolDown;
        }


        // Update is called once per frame
        public override void FixedUpdateNetwork()
        {
            _actionCoolDownTimeCount += Runner.DeltaTime;
            if (_actionCoolDownTimeCount > _actionCoolDownTime)
            {
                _actionCoolDownTimeCount -= _actionCoolDownTime;
                Action();
            }
        }
        protected virtual void Action() {
            Debug.LogWarning("設定されていないアクションが実行されました");
        }
    }
}
