using UnityEngine;
using Fusion;

public class AnimatorStateSync : NetworkBehaviour
{
    [SerializeField]
    Animator _animator;
    [SerializeField]
    bool _enableAutoSync;
    [SerializeField, Tooltip("同期間隔[s]")]
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

        UpdateAutoSync();   //一定周期で自動でアニメーション同期をとる

        if (_layerSyncPeding == true)   //同期フラグがONなら
            SynchronizeStates();    //同期
    }

    public override void Render()
    {
        if (Object.IsProxy == true)
            UpdateStates();
    }

    protected void Awake()
    {
        _layerCount = _animator != null ? _animator.layerCount : 0; //アニメーターのレイヤー数を取得
    }

    void UpdateAutoSync()
    {
        if (_enableAutoSync == false)   //自動同期がOFFなら
            return;

        if (Runner.SimulationTime > _state.SyncTick * Runner.DeltaTime + _autoSyncInterval) //一定間隔で同期
            RequestSync();
    }

    void SynchronizeStates()
    {
        for(int i = 0; i < _layerCount; i++)
        {
            //すべてのアニメーターレイヤーの遷移が終了するまで待機
            if (_animator.IsInTransition(i) == true)
                return;
        }

        for(int i = 0; i < _layerCount; i++)
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(i);

            float time = stateInfo.normalizedTime % 1;  //整数部分を切り捨て

            _state.States.Set(i, new StateData(stateInfo.fullPathHash, time));  //バッファーにアニメーションとその時間をセット
        }

        _state.SyncTick = Runner.Tick;  //現在のTickをセット
        _layerSyncPeding = false;   //同期フラグをOFF
    }

    void UpdateStates()
    {
        //直近の2つのデータの状態と、その時間の差を取得
        if (TryGetSnapshotsBuffers(out NetworkBehaviourBuffer fromBuffer, out NetworkBehaviourBuffer toBuffer, out float alpha) == false)
            return;

        //各時間のState構造体のデータをキャストして取得
        State from = fromBuffer.ReinterpretState<State>();
        State to = toBuffer.ReinterpretState<State>();

        if (_layerVisibleSyncTick == from.SyncTick) //同Tickで複数回スナップショットを取得した場合
            return;

        _layerVisibleSyncTick = from.SyncTick;  

        for(int i = 0; i < _layerCount; i++)
        {
            var fromState = from.States.Get(i); //順番にバッファされたアニメーションを取得
            var toState = to.States.Get(i); 

            int stateHash = alpha < 0.5f ? fromState.StateHash : toState.StateHash; //より近いほうのアニメーションクリップを取得



            if(stateHash != 0)
            {
                bool stateChanged = fromState.StateHash != toState.StateHash;   //二つのデータのアニメーションが同じならtrue,違うならfalse

                float time = InterpolateTime(fromState.NormalizedTime, toState.NormalizedTime, alpha, stateChanged);
                _animator.Play(stateHash, i, time); //補完した時間からアニメーションをスタート
            }
        }
    }

    //現在のアニメーションの状態から自然に遷移させるために補完して計算
    static float InterpolateTime(float from, float to, float alpha, bool stateChanged)
    {
        if (to >= from) //toの方が大きければ
            return Mathf.Lerp(from, to, alpha); //補完したfromからtoへ補完した値を渡す

        float time = Mathf.Lerp(from, to + 1f, alpha);  //toの方が小さければ、to+1して補完する

        time = time > 1f ? time - 1f : time;    //1より小さいことを保証

        //alpha < 0.5に基づいてFromStateまたはToStateのいずれかに単純に丸められるため、状態が早く切り替わりすぎる可能性がある
        if (stateChanged == true && time > to)  
            return 0f;

        if (stateChanged == false && time < from)
            return 1f;

        return time;
    }

    public struct StateData : INetworkStruct
    {
        public readonly int StateHash;  //動作中のアニメーションクリップのハッシュ
        public float NormalizedTime;    //動作中のアニメーションの動作時間

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
