using Scenes.Ingame.Player;
using UnityEngine;
using Fusion;

public class EnemyLaser : NetworkBehaviour, IEnemyRangeAttack
{
    [SerializeField] private int _damage;
    [SerializeField] private int _breedDamage;
    [SerializeField] private float _lifeTime;
    [SerializeField][Tooltip("命中率、1なら必中")] private float _accuracy;
    [SerializeField][Tooltip("命中しなかった場合、対象からどれだけの距離をずらして弾頭を飛翔させるか")] private float _shootingErrorDistance;
    [SerializeField][Tooltip("ターゲット座標に対する補正、プレイヤー座標が地面に触れていることに対応")] private Vector3 _hitPositionSetting;

    private PlayerStatus _targetStatus;
    private NetworkObject _targetNetworkObject;
    private GameObject _targetObject;

    private Vector3 _shootingErrorVector;
    private bool _overShoot;//相手を超えたら
    private NetworkId _shooterNetworkId;

    [Networked] private bool _hit { get; set; } = false;//命中するかどうか

    public void Init(NetworkId targetId, NetworkId enemyId)
    {
        Runner.TryFindObject(targetId, out _targetNetworkObject);
        _targetObject = _targetNetworkObject.gameObject;
        _targetStatus = _targetObject.GetComponent<PlayerStatus>();
        _shooterNetworkId = enemyId;
        if (HasStateAuthority)
        {//どのような条件で何が起こるか決めるのはホスト（なおInitはホストでしか呼ばれない）
            if (UnityEngine.Random.RandomRange(0f, 1f) <= _accuracy)
            {
                _hit = true;
            }
            this.transform.rotation = Quaternion.LookRotation((_targetObject.transform.position + _hitPositionSetting + _shootingErrorVector - this.transform.position), Vector3.up);
            _shootingErrorVector = new Vector3(0, 0, 0);
            if (!_hit)
            {
                float _targetAngleY;
                _targetAngleY = Mathf.Atan2(_targetObject.transform.position.x + _hitPositionSetting.x - this.transform.position.x, _targetObject.transform.position.z + _hitPositionSetting.z - this.transform.position.z);
                if (UnityEngine.Random.RandomRange(0, 2) == 0)//左右どちらにずらすか
                {
                    _targetAngleY += Mathf.PI / 4;
                }
                else
                {
                    _targetAngleY += Mathf.PI / -4;
                }
                _shootingErrorVector.x = _shootingErrorDistance * Mathf.Cos(_targetAngleY);
                _shootingErrorVector.z = _shootingErrorDistance * Mathf.Sin(_targetAngleY);
                if (UnityEngine.Random.RandomRange(0, 2) == 0)//上下どちらにずらすか
                {
                    _shootingErrorVector.z = _shootingErrorDistance;
                }
                else
                {
                    _shootingErrorVector.z = -_shootingErrorDistance;
                }
                this.transform.rotation = Quaternion.LookRotation((_targetObject.transform.position + _hitPositionSetting + _shootingErrorVector - this.transform.position), Vector3.up);
            }
        }
    }
    public override void FixedUpdateNetwork()
    {
        _lifeTime -= Time.deltaTime;

        if (_lifeTime < 0)
        {
            Runner.Despawn(GetComponent<NetworkObject>());
        }
    }
}
