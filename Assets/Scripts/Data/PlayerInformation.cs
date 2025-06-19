using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Data
{
    public class PlayerInformation : MonoBehaviour
    {
        private int characterId = 1;
        private string _name;//キャラクター名
        private DateTime _created;//作成日時
        private DateTime _end;//死亡日時
        private int _money;//所持金
        private Dictionary<int, int> _items = new Dictionary<int, int>();//アイテム
        private Dictionary<int, bool> _spell = new Dictionary<int, bool>();//習得した呪文
        private Dictionary<int , int> _mythCreature = new Dictionary<int, int>();//遭遇した敵
        private int _escape;//退散させずに脱出した回数
        private int _dispersingEscape;//退散して脱出した回数
        public static PlayerInformation Instance;
        private int[] _currentItems = new int[7];//ゲーム前に選択したアイテム
        private int _currentSpell;//ゲーム前に選択したアイテム

        public int CharacterId { get => characterId; }
        public string Name { get => _name; }
        public DateTime Created { get => _created; }
        public DateTime End { get => _end; }
        public int Money { get => _money; }
        public Dictionary<int, int> Items { get => _items; }
        public Dictionary<int, bool> Spell { get => _spell; }
        public Dictionary<int, int> MythCreature { get => _mythCreature; }
        public int Escape { get => _escape; }
        public int DispersingEscape { get => _dispersingEscape; }
        public int[] CurrentItems { get => _currentItems; }//ゲーム前に選択したアイテム
        public int CurrentSpell { get => _currentSpell; }//ゲーム前に選択したスペル

        public void SpellUnlock(int i)
        {
            _spell[i] = true;
        }

        void Awake()
        {
            Instance = this;
        }

        async void Start()
        {
            DecodeData().Forget();
        }

        public void SetCurrentItem(int[] currentItems)
        {
            _currentItems = currentItems;
        }

        public void SetCurrentSpell(int spellId)
        {
            _currentSpell = spellId;
        }

        public void MetEnemy(int enemyId)
        {
            _mythCreature[enemyId]++;
        }

        public void GetItem(int itemId, int count)
        {
            _items[itemId] += count;
        }

        public void GetMoney(int money)
        {
            _money += money;
        }

        private async UniTaskVoid DecodeData()
        {
            await UniTask.WaitUntil(() => WebDataRequest.OnCompleteLoadData);
            await Task.Delay(100);
            int itemSize = WebDataRequest.GetItemDataArrayList.Count;//?A?C?e??????
            int spellSize = WebDataRequest.GetSpellDataArrayList.Count;//?G????
            int mythCreatureSize = WebDataRequest.GetEnemyDataArrayList.Count;//?G????

            for (int i = 0; i < itemSize; i++)
            {
                _items[i] = 0;
            }
            for (int i = 0; i < spellSize; i++)
            {
                _spell[i] = false;
            }
            for (int i = 0; i < mythCreatureSize; i++)
            {
                _mythCreature[i] = 0;
            }
            var data = WebDataRequest.GetPlayerDataArrayList.Where(data => data.Id == characterId).FirstOrDefault();
            for (int i = 0; i < data.Spell.Length; i++)
            {
                _spell[int.Parse(data.Spell[i])] =  true;
            }
            for (int i = 0; i < data.Items.Length; i++)
            {
                var decode = data.Items[i].Split('=');
                if (decode.Length > 2) Debug.LogError("Failed decode item data");
                _items[int.Parse(decode[0])] = int.Parse(decode[1]);
            }
            for (int i = 0; i < data.MythCreature.Length; i++)
            {
                var decode = data.MythCreature[i].Split('=');
                if (decode.Length > 2) Debug.LogError($"Failed decode mythCreature data. {data.MythCreature[i]}");
                _mythCreature[int.Parse(decode[0])] = int.Parse(decode[1]);
            }
            Debug.Log("end");
        }
    }
}