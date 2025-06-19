using UnityEngine;

using Fusion;
using Fusion.Addons.SimpleKCC;

public enum EInputButton
{
    UseItem,
    GetItemOrIntract,
    ThrowItem,
    UseMagicOrStopMagic,
    Sneak,
    Dash
}

public struct GameplayInput : INetworkInput
{
    public Vector2 LookRotation;
    public float MouseWheelValue;
    public Vector2 MoveDirection;
    public NetworkButtons Buttons;
}

[DefaultExecutionOrder(-10)]
public sealed class PlayerInput : NetworkBehaviour, IBeforeUpdate, IAfterTick
{
    public NetworkButtons PreviousButtons { get; set; }
    public Vector2 LookRotation => _input.LookRotation;

    static float LookSensitivity = 30f;

    GameplayInput _input;
    Vector2Accumulator _lookRotationAccumulator = new Vector2Accumulator(0.02f, true);

    //�ݒ�Ȃ�
    float scrollSense = 10;

    public override void Spawned()
    {
        //���͌����������Ă��Ȃ���Ώ������Ȃ�
        if (HasInputAuthority == false)
            return;

        //�R�[���o�b�N���w��
        var networkEvents = Runner.GetComponent<NetworkEvents>();
        networkEvents.OnInput.AddListener(OnInput);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (runner == null)
            return;

        var networkEvents = runner.GetComponent<NetworkEvents>();
        if (networkEvents != null)
            networkEvents.OnInput.RemoveListener(OnInput);
    }

    void IBeforeUpdate.BeforeUpdate()
    {
        if (HasInputAuthority == false)
            return;

        //���͂��擾
        var lookRotationDelta = new Vector2(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"));

        //���x��K��
        lookRotationDelta *= LookSensitivity / 60f;
        //���͂�~��
        _lookRotationAccumulator.Accumulate(lookRotationDelta);

        //�z�C�[���̓��͂��擾
        if (Input.GetAxis("Mouse ScrollWheel") != 0 || ItemNumberKeyDown() != 0)
        {
            //�}�E�X�z�[���݂̂̓��͎�
            if (ItemNumberKeyDown() == 0)
            {
                _input.MouseWheelValue -= Input.GetAxis("Mouse ScrollWheel") * scrollSense;
                _input.MouseWheelValue = Mathf.Clamp(_input.MouseWheelValue, 0, 6);
            }
            //�����L�[�݂̂̓��͎�
            if (Input.GetAxis("Mouse ScrollWheel") == 0)
            {
                _input.MouseWheelValue = ItemNumberKeyDown() - 49;
            }
        }
        

        var moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _input.MoveDirection = moveDirection.normalized;

        _input.Buttons.Set(EInputButton.UseItem, Input.GetMouseButtonDown(0));
        _input.Buttons.Set(EInputButton.GetItemOrIntract, Input.GetMouseButton(1));
        _input.Buttons.Set(EInputButton.ThrowItem, Input.GetKey(KeyCode.H));
        _input.Buttons.Set(EInputButton.UseMagicOrStopMagic, Input.GetKey(KeyCode.Q));

        _input.Buttons.Set(EInputButton.Sneak, Input.GetKey(KeyCode.LeftControl));
        _input.Buttons.Set(EInputButton.Dash, Input.GetKey(KeyCode.LeftShift));
    }

    void IAfterTick.AfterTick()
    {
        PreviousButtons = GetInput<GameplayInput>().GetValueOrDefault().Buttons;
    }

    void OnInput(NetworkRunner runner, NetworkInput networkInput)
    {
        _input.LookRotation = _lookRotationAccumulator.ConsumeTickAligned(runner);
        networkInput.Set(_input);
    }

    /// <summary>
    /// �����L�[�������ꂽ���̊m�F
    /// </summary>
    /// <returns></returns>
    private int ItemNumberKeyDown()
    {
        if (Input.anyKeyDown)
        {
            for (int i = 49; i <= 55; i++)//1�L�[����7�L�[�܂ł͈̔͂�����
            {
                if (Input.GetKeyDown((KeyCode)i))
                    return i;
            }
            return 0;
        }
        else return 0;
    }
}

