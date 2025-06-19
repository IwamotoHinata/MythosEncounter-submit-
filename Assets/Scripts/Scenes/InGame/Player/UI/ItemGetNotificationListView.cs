using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGetNotificationListView : MonoBehaviour
{
    [SerializeField] private List<ItemGetNotificationView> _itemGetNotificationViews = new List<ItemGetNotificationView>();
    private int latestViewId = 0;
    
#if UNITY_EDITOR
    //デバック用
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            PopUp(null);
        }
    }
#endif

    public void Init()
    {
        foreach (var view in _itemGetNotificationViews)
        {
            view.Init();
        }
    }
    public async UniTaskVoid PopUp(ItemData item)
    {
        bool reservation = true;
        for (int i = 0; i < _itemGetNotificationViews.Count; i++) // 非表示のviewがある場合は非表示のviewを使う
        {
            if (!_itemGetNotificationViews[i].isSequence)
            {
                _itemGetNotificationViews[i].PopUp(item).Forget();
                latestViewId = i;
                reservation = false;
                break;
            }
        }
        if(reservation) // すべて使っている場合は、一番古いviewを再利用する
        {
            int index = (latestViewId + 1) % _itemGetNotificationViews.Count;//最後の次のviewのIndexを取得する
            _itemGetNotificationViews[index].PopUp(item).Forget();
            latestViewId = index;
            await UniTask.WaitUntil(() => _itemGetNotificationViews[index].isfFdeSequence);
        }
        for (int i = 0; i < _itemGetNotificationViews.Count; i++)
        {
            if(i == latestViewId)
            {
                continue;
            }
            _itemGetNotificationViews[i].DownPanel((latestViewId + i) % _itemGetNotificationViews.Count);
        }
    }
}
