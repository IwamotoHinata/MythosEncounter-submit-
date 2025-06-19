using UnityEngine;
using UniRx;
using Scenes.Ingame.Player;
using Fusion;
using System;

namespace Scenes.Ingame.Enemy
{
    public class EnemyComboCloseAttackBehaviourScript : EnemyAttackBehaviour
    {
        [SerializeField] private int _damage;
        [SerializeField] private int _breedDamage;
        [SerializeField] private int _comboDamage;
        [SerializeField] private int _comboBreedDamage;
        [SerializeField] private float _comboStiffness;

        [SerializeField] private float _comboReceptionTime;
        [Networked] public float _comboReceptionTimeCount { get; private set; }

        public override void Behaviour(PlayerStatus targetStatus)
        {
            if (_comboReceptionTime > _comboReceptionTimeCount)
            {
                Debug.Log("コンボ攻撃！");
                SoundManager.Instance.PlaySe("se_punching00", transform.position);
                targetStatus.ChangeHealth(_comboDamage, ChangeValueMode.Damage);
                targetStatus.ChangeBleedingHealth(_comboBreedDamage);
                targetStatus.OnEnemyAttackedMeEvent.OnNext(Unit.Default);
            }
            else {
                Debug.Log("コンボ前攻撃！");
                _comboReceptionTimeCount = 0;
                SoundManager.Instance.PlaySe("se_punching00", transform.position);
                targetStatus.ChangeHealth(_damage, ChangeValueMode.Damage);
                targetStatus.ChangeBleedingHealth(_breedDamage);
                targetStatus.OnEnemyAttackedMeEvent.OnNext(Unit.Default);
            }

        }

        public override void FixedUpdateNetwork()
        {
            if (_comboReceptionTime > _comboReceptionTimeCount) {
                _comboReceptionTimeCount += Runner.DeltaTime;
            }
        }

        public override float GetStiffness()
        {
            if (_comboReceptionTime > _comboReceptionTimeCount)
            {
                return _comboStiffness;
            }
            else {
                return _stiffness;
            }
               
        }
    }
}
