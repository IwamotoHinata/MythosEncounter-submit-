using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGetNotificationListView : MonoBehaviour
{
    [SerializeField] private List<ItemGetNotificationView> _itemGetNotificationViews = new List<ItemGetNotificationView>();
    private int latestViewId = 0;
    
#if UNITY_EDITOR
    //�f�o�b�N�p
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
        for (int i = 0; i < _itemGetNotificationViews.Count; i++) // ��\����view������ꍇ�͔�\����view���g��
        {
            if (!_itemGetNotificationViews[i].isSequence)
            {
                _itemGetNotificationViews[i].PopUp(item).Forget();
                latestViewId = i;
                reservation = false;
                break;
            }
        }
        if(reservation) // ���ׂĎg���Ă���ꍇ�́A��ԌÂ�view���ė��p����
        {
            int index = (latestViewId + 1) % _itemGetNotificationViews.Count;//�Ō�̎���view��Index���擾����
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
