using Cysharp.Threading.Tasks;
using Scenes.Ingame.Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class SubNiggurathAction : EnemyUniqueAction
{
    private EnemySpawner _enemySpawner;
    private EnemyAttack _enemyAttack;


    protected override void Start()
    {
        _enemySpawner = FindObjectOfType<EnemySpawner>();
        _enemyAttack = GetComponent<EnemyAttack>();
        _enemyAttack.OnPlayerKill.Subscribe(_ => {
            Debug.Log("Player‚ğƒLƒ‹‚µ‚½‚Ì‚Å‘B‚µ‚Ü‚·");
            _enemySpawner.EnemySpawn(EnemyName.SubNiggurath, this.transform.position);
        });
    }

    protected override void Action()
    {

    } 
}
