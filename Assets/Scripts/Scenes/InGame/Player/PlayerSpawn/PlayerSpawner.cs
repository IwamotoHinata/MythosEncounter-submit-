using Scenes.Ingame.InGameSystem;
using Scenes.Ingame.InGameSystem.UI;
using Scenes.Ingame.Manager;
using Scenes.Ingame.Stage;
using UniRx;
using UnityEngine;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// プレイヤーのスポーン関係を設定するためのスクリプト
    /// </summary>
    public class PlayerSpawner : MonoBehaviour
    {
        public static PlayerSpawner Instance;
        [Header("プレイヤーのスポーン関係")]
        [SerializeField] private GameObject[] _myPlayerPrefab;//プレイヤープレハブ

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
            //指定された人数分複製を行う
            for (int i = 0; i < _playerNum; i++)
            {
                Instantiate(_myPlayerPrefab[i], _spawnPosition, Quaternion.identity);
            }

            //PlayerUIを１つだけ生成する。
            var playerUI = Instantiate(_playerUI, Vector3.zero, Quaternion.identity);
            playerUI.transform.Find("FadeOut_InCanvas").GetComponent<FadeBlackImage>().SubscribeFadePanelEvent();//プレイヤーの死亡・蘇生時のイベントを登録


            //プレイヤーの沸きが完了したことを知らせる
            IngameManager.Instance.SetReady(ReadyEnum.PlayerReady);
        }
    }
}

