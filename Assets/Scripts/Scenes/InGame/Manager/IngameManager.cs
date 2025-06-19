using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Scenes.Ingame.InGameSystem;
using Fusion;

namespace Scenes.Ingame.Manager
{
    public class IngameManager : MonoBehaviour
    {
        private IngameState _currentState = IngameState.Outgame;
        public static IngameManager Instance;
        private IngameReady _ingameReady;
        [SerializeField] private bool _ingameSingleDebug = false;

        private Subject<Unit> _initialEvent = new Subject<Unit>();
        private Subject<Unit> _ingameEvent = new Subject<Unit>();
        private Subject<Unit> _openEscapePointEvent = new Subject<Unit>();
        private Subject<int> _escapeItemEvent = new Subject<int>();
        private Subject<Unit> _resultEvent = new Subject<Unit>();
        private Subject<Unit> _outgameEvent = new Subject<Unit>();
        private Subject<Unit> _stageGenerateEvent = new Subject<Unit>();
        private Subject<Unit> _playerSpawnEvent = new Subject<Unit>();
        public IObservable<Unit> OnInitial { get { return _initialEvent; } }
        public IObservable<Unit> OnIngame { get { return _ingameEvent; } }
        public IObservable<Unit> OnOpenEscapePointEvent { get { return _openEscapePointEvent; } }
        public IObservable<int> OnEscapeItemEvent { get { return _escapeItemEvent; } }
        public IObservable<Unit> OnResult { get { return _resultEvent; } }
        public IObservable<Unit> OnOutgame { get { return _outgameEvent; } }
        public IObservable<Unit> OnStageGenerateEvent { get { return _stageGenerateEvent; } }
        public IObservable<Unit> OnPlayerSpawnEvent { get { return _playerSpawnEvent; } }

        public IngameState CurrentState { get => _currentState; }

        [SerializeField]
        private int _escapeItemCount;//�E�o�܂łɕK�v�ȒE�o�A�C�e���̐�
        private ReactiveProperty<int> _getEscapeItemCount = new ReactiveProperty<int>();
        public IObservable<int> OnEscapeCount => _getEscapeItemCount; //���ݎ擾���Ă���E�o�A�C�e���̐�

        private Subject<Unit> _getJournal = new Subject<Unit>();

        public IObservable<Unit> OnGetJournal => _getJournal; //�W���[�i���̒f�Ђ�N�����E�����ۂɔ��s�����C�x���g
        public int GetEscapeItemCurrentCount { get => _getEscapeItemCount.Value; }
        public int GetEscapeItemMaxCount { get => _escapeItemCount; }

        void Awake()
        {
            Instance = this;
            OnInitial.Subscribe(_ => InitialState().Forget()).AddTo(this);
            OnIngame.Subscribe(_ => InGameState()).AddTo(this);
            OnResult.Subscribe(_ => ResultState()).AddTo(this);
            OnOutgame.Subscribe(_ => OutGameState()).AddTo(this);
            OnStageGenerateEvent.Subscribe(_ => OnStageReady()).AddTo(this);
            OnPlayerSpawnEvent.Subscribe(_ =>
            {
                try
                {
                    SoundManager.Instance.PlayBgm("bgm_main_1");
                    SubBgmManager.Instance.Init();
                }
                catch (Exception e)
                {
                    Debug.LogError($"SoundManager��SubBgmManager��Hierarchy�ɂ���܂���B");
                }
            }).AddTo(this);
        }
        private async void Start()
        {
            await Task.Delay(500);
            _initialEvent.OnNext(default);
            Debug.Log("Current State is Initial!");
        }
        private async UniTaskVoid InitialState()
        {
            _currentState = IngameState.Initial;
            await UniTask.WaitUntil(() => _ingameReady.Ready());
            _ingameEvent.OnNext(default);
        }

        private void InGameState()
        {
            Debug.Log("Current State is InGame!");
            _currentState = IngameState.Ingame;
        }

        private void ResultState()
        {
            Debug.Log("Current State is Result!");
            _currentState = IngameState.Result;
        }

        private void OnStageReady()
        {
            if (_ingameSingleDebug)
            {
                var runner = FindObjectOfType<RunnerSpawner>();
                runner.DirectStart(GameMode.Single);
            }
        }
        private void OutGameState()
        {
            Debug.Log("Current State is OutGame!");
            _currentState = IngameState.Outgame;
        }

        public async void SetReady(ReadyEnum ready)
        {
            Debug.Log($"SetReady.value = {ready}");
            //await Task.Delay(500);//LoadView�̃f�o�b�N�p
            switch (ready)
            {
                case ReadyEnum.StageReady:
                    _stageGenerateEvent.OnNext(default);
                    break;
                case ReadyEnum.PlayerReady:
                    _playerSpawnEvent.OnNext(default);
                    break;
                case ReadyEnum.EnemyReady:
                    break;
                default:
                    break;
            }
            _ingameReady.SetReady(ready);
        }

        //�v���C���[���E�o�����ۂ̏���
        public void Escape()
        {
            Debug.Log("�E�o���܂���");
            _resultEvent.OnNext(default);
        }

        //�v���C���[���E�o�A�C�e������肵���ۂ̏���
        public void GetEscapeItem()
        {
            _getEscapeItemCount.Value++;
            _escapeItemEvent.OnNext(_getEscapeItemCount.Value);
            if (_getEscapeItemCount.Value >= _escapeItemCount)
            {
                _openEscapePointEvent.OnNext(default);
            }
        }

        public void GetJournalItem()
        {
            _getJournal.OnNext(default);
        }
    }
}