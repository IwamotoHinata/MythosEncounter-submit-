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
    InputActionReference _actionRef;    //������List�ɕς���Ȃǂ��ĕ����̃A�N�V�������Ǘ��ł���悤�ɂ���
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

    //���o�C���h�J�n
    public void StartRebinding(string name) //�e�L�[�ɑΉ�����{�^���ɐݒ肷��K�v�����邽�߁A�n���ɐݒ肷�邩�A��������������
    {
        if (_action == null)
            return;

        //�O��̃I�y���[�V���������s����Ă�����Cancel����
        _rebindOperation?.Cancel();

        //���o�C���h�O�ɃA�N�V�����𖳌�������K�v������
        _action.Disable();

        //���o�C���h�Ώۂ�BindingIndex���擾
        var bindingIndex = _action.GetBindingIndex(InputBinding.MaskByGroup(_scheme));

        //�u���b�L���O�p�}�X�N��\��
        if (_mask != null)
            _mask.SetActive(true);

        void OnFinished()
        {
            CleanUpOperation();

            _action.Enable();

            if (_mask != null)
                _mask.SetActive(false);
        }

        //���o�C���h�I�y���[�V�������쐬���A�e��R�[���o�b�N�̐ݒ�����{����
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
