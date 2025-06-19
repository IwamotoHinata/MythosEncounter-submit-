using Cinemachine;
using Scenes.Ingame.Enemy;
using UnityEngine;
using UnityEngine.VFX;
using UniRx;
using System;



namespace Scenes.Ingame.Player
{
    public class ShotgunBurst : MonoBehaviour
    {
        [SerializeField] PlayerStatus _playerStatus;
        [SerializeField] Transform _mainCameraTransform;
        [SerializeField] Animator _animator;
        [SerializeField] VisualEffect _visualEffect;
        [SerializeField] CinemachineImpulseSource _cinemachineImpulseSource;
        [SerializeField] int _bulletDamage = 25;
        [SerializeField] float _maxBulletRange = 30f;
        [SerializeField] float _judgeStiffnessLength = 5f;// スタン付与が可能な距離
        [SerializeField] float _stiffnessTime;// スタン時間
        [SerializeField] int _bullets = 2;
        public int Bullets { get { return _bullets; } }
        private const float TILELENGTH = 5.85f;

        private ReactiveProperty<bool> _changeItemStatus = new ReactiveProperty<bool>();
        public IObservable<bool> OnChangeItemStatus { get { return _changeItemStatus; } }

        /// <summary>
        /// 右クリック時に発砲・リロードのどちらかを決定する関数
        /// </summary>
        public void BurstShotgun()
        {
            RaycastHit hit;
            Physics.Raycast(_mainCameraTransform.position, _mainCameraTransform.forward, out hit, _maxBulletRange);
            if (hit.collider != null)
            {
                Debug.Log($"当たった座標は{hit.point}");
                if (hit.collider.transform.root.gameObject.CompareTag("Enemy"))//hitしたのが敵キャラであればダメージ+スタン効果を与える
                {
                    Debug.Log("EnemyHit");
                    int damage = _bulletDamage;
                    if (hit.distance >= 15f)
                    {
                        damage -= (int)(((int)(hit.distance - 15) / 1.5f) * 2.5f);
                        Debug.Log($"ダメージは{damage}");
                    }

                    if (damage > 0)
                    {
                        EnemyStatus enemyStatus = hit.collider.transform.root.gameObject.GetComponent<EnemyStatus>();
                        enemyStatus.AddDamage(damage);
                        if (hit.distance <= _judgeStiffnessLength)
                        {
                            Debug.Log("ノックバック効果起動");
                            enemyStatus.ChangeStiffnessTime(_stiffnessTime);
                        }
                    }
                }

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Floor"))
                {
                    if (hit.distance <= TILELENGTH)
                    {
                        // +弾痕を残す処理
                    }
                }
                _animator.SetTrigger("Burst");
                _visualEffect.SendEvent("OnPlay");
                _cinemachineImpulseSource.GenerateImpulse();//カメラの揺れ発生
            }
        }

        //弾数を変更
        public void ChangeNumBullets(int num)
        {
            _bullets += num;
        }

        //リロードアニメーションを再生
        public void PlayReloadAnim()
        {
            _animator.SetTrigger("Reload");
        }

        /// <summary>
        /// アイテム変更可否などのステータスをまとめて変更する関数　アニメーションイベントから呼び出す
        /// </summary>
        /// <param name="value"></param> 1 → true, 0 → false
        public void ChangeItemStatus(int num)
        {
            if (_playerStatus == null) return;
            bool value = (num == 1);
            _playerStatus.UseItem(!value);
            _playerStatus.ChangeSpeed();
            _changeItemStatus.Value = value;
        }


        /// <summary>
        /// 発砲音などのseを管理する関数
        /// </summary>
        /// <param name="value"></param>流したいseを数値で選択する
        public void PlayGunSe(int value)
        {
            switch (value)
            {
                case 0:
                    SoundManager.Instance.PlaySe("se_gunBurst01", _playerStatus.gameObject.transform.position);
                    break;
                case 1:
                    SoundManager.Instance.PlaySe("se_guncock00", _playerStatus.gameObject.transform.position);
                    break;
                case 2:
                    SoundManager.Instance.PlaySe("se_guncock01", _playerStatus.gameObject.transform.position);
                    break;
            }
        }
        public void Init(PlayerStatus myStatus)
        {
            if (myStatus == null)
            {
                throw new ArgumentNullException("myPlayer or PlayerStatus", "These parameters cannot be null");
            }

            _playerStatus = myStatus;
        }

    }
}