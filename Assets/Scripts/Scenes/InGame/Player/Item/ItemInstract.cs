using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
// using static UnityEditor.Progress;

namespace Scenes.Ingame.Player
{
    public class ItemInstract : NetworkBehaviour, IInteractable
    {
        private bool isPickedUp = false;
        public virtual void Intract(PlayerStatus status,bool processWithConditionalBypass)
        {
            Debug.Log("AddItemIntract");
            var PlayerItem = status.gameObject.GetComponent<PlayerItem>();

            ItemSlotStruct item = new ItemSlotStruct();
            item.ChangeInfo(this.GetComponent<ItemEffect>().GetItemData(), ItemSlotStatus.available);
            if (!isPickedUp)
            {
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
                if(TryGetComponent(out NetworkObject network) && Runner != null)
                {
                    Runner.Despawn(network);
                }
                else
                {
                    Destroy(this.gameObject);
                }
            }
        }

        public string ReturnPopString()
        {
            //このスクリプトでは使わない
            return null;
        }


    }
}
