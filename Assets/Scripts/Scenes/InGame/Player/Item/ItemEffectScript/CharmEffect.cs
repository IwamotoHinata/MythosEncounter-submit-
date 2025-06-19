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
            //�@�A�C�e���X���b�g���ɂ܂�����肪�c���Ă��邩�ǂ�������
            var _isHaveCharm = ownerPlayerItem.ItemSlots
                .Where((item, i) => i != ownerPlayerItem.nowIndex && item.myItemData != null)
                .Any(item => item.myItemData.itemID == 6);

            //����肪�c���Ă��Ȃ���Ό��ʏ���
            if (!_isHaveCharm) {
                ownerPlayerStatus.HaveCharm(false);
            }          
        }

        public override void Effect()
        {

        }

      
    }
}

