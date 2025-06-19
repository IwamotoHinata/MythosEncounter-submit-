using UnityEngine;
using UniRx;

namespace Scenes.Ingame.Enemy
{
    public class EnemySoundManager : MonoBehaviour
    {
        [Header("自身についているメソッド")]
        [SerializeField] private EnemyStatus _enemyStatus;
        [SerializeField] private AudioSource _audioSource;

        [Header("音源")]
        [SerializeField] private AudioClip _slowClips;//歩くときの足音のClip
        [SerializeField] private AudioClip _fastClips;//走るときの足音のClip
        void Start()
        {
            _enemyStatus.OnEnemyStateChange.Subscribe(state =>
            {
                switch (state)
                {
                    case EnemyState.Patrolling:
                        _audioSource.clip = _slowClips;
                        _audioSource.Play();
                        break;
                    case EnemyState.Searching:
                        _audioSource.clip = _slowClips;
                        _audioSource.Play();
                        break;
                    case EnemyState.Chase:
                        _audioSource.clip = _fastClips;
                        _audioSource.Play();
                        break;
                    case EnemyState.Attack:
                        _audioSource.clip = _fastClips;
                        _audioSource.Play();
                        break;
                }


                _audioSource.PlayOneShot(_slowClips);
            }).AddTo(this);
        }
        public void AttackSound(AnimationEvent animationEvent)
        {
            Debug.Log("AttackSound");
            SoundManager.Instance.PlaySe(animationEvent.stringParameter, transform.position);
        }
    }
}
