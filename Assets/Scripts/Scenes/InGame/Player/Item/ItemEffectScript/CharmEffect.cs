using UniRx;
using System.Linq;

namespace Scenes.Ingame.Player
{
    public class CharmEffect : ItemEffect
    {
        public override void OnPickUp()
        {
            ownerPlayerStatus.HaveCharm(true);
        }

        public override void OnThrow()
        {
            //　アイテムスロット内にまだお守りが残っているかどうか判定
            var _isHaveCharm = ownerPlayerItem.ItemSlots
                .Where((item, i) => i != ownerPlayerItem.nowIndex && item.myItemData != null)
                .Any(item => item.myItemData.itemID == 6);

            //お守りが残っていなければ効果消滅
            if (!_isHaveCharm) {
                ownerPlayerStatus.HaveCharm(false);
            }          
        }

        public override void Effect()
        {

        }

      
    }
}

