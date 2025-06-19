using UnityEngine;
using UniRx;

namespace Scenes.Ingame.Player
{
    public class ShotgunEffect : ItemEffect
    {
        [SerializeField] private int _bullets = 2;
        private ShotgunBurst _shotgunBurst;
        public override void OnPickUp()
        {
            _shotgunBurst = ownerPlayerItem._shotgunBurst;
            _shotgunBurst.gameObject.SetActive(true);
            //選択アイテムを別のものにしたとき、自動でプレビューを削除する
            ownerPlayerItem.OnNowIndexChange
                .Skip(1)
                .Where(_ => ownerPlayerItem.ItemSlots[ownerPlayerItem.nowIndex].myItemData == null || ownerPlayerItem.ItemSlots[ownerPlayerItem.nowIndex].myItemData.itemID != 12)
                .Subscribe(_ =>
                {
                    _shotgunBurst.gameObject.SetActive(false);
                }).AddTo(this);
        }

        public override void OnThrow()
        {
            _shotgunBurst.gameObject.SetActive(false);
        }

        public override void Effect()
        {
            if(_shotgunBurst.Bullets > 0 )
            {
                _shotgunBurst.BurstShotgun();
                _shotgunBurst.ChangeNumBullets(-1);
                Debug.Log("弾数1減らす");
            }
            else
            {
                _shotgunBurst.PlayReloadAnim();
                _shotgunBurst.ChangeNumBullets(2);
            }
        }
    }
}
