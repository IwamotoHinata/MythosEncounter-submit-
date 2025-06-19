using System.Collections;
using System.Collections.Generic;
using Scenes.Ingame.Player;
using UnityEngine;
using UniRx;
using UnityEngine.AI;
using Fusion;

namespace Scenes.Ingame.Enemy
{
    /// <summary>
    /// �p�g���[������B�v���C���[�̍��Ղ�T���B�����Ԃƍ��G��Ԃ̓��������肵�A�ǐՂƍU����Ԃւ̈ڍs���s���B
    /// </summary>
    public class EnemySearch : NetworkBehaviour
    {
        protected EnemyVisibilityMap _myVisivilityMap;
        [SerializeField]
        protected float _checkRate;//���b���ƂɎ��E�̏�Ԃ��`�F�b�N���邩
        protected float _checkTimeCount;//�O��`�F�b�N���Ă���̎��Ԃ��v��
        [SerializeField]
        protected bool _debugMode;
        [SerializeField]
        protected EnemyMove _myEnemyMove;
        protected NavMeshAgent _myAgent;


        [Tooltip("���E�̒����A���̓}�b�v�[����[�܂Ō�����悤�ɂ��Ă��܂��B����������ƌy�ʉ��\")]protected float _visivilityRange = 500;
        [SerializeField]
        protected EnemyStatus _enemyStatus;
        protected float _audiomaterPower;

        public EnemyVisibilityMap MyVisivillityMap { get => _myVisivilityMap; }

        //���G�s���̃N���X�ł�
        // Start is called before the first frame update
        void Start()
        {



        }

        /// <summary>
        /// �O�����炱�̃X�N���v�g�̏����ݒ�����邽�߂ɌĂяo��
        /// </summary>
        public void Init(EnemyVisibilityMap setVisivilityMap)
        {
            _myVisivilityMap = setVisivilityMap;
            _myAgent = GetComponent<NavMeshAgent>();

            //�X�y�b�N�̏����ݒ�
            _audiomaterPower = _enemyStatus.AudiometerPower;



            //�X�y�b�N�̕ύX���󂯎��
            _enemyStatus.OnAudiometerPowerChange.Subscribe(x => { _audiomaterPower = x; }).AddTo(this);


            foreach (PlayerStatus playerStatus in _enemyStatus.MultiPlayManager.PlayerStatusList)
            {
                playerStatus.OnPlayerActionStateChangeEvent.Subscribe(x =>
                {
                    //�v���C���[�̑����𕷂�
                    if (_enemyStatus.State == EnemyState.Patrolling || _enemyStatus.State == EnemyState.Searching)//�������T���Ă���Ƃ��ł����
                    {
                        float valume = 0;
                        switch (x.nowPlayerActionState)
                        {
                            case PlayerActionState.Sneak:
                                valume = playerStatus.nowPlayerSneakVolume;
                                break;
                            case PlayerActionState.Walk:
                                valume = playerStatus.nowPlayerWalkVolume;
                                break;
                            case PlayerActionState.Dash:
                                valume = playerStatus.nowPlayerRunVolume;
                                break;
                        }
                        if (Mathf.Pow((float)(valume * _audiomaterPower * 0.01f), 2f) - (Mathf.Pow(transform.position.x - playerStatus.transform.position.x, 2) + (Mathf.Pow(transform.position.y - playerStatus.transform.position.y, 2))) > 0)
                        {//�����������邩�ǂ���
                            _myVisivilityMap.HearingSound(playerStatus.transform.position, 15, false);
                            _myEnemyMove.SetMovePosition(_myVisivilityMap.GetNextNearWatchPosition(this.transform.position));
                        }
                    }
                }).AddTo(this);
            }

        }


        public override void FixedUpdateNetwork()
        {
            if (_myVisivilityMap != null)//���G�̏������ł��Ă��Ȃ��ꍇ
            {
                Debug.LogError("�}�b�v��񂪂���܂���A_myVisivilityMap���쐬���Ă�������");
                return;
            }

            if (_enemyStatus.State == EnemyState.Patrolling || _enemyStatus.State == EnemyState.Searching)
            { //�����Ԃ܂��͑{����Ԃ̏ꍇ
                //����I�Ɏ��E���𒲂ׂ�
                _checkTimeCount += Runner.DeltaTime;
                if (_checkTimeCount > _checkRate)
                {
                    _checkTimeCount = 0;
                    //�����Ȃ��̂𒲂ׂ�B����͌���I�Ȃ��̂قǗD�悵�ĔF������
                    if (CheckCanSeeThePlayer())
                    {//�v���C���[���g�������邩���ׂ�
                    }
                    else if (CheckCanSeeTheLight())
                    {//�v���C���[�̎����������邩���ׂ�
                    }                    
                    else if (CheckCanHearThePlayerSound()) 
                    { //�v���C���[�̑������������邩���ׂ�
                    }
                    else
                    {
                        //�Ȃ�̍��Ղ�������Ȃ������ꍇ���ʂɏ��񂷂�
                        _myVisivilityMap.CheckVisivility(this.transform.position, _visivilityRange);
                        
                        if (_myEnemyMove.EndMove)//�ړ����I����Ă���ꍇ
                        {
                            if (_debugMode) { Debug.Log(_myEnemyMove.GetMovePosition()); }
                            _myVisivilityMap.ChangeGridWatchNum(_myEnemyMove.GetMovePosition(), 1, true);
                            //���Ղ̂������ꏊ�܂ŗ������������Ȃ������ꍇ���������s�����̂�Status������������
                            _enemyStatus.SetEnemyState(EnemyState.Patrolling);
                            //���炽�Ȉړ�����擾���郁�\�b�h����������
                            _myEnemyMove.SetMovePosition(_myVisivilityMap.GetNextNearWatchPosition(this.transform.position));
                            
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �v���C���[�𒼐ڎ��F�\���ǂ����𒲂ׂ�
        /// </summary>
        /// <returns></returns>
        protected bool CheckCanSeeThePlayer() {
            bool see = false;//���邱�Ƃ��ł���
            float approachRange = float.MaxValue;//�ǂ�������v���C���[�̋���
            Vector3 tgtPosition = _myEnemyMove.GetMovePosition();
            foreach (PlayerStatus playerStatus in _enemyStatus.MultiPlayManager.PlayerStatusList)
            {//�v���C���[���Ƃ̏���
                float range = Vector3.Magnitude(this.transform.position - playerStatus.transform.position);//�����������߂�̂͂������R�X�g���d���炵���̂Ŋm���Ɍv�Z���K�v�ɂȂ��Ă��炵�Ă܂�
                bool hit = true;
                Ray ray = new Ray(this.transform.position + new Vector3(0,2,0), playerStatus.transform.position - this.transform.position);
                hit = Physics.Raycast(ray, out RaycastHit hitInfo, range, -1 ^ LayerMask.GetMask(new string[] { "Ignore Raycast", "Player" }), QueryTriggerInteraction.Collide);
                if (!hit) 
                { //���ɂ��������Ă��Ȃ�=�Ώ̂܂Œ����I�Ɏ��E���ʂ�
                    SanCheck(playerStatus);
                    if (_visivilityRange > range) //�����鋗����
                    {
                        if (true) { //����p�����Ƀv���C���[�����邩�ǂ���=���͂�����True
                            see = true;
                            if (_debugMode) { Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 5); Debug.Log("�v���C���[����"); }
                            if (approachRange > range)
                            { //��قǂ܂łɒ��ڌ��邱�Ƃ̂ł����v���C���[���߂��v���C���[�ł���ꍇ
                                approachRange = range;
                                tgtPosition = playerStatus.transform.position;
                            }
                        }
                    }
                }
            }
            if (see)
            { //�������ǐՂ���ꍇ
                
                _myEnemyMove.SetMovePosition(tgtPosition);
                _enemyStatus.SetEnemyState(EnemyState.Discover);
                return true;
            }
            else { 
                return false;
            }
        }

        protected bool CheckCanSeeTheLight() {
            Vector3 neerLightPosition = _myEnemyMove.GetMovePosition();
            bool canWatchLight = false;
            if (_enemyStatus.ReactToLight ) 
            {
                //�����������������邩���ׂ�
                foreach (PlayerStatus playerStatus in _enemyStatus.MultiPlayManager.PlayerStatusList)
                {//�v���C���[���Ƃ̏���
                    Vector3 positionCandidate = Vector3.zero;
                    if (_myVisivilityMap.RightCheck(this.transform.position, playerStatus.transform.position, _visivilityRange, playerStatus.nowPlayerLightRange, ref  positionCandidate)) {
                        //�Ώ̂̃v���C���[�̌��������邩�ǂ���
                        if (canWatchLight) {//����������
                            if ((this.transform.position - neerLightPosition).magnitude > (this.transform.position - positionCandidate).magnitude) {
                                neerLightPosition = positionCandidate;
                            }
                        }else{
                            canWatchLight = true;
                            neerLightPosition = positionCandidate;
                        }
                    }
                }
            }
            if (canWatchLight)
            {
                //���������邩���ׂ�
                if (_debugMode) Debug.Log("����������");
                _enemyStatus.SetEnemyState(EnemyState.Searching);
                _myEnemyMove.SetMovePosition(neerLightPosition);
                return true;
            }
            else {            
            return false; 
            }
        }

        protected bool CheckCanHearThePlayerSound() {
            bool canHear = false;

            
            foreach (PlayerStatus playerStatus in _enemyStatus.MultiPlayManager.PlayerStatusList)
            {
                float valume = 0;//�v���C���[�̑������L�^
                switch (playerStatus.nowPlayerActionState)
                {
                    case PlayerActionState.Sneak:
                        valume = playerStatus.nowPlayerSneakVolume;
                        if (_debugMode) Debug.Log("�E�ԉ�����������");
                        break;
                    case PlayerActionState.Walk:
                        valume = playerStatus.nowPlayerWalkVolume;
                        if (_debugMode) Debug.Log("����������������");
                        break;
                    case PlayerActionState.Dash:
                        valume = playerStatus.nowPlayerRunVolume;
                        if (_debugMode) Debug.Log("���鉹����������");
                        break;
                }
                if (Mathf.Pow((float)(valume * _audiomaterPower * 0.01f), 2f) - (Mathf.Pow(transform.position.x - playerStatus.transform.position.x, 2) + (Mathf.Pow(transform.position.y - playerStatus.transform.position.y, 2))) > 0)
                {
                    _myVisivilityMap.HearingSound(playerStatus.transform.position, 15, true);
                    canHear = true;
                }
            }
            if (canHear)
            {
                _enemyStatus.SetEnemyState(EnemyState.Searching);
                _myEnemyMove.SetMovePosition(_myVisivilityMap.GetNextNearWatchPosition(this.transform.position));
                return true;
            }
            else { return false; }
        }

        protected void SanCheck(PlayerStatus playerStatus) { 
        //#######################################�v���C���[�̏�񂪕K�v�ł��B
        }


    } 
}
