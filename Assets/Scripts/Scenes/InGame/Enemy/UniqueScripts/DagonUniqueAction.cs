using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Scenes.Ingame.Manager;

namespace Scenes.Ingame.Enemy
{
    public class DagonUniqueAction : EnemyUniqueAction
    {
        [Networked] public bool canSpawn { get; private set; } = true;
        private EnemySpawner _enemySpawner;
        private IngameManager _ingameManager;
        [SerializeField] private bool _nonInGameManagerMode;


        protected override void Start()
        {
            _enemySpawner = FindObjectOfType<EnemySpawner>();
            if (!_nonInGameManagerMode) { 
                _ingameManager = FindObjectOfType<IngameManager>();
            }
        }

        protected override void Action()
        {
            if (!_nonInGameManagerMode && canSpawn)
            {
                canSpawn = false;
                for (int i = 0;i < _ingameManager.GetEscapeItemCurrentCount;i++) {
                    _enemySpawner.EnemySpawn(EnemyName.DeepOnes, this.transform.position);
                }
            }

        }
    }
}
