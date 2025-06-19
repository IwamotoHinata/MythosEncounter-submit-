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
        //������FallBack�ɂȂ�

        _enemyStatus.SetEnemyState(EnemyState.FallBack);
        Debug.Log("�n���w���œP�ނ��܂���" + _enemyStatus.State);
    }

    public override void Init(int actionCoolDown)
    {
        _actionCoolDownTime = (float)actionCoolDown;
        //�����ňړ�������ǂݎ���ăT�[�`��ԂȂ�_���ׂ��o���҂𒊑I����
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
            if (_bloodCheckTimeCount > _bloodCheckTime) //��莞�Ԃ��Ƃɍs��
            { 
                _bloodCheckTimeCount = 0;
                _actionCoolDownTimeCount += _bloodCheckTime;
                _bloodPlayerStatuses.Clear();
                foreach (PlayerStatus playerStatus in _enemyStatus.MultiPlayManager.PlayerStatusList)
                { //�S�Ẵv���C���[���`�F�b�N
                    if (playerStatus.nowBleedingValue) { //�o�����Ă���̂ł����
                        _actionCoolDownTimeCount -= _bloodCheckTime;//�ގU�܂ł̎��Ԃ�����
                        _bloodPlayerStatuses.Add(playerStatus);
                    }
                }
                if (_enemyStatus.State == EnemyState.Patrolling || _enemyStatus.State == EnemyState.Searching)//�T����=�v���C���[��ǂ��񂵂Ă��Ȃ�
                {
                    if ((_targetPlayerStatus != null) && _targetPlayerStatus.nowBleedingValue)
                    { //�Ώۂ��o���𖢂��������Ă��Ȃ��ꍇ
                        _enemyMove.SetMovePosition(_targetPlayerStatus.transform.position);//�Ώۂ��ړ���Ɏw��
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
                Debug.Log("�^�[�Q�b�g��" + _targetPlayerStatus);
            }
        }
    }



    /// <summary>
    /// �H���������ꍇ�ɃA�N�Z�X���Ă��������B�ގU�܂ł̎��Ԃ���������܂�
    /// </summary>
    public void EatFood() {
        if (_debugMode) Debug.Log("�H�����܂����B���̓P�ނ܂ł̃J�E���g����" + _actionCoolDownTimeCount +"�F�H����̓P�ނ܂ł̃J�E���g����" +(float)(_actionCoolDownTimeCount - _extendedTimeWithMeals));
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
