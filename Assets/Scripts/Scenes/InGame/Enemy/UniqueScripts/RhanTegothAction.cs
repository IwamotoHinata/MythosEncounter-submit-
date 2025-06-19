using Cysharp.Threading.Tasks;
using Scenes.Ingame.Enemy;
using Scenes.Ingame.Player;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class RhanTegothAction : EnemyUniqueAction
{
    [SerializeField]private float _extendedTimeWithMeals;
    [SerializeField]private EnemyStatus _enemyStatus;
    [SerializeField] private float _bloodCheckTime;
    [SerializeField] private EnemyMove _enemyMove;
    [SerializeField] private bool _debugMode;
    private float _bloodCheckTimeCount;
    private PlayerStatus _targetPlayerStatus;
    private List<PlayerStatus> _bloodPlayerStatuses = new List<PlayerStatus>();
    protected override void Action()
    {
        //ここでFallBackになる

        _enemyStatus.SetEnemyState(EnemyState.FallBack);
        Debug.Log("ハラヘリで撤退しました" + _enemyStatus.State);
    }

    public override void Init(int actionCoolDown)
    {
        _actionCoolDownTime = (float)actionCoolDown;
        //ここで移動完了を読み取ってサーチ状態なら狙うべき出血者を抽選する
        _enemyMove._OnEndMoveChanged.Subscribe(x => { 
            if (x && (_bloodPlayerStatuses.Count > 0)) {
                int targetNumber = Random.Range(0, _bloodPlayerStatuses.Count);
                _targetPlayerStatus = _bloodPlayerStatuses[targetNumber];
            } 
        }).AddTo(this);

    }

    public override void FixedUpdateNetwork()
    {
        if (_enemyStatus.State != EnemyState.FallBack) {
            _bloodCheckTimeCount += Runner.DeltaTime;
            if (_bloodCheckTimeCount > _bloodCheckTime) //一定時間ごとに行う
            { 
                _bloodCheckTimeCount = 0;
                _actionCoolDownTimeCount += _bloodCheckTime;
                _bloodPlayerStatuses.Clear();
                foreach (PlayerStatus playerStatus in _enemyStatus.MultiPlayManager.PlayerStatusList)
                { //全てのプレイヤーをチェック
                    if (playerStatus.nowBleedingValue) { //出血しているのであれば
                        _actionCoolDownTimeCount -= _bloodCheckTime;//退散までの時間を延長
                        _bloodPlayerStatuses.Add(playerStatus);
                    }
                }
                if (_enemyStatus.State == EnemyState.Patrolling || _enemyStatus.State == EnemyState.Searching)//探索中=プレイヤーを追い回していない
                {
                    if ((_targetPlayerStatus != null) && _targetPlayerStatus.nowBleedingValue)
                    { //対象が出血を未だ治癒していない場合
                        _enemyMove.SetMovePosition(_targetPlayerStatus.transform.position);//対象を移動先に指定
                    }
                    else { 
                        _targetPlayerStatus = null;
                    }
                }

                if (_actionCoolDownTimeCount > _actionCoolDownTime)
                {
                    _actionCoolDownTimeCount -= _actionCoolDownTime;
                    Action();
                }
            }
            if (_debugMode) 
            {
                Debug.Log("ターゲットは" + _targetPlayerStatus);
            }
        }
    }



    /// <summary>
    /// 食事をした場合にアクセスしてください。退散までの時間が延長されます
    /// </summary>
    public void EatFood() {
        if (_debugMode) Debug.Log("食事しました。元の撤退までのカウント時間" + _actionCoolDownTimeCount +"：食事後の撤退までのカウント時間" +(float)(_actionCoolDownTimeCount - _extendedTimeWithMeals));
        _actionCoolDownTimeCount -= _extendedTimeWithMeals;
    }

    public override void Render() {
        if (_debugMode) {
            if (Input.GetKeyDown(KeyCode.L)) {
                EatFood();
            }
        }
    }
}
