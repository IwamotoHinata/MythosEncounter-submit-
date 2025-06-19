using Scenes.Ingame.Manager;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;
using Fusion;
using Cysharp.Threading.Tasks;

namespace Scenes.Ingame.Stage
{
    public class ItemGenerator : NetworkBehaviour
    {
        [SerializeField, Tooltip("�E�o�A�C�e���̐�����")]
        private int _escapeItemCount = 4;
        [SerializeField, Tooltip("�X�e�[�W�A�C�e���̐�����")]
        private int _stageItemCount = 20;
        [SerializeField, Tooltip("item��prefab")]
        private List<GameObject> _stageItemPrefab;
        [SerializeField, Tooltip("�E�o�A�C�e����prefab")]
        private GameObject _escapeItemPrefab;
        [SerializeField, Tooltip("�E�o�n�_��prefab")]
        private GameObject _escapePointPrefab;
        private List<GameObject> _itemMarker;
        private GameObject[] _escapePointMarker;
        void Start()
        {
            IngameManager.Instance.OnStageGenerateEvent
                .Subscribe(_ =>
                {
                    ItemGenerateEvent().Forget();
                }).AddTo(this);
        }

        private async UniTaskVoid ItemGenerateEvent()
        {
            await UniTask.WaitUntil(() => Runner != null);
            if (Runner.IsClient)
            {
                Debug.Log("Client�Ȃ̂ŃA�C�e���������L�����Z�����܂�");
                return;
            }
            _itemMarker = GameObject.FindGameObjectsWithTag("ItemSpawnPoint").ToList();
            _escapePointMarker = GameObject.FindGameObjectsWithTag("EscapePoint");
            InstatiateEscapePoint();
            InstatiateEscapeItem();
            RandomStageItemSet();
        }

        private void InstatiateEscapeItem()
        {
            if (_itemMarker.Count < _escapeItemCount)
            {
                Debug.LogError("escapeMarker�̐�����������escapeItem�̐���菭�Ȃ��ł�");
                return;
            }
            for (int i = 0; i < _escapeItemCount; i++)
            {
                int randomNumber = Random.Range(0, _itemMarker.Count);
                Instantiate(_escapeItemPrefab, _itemMarker[randomNumber].transform.position, Quaternion.identity);
                DeleteList(_itemMarker[randomNumber]);
            }
        }
        private void InstatiateEscapePoint()
        {
            var targetPoint = _escapePointMarker[Random.Range(0, _escapePointMarker.Length)];
            Instantiate(_escapePointPrefab, targetPoint.transform.position, targetPoint.transform.rotation);
            foreach (var item in _escapePointMarker)
            {
                Destroy(item);
            }
        }
        private void RandomStageItemSet()
        {
            Debug.Log($"_itemMarker = {_itemMarker.Count}");
            if (_itemMarker.Count < 1)
            {
                Debug.LogError("escapeMarker�̐�������܂���");
                return;
            }
            else if (_itemMarker.Count < _stageItemCount)
            {
                Debug.LogWarning("escapeMarker�̐������Ȃ����߁A�������𒲐����܂�");
                _stageItemCount = _itemMarker.Count;
            }
            for (int i = 0; i < _stageItemCount; i++)
            {
                int randomNumber = Random.Range(0, _itemMarker.Count);
                int itemRandomNumber = Random.Range(0, _stageItemPrefab.Count);
                Runner.Spawn(_stageItemPrefab[itemRandomNumber], _itemMarker[randomNumber].transform.position, Quaternion.identity);
                //Instantiate(_stageItemPrefab[itemRandomNumber], _itemMarker[randomNumber].transform.position, Quaternion.identity);
                DeleteList(_itemMarker[randomNumber]);
            }
            ClearList();
        }
        private void ClearList()
        {
            List<GameObject> _clearList = _itemMarker;
            foreach (GameObject item in _clearList)
            {
                Destroy(item);
            }
            _itemMarker.Clear();
        }
        private void DeleteList(GameObject target)
        {
            _itemMarker.Remove(target);
            Destroy(target);
        }
    }
}