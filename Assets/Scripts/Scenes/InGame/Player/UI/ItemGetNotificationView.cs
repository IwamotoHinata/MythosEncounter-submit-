using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class ItemGetNotificationView : MonoBehaviour
{
    [SerializeField] private Image _backImage;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _itemName;
    [SerializeField] private TextMeshProUGUI _description;
    [SerializeField] private const float ANIMETIONTIME = 0.5f;
    [SerializeField] private const float DISPLAYTIME = 5f;
    [SerializeField] private const float FIDETIME = 1f;
    public bool isSequence { get {  return popSequence.IsPlaying(); } }
    public bool isfFdeSequence { get { return fideSequence.IsPlaying(); } }
    private const float MOVEDISTANCE = 70f;
    private Sequence popSequence; //Sequence¶¬
    private Sequence fideSequence; //Sequence¶¬

    public void Init()
    {
        _icon.DOFade(0, 0);
        _backImage.DOFade(0, 0);
        _itemName.DOFade(0, 0);
        _description.DOFade(0, 0);

        popSequence = DOTween.Sequence()
            .Pause()
            .SetAutoKill(false)
            .SetLink(gameObject);
        popSequence.Append(transform.DOLocalMoveX(0, 0))
        .Append(transform.DOLocalMoveY(0, ANIMETIONTIME).From(MOVEDISTANCE))
        .Join(_icon.DOFade(1, ANIMETIONTIME).From(0))
        .Join(_backImage.DOFade(1, ANIMETIONTIME).From(0))
        .Join(_itemName.DOFade(1, ANIMETIONTIME).From(0))
        .Join(_description.DOFade(1, ANIMETIONTIME).From(0))
        .AppendInterval(DISPLAYTIME);

        popSequence.OnComplete(() => {
            fideSequence.Restart();
        });

        fideSequence = DOTween.Sequence()
            .Pause()
            .SetAutoKill(false)
            .SetLink(gameObject);
        fideSequence.Append(transform.DOLocalMoveX(500, FIDETIME))
        .Join(_icon.DOFade(0, FIDETIME))
        .Join(_backImage.DOFade(0, FIDETIME))
        .Join(_itemName.DOFade(0, FIDETIME))
        .Join(_description.DOFade(0, FIDETIME));
    }
    public async UniTaskVoid PopUp(ItemData item)
    {
        if (item != null)
        {
            _icon.sprite = item.itemSprite;
            _itemName.text = item.itemName;
        }

        if (popSequence.IsPlaying())
        {
            if (popSequence.IsPlaying())
            {
                popSequence.Complete();
            }
            await UniTask.WaitUntil(() => !fideSequence.IsPlaying());
            popSequence.Restart();
        }
        else
        {
            popSequence.Restart();
        }
    }

    public void DownPanel(int count)
    {
        transform.DOLocalMoveY(-150 * count, ANIMETIONTIME);
    }
}
