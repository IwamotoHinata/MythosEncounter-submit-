using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using System.Threading;
using Scenes.Ingame.Manager;
using Scenes.Ingame.Player;
using System;
using Fusion;
using Common.Player;

namespace Scenes.Ingame.InGameSystem
{
    public class EscapePoint : MonoBehaviour, IInteractable
    {
        IngameManager manager;
        private bool _isAnimation = false;
        private bool _isActive = false;
        CancellationTokenSource token;
        private bool viewDebugLog = true;//確認用のデバックログを表示する

        private int _escpaeItemCount = 0;
        private int _progress = 0;
        private const string CANT = "アイテムが足りません";
        private const string RITUAL = "儀式を開始";
        private const string ESCAPE = "脱出";
        private const int CASTTIME = 3;//詠唱時間
        [SerializeField] ParticleSystem _particleSystem;
        [SerializeField] Renderer _dissolveRenderer;
        void Start()
        {
            token = new CancellationTokenSource();
            manager = IngameManager.Instance;
            _isActive = false;
            _escpaeItemCount = manager.GetEscapeItemMaxCount;
            manager.OnOpenEscapePointEvent.Subscribe(_ =>
                {
                    _isActive = true;
                }).AddTo(this);
            if(viewDebugLog) _isActive = true;
        }
        private void OnDestroy()
        {
            token.Cancel();
            token.Dispose();
        }

        public async void Intract(PlayerStatus status, bool processWithConditionalBypass)
        {
            if (viewDebugLog) Debug.Log($"EscapeItem.Interact:Button = {Input.GetMouseButtonDown(1)},progress = {_escpaeItemCount >= _progress}, notAnimation = {!_isAnimation}, active = {_isActive},Current = {manager.GetEscapeItemCurrentCount}");
            if (Input.GetMouseButtonDown(1) &&
                _escpaeItemCount >= _progress &&
                !_isAnimation &&
                _isActive &&
                (manager.GetEscapeItemCurrentCount > _progress ||
                manager.GetEscapeItemCurrentCount == manager.GetEscapeItemMaxCount
                ))
            {
                status.UseEscapePoint(true, CASTTIME);
                status.ChangeSpeed();
                await Ritual(token.Token);
                status.UseEscapePoint(false);
                status.ChangeSpeed();

                //if(status.gameObject.GetComponent<NetworkObject>().HasInputAuthority)
                    SpellUnlockSystem.Instance.IncreaseEscapeTimes();
            }
        }

        async UniTask Ritual(CancellationToken token)
        {
            if (viewDebugLog) Debug.Log("StartRitual");
            _isAnimation = true;
            await UniTask.Delay(TimeSpan.FromSeconds(CASTTIME));
            if (_escpaeItemCount > _progress)
            {
                _progress++;
                _isAnimation = false;
                if (_escpaeItemCount == _progress) _particleSystem.Play();
                if (viewDebugLog) Debug.Log($"EndRitual, progress {_progress}");
            }
            else
            {
                manager.Escape();
                if (viewDebugLog) Debug.Log("Escape");
            }
            if (_dissolveRenderer.material != null && _escpaeItemCount != 0 && _progress != 0)
            {
                _dissolveRenderer.material.SetFloat("_DissolveAmount", 1f - (float)_progress / _escpaeItemCount);
            }
        }
        public string ReturnPopString()
        {
            if (manager.GetEscapeItemMaxCount == _progress)
            {
                return ESCAPE;
            }
            else if (manager.GetEscapeItemCurrentCount <= _progress)
            {
                return CANT;
            }
            else if (_escpaeItemCount > _progress)
            {
                return RITUAL;
            }
            else {
                return "ERROR";
            }
        }
    }
}