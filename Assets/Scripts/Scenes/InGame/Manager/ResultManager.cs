using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using Scenes.Ingame.InGameSystem;
using System;
using Data;
using Scenes.Ingame.Player;
using System.Linq;
using Scenes.Ingame.Enemy;

namespace Scenes.Ingame.Manager
{
    public class ResultManager : MonoBehaviour
    {
        public static ResultManager Instance;
        private ResultValue _resultValue;
        private Subject<ResultValue> _result = new Subject<ResultValue>();
        public IObservable<ResultValue> OnResultValue { get { return _result; } }
        EventManager eventManager;
        private PlayerItem _playerItem = null;


        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            eventManager = EventManager.Instance;
            _resultValue = new ResultValue();

            IngameManager.Instance.OnResult
                .Subscribe(_ =>
                {
                    _resultValue.time = eventManager.GetGameTime;
                    _resultValue.level = eventManager.EnemyLevel();
                    _resultValue.getUnique = eventManager.GetUniqueItem;
                    _resultValue.firstContact = eventManager.GetContact;
                    _resultValue.totalMoney = Bonus();
                    _result.OnNext(_resultValue);
                    PlayerInformationFacade.Instance.GetMoney(Bonus());
                    PlayerInformationFacade.Instance.SetIfomationCurrentItems(GetPlayerItem());
                    PlayerInformationFacade.Instance.MetEnemy(GetEnemyStatus().EnemyId);
                }).AddTo(this);

        }

        private int Bonus()
        {
            int money = 100;
            money += (20 - _resultValue.time / 60) * 5 > 0 ? (20 - _resultValue.time / 60) * 5 : 0;
            money += 20 * _resultValue.level;
            money += _resultValue.getUnique ? 50 : 0;
            money += _resultValue.firstContact ? 100 : 0;
            money = SetRingFlag() ? (int)(money * 1.5f) : money;
            return money;
        }

        public bool SetRingFlag()
        {
            var playerItem = GetPlayerItem();
            return playerItem.ItemSlots
                .Where(item => item.myItemData != null)
                .Any(item => item.myItemData.itemID == 22);//itemID22‚Í‰¤Š¥‚Å‚·BŒø‰Ê‚Í‚Á‚Ä‚¢‚½‚çŠl“¾‚·‚é‚¨‹à‚ª1.5”{‚É‚È‚è‚Ü‚·
        }

        private PlayerItem GetPlayerItem()
        {
            if(_playerItem == null)
            {
                _playerItem = FindObjectsOfType<PlayerItem>().Where(p => p.HasInputAuthority).FirstOrDefault();
            }
            return _playerItem;
        }

        private EnemyStatus GetEnemyStatus()
        {
            return FindObjectOfType<EnemyStatus>();
        }
    }
}