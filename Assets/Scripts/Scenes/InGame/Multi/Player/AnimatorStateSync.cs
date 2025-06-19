using UnityEngine;
using Fusion;

public class AnimatorStateSync : NetworkBehaviour
{
    [SerializeField]
    Animator _animator;
    [SerializeField]
    bool _enableAutoSync;
    [SerializeField, Tooltip("�����Ԋu[s]")]
    float _autoSyncInterval = 2f;

    [Networked]
    ref State _state => ref MakeRef<State>();

    int _layerVisibleSyncTick;
    bool _layerSyncPeding;
    int _layerCount = -1;

    public void RequestSync()
    {
        if (HasStateAuthority == false)
            return;

        if (_animator == null)
            return;

        _layerSyncPeding = true;

        SynchronizeStates();
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority == false)
            return;

        UpdateAutoSync();   //�������Ŏ����ŃA�j���[�V�����������Ƃ�

        if (_layerSyncPeding == true)   //�����t���O��ON�Ȃ�
            SynchronizeStates();    //����
    }

    public override void Render()
    {
        if (Object.IsProxy == true)
            UpdateStates();
    }

    protected void Awake()
    {
        _layerCount = _animator != null ? _animator.layerCount : 0; //�A�j���[�^�[�̃��C���[�����擾
    }

    void UpdateAutoSync()
    {
        if (_enableAutoSync == false)   //����������OFF�Ȃ�
            return;

        if (Runner.SimulationTime > _state.SyncTick * Runner.DeltaTime + _autoSyncInterval) //���Ԋu�œ���
            RequestSync();
    }

    void SynchronizeStates()
    {
        for(int i = 0; i < _layerCount; i++)
        {
            //���ׂẴA�j���[�^�[���C���[�̑J�ڂ��I������܂őҋ@
            if (_animator.IsInTransition(i) == true)
                return;
        }

        for(int i = 0; i < _layerCount; i++)
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(i);

            float time = stateInfo.normalizedTime % 1;  //����������؂�̂�

            _state.States.Set(i, new StateData(stateInfo.fullPathHash, time));  //�o�b�t�@�[�ɃA�j���[�V�����Ƃ��̎��Ԃ��Z�b�g
        }

        _state.SyncTick = Runner.Tick;  //���݂�Tick���Z�b�g
        _layerSyncPeding = false;   //�����t���O��OFF
    }

    void UpdateStates()
    {
        //���߂�2�̃f�[�^�̏�ԂƁA���̎��Ԃ̍����擾
        if (TryGetSnapshotsBuffers(out NetworkBehaviourBuffer fromBuffer, out NetworkBehaviourBuffer toBuffer, out float alpha) == false)
            return;

        //�e���Ԃ�State�\���̂̃f�[�^���L���X�g���Ď擾
        State from = fromBuffer.ReinterpretState<State>();
        State to = toBuffer.ReinterpretState<State>();

        if (_layerVisibleSyncTick == from.SyncTick) //��Tick�ŕ�����X�i�b�v�V���b�g���擾�����ꍇ
            return;

        _layerVisibleSyncTick = from.SyncTick;  

        for(int i = 0; i < _layerCount; i++)
        {
            var fromState = from.States.Get(i); //���ԂɃo�b�t�@���ꂽ�A�j���[�V�������擾
            var toState = to.States.Get(i); 

            int stateHash = alpha < 0.5f ? fromState.StateHash : toState.StateHash; //���߂��ق��̃A�j���[�V�����N���b�v���擾



            if(stateHash != 0)
            {
                bool stateChanged = fromState.StateHash != toState.StateHash;   //��̃f�[�^�̃A�j���[�V�����������Ȃ�true,�Ⴄ�Ȃ�false

                float time = InterpolateTime(fromState.NormalizedTime, toState.NormalizedTime, alpha, stateChanged);
                _animator.Play(stateHash, i, time); //�⊮�������Ԃ���A�j���[�V�������X�^�[�g
            }
        }
    }

    //���݂̃A�j���[�V�����̏�Ԃ��玩�R�ɑJ�ڂ����邽�߂ɕ⊮���Čv�Z
    static float InterpolateTime(float from, float to, float alpha, bool stateChanged)
    {
        if (to >= from) //to�̕����傫�����
            return Mathf.Lerp(from, to, alpha); //�⊮����from����to�֕⊮�����l��n��

        float time = Mathf.Lerp(from, to + 1f, alpha);  //to�̕�����������΁Ato+1���ĕ⊮����

        time = time > 1f ? time - 1f : time;    //1��菬�������Ƃ�ۏ�

        //alpha < 0.5�Ɋ�Â���FromState�܂���ToState�̂����ꂩ�ɒP���Ɋۂ߂��邽�߁A��Ԃ������؂�ւ�肷����\��������
        if (stateChanged == true && time > to)  
            return 0f;

        if (stateChanged == false && time < from)
            return 1f;

        return time;
    }

    public struct StateData : INetworkStruct
    {
        public readonly int StateHash;  //���쒆�̃A�j���[�V�����N���b�v�̃n�b�V��
        public float NormalizedTime;    //���쒆�̃A�j���[�V�����̓��쎞��

        public StateData(int stateHash, float normalizedTime)
        {
            StateHash = stateHash;
            NormalizedTime = normalizedTime;
        }
    }

    public struct State : INetworkStruct
    {
        public int SyncTick;

        [Networked, Capacity(12)]
        public NetworkArray<StateData> States => default;
    }
}
