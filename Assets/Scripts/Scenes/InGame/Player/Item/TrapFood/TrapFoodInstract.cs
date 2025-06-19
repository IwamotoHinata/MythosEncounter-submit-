using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Ingame.Player
{
    public class TrapFoodInstract : ItemInstract
    {
        [SerializeField] private GameObject _trapFood;
        public override void Intract(PlayerStatus status, bool processWithConditionalBypass)
        {
            var PlayerItem = status.gameObject.GetComponent<PlayerItem>();
            bool isPickedUp = false;

            ItemSlotStruct item = new ItemSlotStruct();
            item.ChangeInfo(_trapFood.GetComponent<ItemEffect>().GetItemData(), ItemSlotStatus.available);

            for (int i = 0; i < 7; i++)
            {
                int index = PlayerItem.nowIndex + i;
                if (index > 6)
                    index -= 7;

                if (PlayerItem.ItemSlots[index].myItemData == null && PlayerItem.ItemSlots[index].myItemSlotStatus == ItemSlotStatus.available)
                {
                    PlayerItem.ChangeListValue(index, item);//アイテムスロットにアイテムを格納
                    isPickedUp = true;
                    break;
                }
                else
                    continue;
            }

            //このアイテムが拾えなかったら終了
            if (!isPickedUp)
                return;

            if (PlayerItem.nowBringItem == null)
            {
                PlayerItem.SettingBringItem(this.Object);
            }
            else
            {
                if (this.gameObject.GetComponent<NetworkObject>())
                    Runner.Despawn(this.gameObject.GetComponent<NetworkObject>());
                else
                    Destroy(this.gameObject);
            }
        }
    }
}
