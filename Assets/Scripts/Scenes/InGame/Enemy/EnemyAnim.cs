using Fusion;
using UnityEngine;
using UniRx;

namespace Scenes.Ingame.Enemy
{
    /// <summary>
    /// 敵キャラクターのアニメーション管理基底クラス
    /// </summary>
   
    public abstract class EnemyAnim : MonoBehaviour
    {
        protected Animator _animator;
        protected EnemyStatus _enemyStatus;

        protected virtual void Awake()
        {
            _animator = GetComponent<Animator>();
            _enemyStatus = GetComponent<EnemyStatus>();
            _enemyStatus.OnEnemyStateChange.Subscribe(x => {
                UpdateAnimationState(x);
            }).AddTo(this);
        }

        protected virtual void Update()
        {
        }

        protected abstract void UpdateAnimationState(EnemyState enemyState);

    }
}