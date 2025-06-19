using UnityEngine;
namespace Scenes.Ingame.Player
{
    /// <summary>
    /// アイテムの効果・データを管理する為の抽象クラス
    /// 子クラスの名前は「アイテム名 + Effect」とすること
    /// </summary>
    [RequireComponent(typeof(ItemInstract))]
    public abstract class ItemEffect : MonoBehaviour
    {
        public ItemData myItemData;
        [HideInInspector]public PlayerStatus ownerPlayerStatus;
        [HideInInspector] public PlayerItem ownerPlayerItem;

        public ItemData GetItemData()
        {
            return myItemData;
        }

        /// <summary>
        /// 拾われたときに実行する処理を記述
        /// </summary>
        public abstract void OnPickUp();

        /// <summary>
        /// アイテムを捨てるときに実行される処理を記述
        /// </summary>
        public abstract void OnThrow();

        /// <summary>
        /// アイテムの効果を実装する関数
        /// アイテムを使い終わったら「ownerPlayerItem.ThrowItem(ownerPlayerItem.nowIndex);」を記述すること
        /// </summary>
        public abstract void Effect();
    }
}

