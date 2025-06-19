using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class KeyConfig : MonoBehaviour
{
    [SerializeField]
    Button _changePageButton;

    [SerializeField]
    InputActionReference _actionRef;    //ここをListに変えるなどして複数のアクションを管理できるようにする
    [SerializeField]
    string _scheme = "Keyboard";

    [SerializeField]
    TMP_Text _pathText;
    [SerializeField]
    GameObject _mask;

    InputAction _action;
    InputActionRebindingExtensions.RebindingOperation _rebindOperation;
    void Awake()
    {
        if (_actionRef == null)
            return;

    }

    //リバインド開始
    public void StartRebinding(string name) //各キーに対応するボタンに設定する必要があるため、地道に設定するか、処理を見直すか
    {
        if (_action == null)
            return;

        //前回のオペレーションが実行されていたらCancelする
        _rebindOperation?.Cancel();

        //リバインド前にアクションを無効化する必要がある
        _action.Disable();

        //リバインド対象のBindingIndexを取得
        var bindingIndex = _action.GetBindingIndex(InputBinding.MaskByGroup(_scheme));

        //ブロッキング用マスクを表示
        if (_mask != null)
            _mask.SetActive(true);

        void OnFinished()
        {
            CleanUpOperation();

            _action.Enable();

            if (_mask != null)
                _mask.SetActive(false);
        }

        //リバインドオペレーションを作成し、各種コールバックの設定を実施する
        _rebindOperation = _action
            .PerformInteractiveRebinding(bindingIndex)
            .OnComplete(_ =>
            {
                RefreshDisplay();
                OnFinished();
            })
            .OnCancel(_ =>
            {
                OnFinished();
            })
            .Start();
    }

    void RefreshDisplay()
    {
        if (_action == null || _pathText == null)
            return;

        _pathText.text = _action.GetBindingDisplayString();
    }

    void OnDestroy()
    {
        CleanUpOperation();    
    }

    void CleanUpOperation()
    {
        _rebindOperation?.Dispose();
        _rebindOperation = null;
    }

}
