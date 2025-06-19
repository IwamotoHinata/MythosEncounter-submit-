using Scenes.Ingame.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Data
{
    public class PlayerInformationFacade : MonoBehaviour
    {
        public enum EscapeRequestType
        {
            Escape,                     //脱出の数
            DispersingEscape,           //退散させて脱出した数
            EscapeAndDispersingEscape,  //合計の脱出数
        }

        public enum ItemRequestType
        {
            All,        //すべてのアイテム所持情報
            Owned,      //持っているアイテムだけの所持情報
            NotOwned    //持っていないアイテムの所持情報
        }
        public enum spellRequestType
        {
            All,        //すべての呪文所持情報
            Owned,      //持っている呪文だけの所持情報
            NotOwned    //持っていない呪文の所持情報
        }
        public enum EnemyRequestType
        {
            All,        //すべての敵との遭遇数
            Met,        //会ったことがある敵との遭遇数
            NotMet      //会ったことがないある敵との遭遇数＝０
        }

        public static PlayerInformationFacade Instance;
        PlayerInformation playerInformation;

        private void Awake()
        {
            Instance = this;
        }
        private void Start()
        {
            playerInformation = PlayerInformation.Instance;
        }

        /// <summary>
        /// 脱出数についての参照
        /// </summary>
        public int GetEscapeCount(EscapeRequestType type = EscapeRequestType.EscapeAndDispersingEscape)
        {
            switch (type)
            {
                case EscapeRequestType.Escape:
                    return playerInformation.Escape;
                case EscapeRequestType.DispersingEscape:
                    return playerInformation.DispersingEscape;
                case EscapeRequestType.EscapeAndDispersingEscape:
                    return playerInformation.Escape + playerInformation.DispersingEscape;
                default:
                    Debug.LogError("引数が正確ではありません");
                    return 0;
            }
        }

        /// <summary>
        /// 遭遇した敵が初めてか
        /// </summary>
        public bool IsFarstContactEnemy(int id)
        {
            return playerInformation.MythCreature.Where(x => x.Key == id && x.Value == 0).Any();
        }

        /// <summary>
        /// アイテムついての参照
        /// </summary>
        public Dictionary<int, int> GetEnemy(EnemyRequestType type = EnemyRequestType.All)
        {
            switch (type)
            {
                case EnemyRequestType.All:
                    return playerInformation.MythCreature;
                case EnemyRequestType.Met:
                    return playerInformation.MythCreature.Where(x => x.Value > 0).ToDictionary(x => x.Key, x => x.Value);
                case EnemyRequestType.NotMet:
                    return playerInformation.MythCreature.Where(x => x.Value == 0).ToDictionary(x => x.Key, x => x.Value);
                default:
                    Debug.LogError("引数が正確ではありません");
                    return new Dictionary<int, int>();
            }
        }

        /// <summary>
        /// アイテムついての参照
        /// </summary>
        public Dictionary<int, int> GetItem(ItemRequestType type = ItemRequestType.All)
        {
            switch (type)
            {
                case ItemRequestType.All:
                    return playerInformation.Items;
                case ItemRequestType.Owned:
                    return playerInformation.Items.Where(x => x.Value > 0).ToDictionary(x => x.Key, x => x.Value);
                case ItemRequestType.NotOwned:
                    return playerInformation.Items.Where(x => x.Value == 0).ToDictionary(x => x.Key, x => x.Value);
                default:
                    Debug.LogError("引数が正確ではありません");
                    return new Dictionary<int, int>();
            }
        }

        /// <summary>
        /// アイテムついての参照
        /// </summary>
        public Dictionary<int, SpellStruct> GetSpell(spellRequestType type = spellRequestType.All)
        {
            switch (type)
            {
                case spellRequestType.All:
                    return playerInformation.Spell.ToDictionary(x => x.Key, x => WebDataRequest.GetSpellDataArrayList[x.Key]);
                case spellRequestType.Owned:
                    return playerInformation.Spell.Where(x => x.Value == true).ToDictionary(x => x.Key, x => WebDataRequest.GetSpellDataArrayList[x.Key]);
                case spellRequestType.NotOwned:
                    return playerInformation.Spell.Where(x => x.Value == false).ToDictionary(x => x.Key, x => WebDataRequest.GetSpellDataArrayList[x.Key]);
                default:
                    Debug.LogError("引数が正確ではありません");
                    return null;
            }
        }

        public void UnLockSpell(int spellId)
        {
            playerInformation.SpellUnlock(spellId);
        }

        /// <summary>
        /// アイテムをプレイヤーの所持情報に追加する
        /// </summary>
        public void GetItem(int itemId, int count)
        {
            playerInformation.GetItem(itemId, count);
        }

        /// <summary>
        /// 現在持っているアイテムを所持情報から無くす
        /// ゲーム開始時にすべて無くすための処理
        /// </summary>
        public void LostCurrentItems(PlayerItem userItem)
        {
            foreach (var item in userItem.ItemSlots)
            {
                LostItem(item.myItemData.itemID, 1);
            }
        }

        /// <summary>
        /// 現在持っているアイテムを所持情報にすべて追加する
        /// ゲーム終了時に持っているものを倉庫に入れるための処理
        /// </summary>
        public void SetIfomationCurrentItems(PlayerItem userItem)
        {
            foreach (var item in userItem.ItemSlots)
            {
                playerInformation.GetItem(item.myItemData.itemID, 1);
            }
        }

        /// <summary>
        /// ロビーからインゲームにで使用するスペルのIDを設定する時などに使う関数
        /// </summary>
        public void SetCurrentSpell(int spellId)
        {
            playerInformation.SetCurrentSpell(spellId);
        }

        /// <summary>
        /// ロビーからインゲームに持ち込むアイテムを設定する時などに使う関数
        /// </summary>
        public void SetCurrentitem(int?[] items)
        {
            //配列の数が7出ない場合は整形する
            int[] setItems;
            if (items.Length == 7)
            {
                setItems = items.Select(x => x ?? 0).ToArray();
            }
            else
            {
                setItems = Enumerable.Range(0, 7)
                .Select(i => i < items.Length ? (items[i] ?? 0) : 0)
                    .ToArray();
            }
            playerInformation.SetCurrentItem(setItems);
        }

        /// <summary>
        /// プレイヤーがセットしたアイテムを取得する関数
        /// </summary>
        public int[] GetCurrentitem()
        {
            return playerInformation.CurrentItems;
        }

        /// <summary>
        /// プレイヤーがセットしたスペルのIDを取得する関数
        /// </summary>
        public int GetSpellId()
        {
            return playerInformation.CurrentSpell;
        }

        public void LostItem(int itemId, int count)
        {
            int lostCount = 0;
            if (playerInformation.Items[itemId] > count)
            {
                lostCount = count;
            }
            else
            {
                lostCount = playerInformation.Items[itemId];
            }
            playerInformation.GetItem(itemId, -lostCount);
        }

        public void MetEnemy(int enemyId)
        {
            playerInformation.MetEnemy(enemyId);
        }

        public void GetMoney(int money)
        {
            playerInformation.GetMoney(money);
        }
        
        /// <summary>
        /// データをデータベースに上げる関数
        /// </summary>
        public void SendPlayerInformation()
        {
            PlayerDataStruct sendData = new PlayerDataStruct();
            var items = playerInformation.Items
                .Where(kv => kv.Value > 0)
                .GroupBy(kv => kv.Key)
                .Select(g => $"{g.Key}={g.Sum(kv => kv.Value)}")
                .ToArray();
            var encount = playerInformation.MythCreature
                .Where(kv => kv.Value > 0)
                .GroupBy(kv => kv.Key)
                .Select(g => $"{g.Key}={g.Sum(kv => kv.Value)}")
                .ToArray();
            var spells = playerInformation.Spell
                .Where(kv => kv.Value)
                .GroupBy(kv => kv.Key)
                .Select(g => $"{g.Key}")
                .ToArray();
            sendData.PlayerDataSet(playerInformation.CharacterId, playerInformation.name, playerInformation.Created, playerInformation.End, playerInformation.Money, items, encount, spells, GetEscapeCount(EscapeRequestType.EscapeAndDispersingEscape));
            WebDataRequest.PutPlayerData(sendData).Forget();
        }
    }
}