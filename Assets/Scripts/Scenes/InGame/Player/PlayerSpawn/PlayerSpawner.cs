using Scenes.Ingame.InGameSystem;
using Scenes.Ingame.InGameSystem.UI;
using Scenes.Ingame.Manager;
using Scenes.Ingame.Stage;
using UniRx;
using UnityEngine;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// �v���C���[�̃X�|�[���֌W��ݒ肷�邽�߂̃X�N���v�g
    /// </summary>
    public class PlayerSpawner : MonoBehaviour
    {
        public static PlayerSpawner Instance;
        [Header("�v���C���[�̃X�|�[���֌W")]
        [SerializeField] private GameObject[] _myPlayerPrefab;//�v���C���[�v���n�u

        [Header("UI")]
        [SerializeField] private GameObject _playerUI;

        private Vector3 _spawnPosition;
        private StageGenerator _stageGenerator;
        public readonly int _playerNum = 1;
        void Start()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this.gameObject);

            IngameManager.Instance.OnStageGenerateEvent
                .Subscribe(_ =>
                {
                    _stageGenerator = FindObjectOfType<StageGenerator>();
                    Debug.Log("PlayerSpawnSuccess");
                    _spawnPosition = _stageGenerator.spawnPosition;
                    SpawnPlayer();
                }).AddTo(this);
        }


        private void SpawnPlayer()
        {
            //�w�肳�ꂽ�l�����������s��
            for (int i = 0; i < _playerNum; i++)
            {
                Instantiate(_myPlayerPrefab[i], _spawnPosition, Quaternion.identity);
            }

            //PlayerUI���P������������B
            var playerUI = Instantiate(_playerUI, Vector3.zero, Quaternion.identity);
            playerUI.transform.Find("FadeOut_InCanvas").GetComponent<FadeBlackImage>().SubscribeFadePanelEvent();//�v���C���[�̎��S�E�h�����̃C�x���g��o�^


            //�v���C���[�̕����������������Ƃ�m�点��
            IngameManager.Instance.SetReady(ReadyEnum.PlayerReady);
        }
    }
}

