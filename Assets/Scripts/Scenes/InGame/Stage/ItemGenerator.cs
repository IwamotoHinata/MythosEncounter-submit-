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
        [SerializeField, Tooltip("脱出アイテムの生成数")]
        private int _escapeItemCount = 4;
        [SerializeField, Tooltip("ステージアイテムの生成数")]
        private int _stageItemCount = 20;
        [SerializeField, Tooltip("itemのprefab")]
        private List<GameObject> _stageItemPrefab;
        [SerializeField, Tooltip("脱出アイテムのprefab")]
        private GameObject _escapeItemPrefab;
        [SerializeField, Tooltip("脱出地点のprefab")]
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
                Debug.Log("Clientなのでアイテム生成をキャンセルします");
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
                Debug.LogError("escapeMarkerの数が生成するescapeItemの数より少ないです");
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
                Debug.LogError("escapeMarkerの数がありません");
                return;
            }
            else if (_itemMarker.Count < _stageItemCount)
            {
                Debug.LogWarning("escapeMarkerの数が少ないため、生成数を調整します");
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