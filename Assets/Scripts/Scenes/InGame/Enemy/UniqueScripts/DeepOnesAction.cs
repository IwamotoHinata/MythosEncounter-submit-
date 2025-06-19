using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Ingame.Enemy
{
    public class DeepOnesAction : EnemyUniqueAction
    {
        private EnemySpawner _enemySpawner;
        [SerializeField]private bool _canSpawn = true;

        protected override void Start()
        {
            _enemySpawner = FindObjectOfType< EnemySpawner>();
        }

        protected override void Action() {
            if (_canSpawn) {
                GameObject createObject;
                createObject = _enemySpawner.EnemySpawn(EnemyName.DeepOnes, this.transform.position);
                if (createObject.TryGetComponent<DeepOnesAction>(out DeepOnesAction deepOnesActionScript))
                {
                    deepOnesActionScript.SetCanSpawn(false);
                }
                if (createObject.TryGetComponent<EnemyAttack>(out EnemyAttack enemyAttackScript))
                {
                    List<EnemyAttackBehaviour> setEnemyAttackBehaviours = new List<EnemyAttackBehaviour>();//新しく作るList
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
        /// スポーンさせる能力を与えるかどうか
        /// </summary>
        /// <param name="set">trueならスポーン能力あり、falseならスポーン能力なし</param>
        public void SetCanSpawn(bool set)
        {
            _canSpawn = set;
        }
    }
}