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
        [SerializeField] float _judgeStiffnessLength = 5f;// �X�^���t�^���\�ȋ���
        [SerializeField] float _stiffnessTime;// �X�^������
        [SerializeField] int _bullets = 2;
        public int Bullets { get { return _bullets; } }
        private const float TILELENGTH = 5.85f;

        private ReactiveProperty<bool> _changeItemStatus = new ReactiveProperty<bool>();
        public IObservable<bool> OnChangeItemStatus { get { return _changeItemStatus; } }

        /// <summary>
        /// �E�N���b�N���ɔ��C�E�����[�h�̂ǂ��炩�����肷��֐�
        /// </summary>
        public void BurstShotgun()
        {
            RaycastHit hit;
            Physics.Raycast(_mainCameraTransform.position, _mainCameraTransform.forward, out hit, _maxBulletRange);
            if (hit.collider != null)
            {
                Debug.Log($"�����������W��{hit.point}");
                if (hit.collider.transform.root.gameObject.CompareTag("Enemy"))//hit�����̂��G�L�����ł���΃_���[�W+�X�^�����ʂ�^����
                {
                    Debug.Log("EnemyHit");
                    int damage = _bulletDamage;
                    if (hit.distance >= 15f)
                    {
                        damage -= (int)(((int)(hit.distance - 15) / 1.5f) * 2.5f);
                        Debug.Log($"�_���[�W��{damage}");
                    }

                    if (damage > 0)
                    {
                        EnemyStatus enemyStatus = hit.collider.transform.root.gameObject.GetComponent<EnemyStatus>();
                        enemyStatus.AddDamage(damage);
                        if (hit.distance <= _judgeStiffnessLength)
                        {
                            Debug.Log("�m�b�N�o�b�N���ʋN��");
                            enemyStatus.ChangeStiffnessTime(_stiffnessTime);
                        }
                    }
                }

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Floor"))
                {
                    if (hit.distance <= TILELENGTH)
                    {
                        // +�e�����c������
                    }
                }
                _animator.SetTrigger("Burst");
                _visualEffect.SendEvent("OnPlay");
                _cinemachineImpulseSource.GenerateImpulse();//�J�����̗h�ꔭ��
            }
        }

        //�e����ύX
        public void ChangeNumBullets(int num)
        {
            _bullets += num;
        }

        //�����[�h�A�j���[�V�������Đ�
        public void PlayReloadAnim()
        {
            _animator.SetTrigger("Reload");
        }

        /// <summary>
        /// �A�C�e���ύX�ۂȂǂ̃X�e�[�^�X���܂Ƃ߂ĕύX����֐��@�A�j���[�V�����C�x���g����Ăяo��
        /// </summary>
        /// <param name="value"></param> 1 �� true, 0 �� false
        public void ChangeItemStatus(int num)
        {
            if (_playerStatus == null) return;
            bool value = (num == 1);
            _playerStatus.UseItem(!value);
            _playerStatus.ChangeSpeed();
            _changeItemStatus.Value = value;
        }


        /// <summary>
        /// ���C���Ȃǂ�se���Ǘ�����֐�
        /// </summary>
        /// <param name="value"></param>��������se�𐔒l�őI������
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