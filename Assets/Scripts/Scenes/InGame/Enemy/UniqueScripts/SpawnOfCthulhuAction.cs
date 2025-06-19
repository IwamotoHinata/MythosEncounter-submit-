using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Ingame.Enemy
{
    public class SpawnOfCthulhuAction : EnemyUniqueAction
    {
        private EnemySpawner _enemySpawner;
        [SerializeField]private bool _canSpawn;
        private bool _nonInGameManagerMode;

        protected override void Start()
        {
            _enemySpawner = FindObjectOfType<EnemySpawner>();
        }

        protected override void Action()
        {
            if (_canSpawn) {
                _canSpawn = false;
                GameObject createObject;
                createObject = _enemySpawner.EnemySpawn(EnemyName.DeepOnes, this.transform.position);
                if (createObject.TryGetComponent<DeepOnesAction>(out DeepOnesAction deepOnesActionScript))
                {
                    deepOnesActionScript.SetCanSpawn(false);
                }
                if (createObject.TryGetComponent<EnemyAttack>(out EnemyAttack enemyAttackScript))
                {
                    List<EnemyAttackBehaviour> setEnemyAttackBehaviours = new List<EnemyAttackBehaviour>();//�V�������List
                    foreach (EnemyAttackBehaviour enemyAttackBehaviourScript in enemyAttackScript.GetEnemyAtackBehaviours())
                    {
                        if (enemyAttackBehaviourScript.GetType() != typeof(EnemyShootingAtackBehaviour))
                        {
                            setEnemyAttackBehaviours.Add(enemyAttackBehaviourScript);
                        }
                    }
                    enemyAttackScript.SetEnemyAtackBehaviours(setEnemyAttackBehaviours);
                }

            }


        }

        /// <summary>
        /// �X�|�[��������\�͂�^���邩�ǂ���
        /// </summary>
        /// <param name="set">true�Ȃ�X�|�[���\�͂���Afalse�Ȃ�X�|�[���\�͂Ȃ�</param>
        public void SetCanSpawn(bool set) { 
        _canSpawn = set;
        }
    }
}
