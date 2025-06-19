using Scenes.Ingame.Journal;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

public class JournalView : ViewBase
{
    [SerializeField] private Animator _animator;

    [Header("View")]
    [SerializeField] private ViewBase _progressView;
    [SerializeField] private ViewBase _featureView;
    [SerializeField] private ViewBase _compatibilityView;
    [SerializeField] private ViewBase _enemyView;
    [SerializeField] private ViewBase _itemView;
    [SerializeField] private ViewBase _spellView;

    [Header("Tag")]
    [SerializeField] private Button _progressTag;
    [SerializeField] private Button _featureTag;
    [SerializeField] private Button _compatibilityTag;
    [SerializeField] private Button _enemyTag;
    [SerializeField] private Button _itemTag;
    [SerializeField] private Button _spellTag;

    private Subject<PageType> _jornalTagClick = new Subject<PageType>();
    public IObservable<PageType> OnJornalTagClick { get { return _jornalTagClick; } }
    private ViewBase _pastView;

    public override void Init()
    {
        _progressView.Init();
        _featureView.Init();
        _compatibilityView.Init();
        _enemyView.Init();
        _itemView.Init();
        _spellView.Init();

        _progressTag.OnClickAsObservable().Subscribe(_ => PageChange(PageType.Progress)).AddTo(this);
        _featureTag.OnClickAsObservable().Subscribe(_ => PageChange(PageType.Feature)).AddTo(this);
        _compatibilityTag.OnClickAsObservable().Subscribe(_ => PageChange(PageType.Compatibility)).AddTo(this);
        _enemyTag.OnClickAsObservable().Subscribe(_ => PageChange(PageType.Enemy)).AddTo(this);
        _itemTag.OnClickAsObservable().Subscribe(_ => PageChange(PageType.Item)).AddTo(this);
        _spellTag.OnClickAsObservable().Subscribe(_ => PageChange(PageType.Spell)).AddTo(this);
    }
    public void PageChange(PageType pageType)
    {
        _animator.SetTrigger("isNext");
        _pastView?.Close();
        switch (pageType)
        {
            case PageType.Progress:
                _progressView.Open();
                _pastView = _progressView;
                break;
            case PageType.Feature:
                _featureView.Open();
                _pastView = _featureView;
                break;
            case PageType.Compatibility:
                _compatibilityView.Open();
                _pastView = _compatibilityView;
                break;
            case PageType.Enemy:
                _enemyView.Open();
                _pastView = _enemyView;
                break;
            case PageType.Item:
                _itemView.Open();
                _pastView = _itemView;
                break;
            case PageType.Spell:
                _spellView.Open();
                _pastView = _spellView;
                break;
        }
    }
    public override void Open()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _animator.SetTrigger("isOpen");
    }
    public override void Close()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _animator.SetTrigger("isClose");
    }
}
