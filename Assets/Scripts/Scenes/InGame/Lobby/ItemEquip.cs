using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Scenes.Ingame.Player;

namespace Scenes.Lobby.RoomSettingPanel
{
    public class ItemEquip : MonoBehaviour
    {
        public enum DisplayState //�f�B�X�v���C����t���O
        {
            Close,
            CameraMove,
            Open,
        }

        [Header("Scene Objects")]
        [SerializeField] private GameObject _itemEquipPanels;

        public DisplayState _displayState = DisplayState.Close; //�f�B�X�v���C�̕\�����
        private Vector3 _displayPosition = Vector3.zero; //�f�B�X�v���C��Transform
        private Quaternion _displayRotation = Quaternion.identity;

        private void Start()
        {
            
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                //OnEnableDisplay();
            }
        }

        /// <summary>
        /// �f�B�X�v���C�ɒ��ڂ���
        /// </summary>
        public void OnEnableDisplay()
        {
            //������񂵂��ʂ�Ȃ��悤�ɐ���
            if (_displayState != DisplayState.Close) return;
            else _displayState = DisplayState.CameraMove;

            PlayerLock(true); //�v���C���[���Œ�
            Cursor.visible = true; //�J�[�\���L����
            Cursor.lockState = CursorLockMode.None; //�J�[�\���Œ����
            _itemEquipPanels.SetActive(true); //UI��\��

            //�f�B�X�v���C�L��������
            _displayState = DisplayState.Open;
        }

        /// <summary>
        /// ���_��߂�
        /// </summary>
        public void OnDisableDisplay()
        {
            //������񂵂��ʂ�Ȃ��悤�ɐ���
            if (_displayState != DisplayState.Open) return;
            else _displayState = DisplayState.CameraMove;

            Cursor.visible = false; //�J�[�\��������
            Cursor.lockState = CursorLockMode.Locked; //�J�[�\���̌Œ�
            _itemEquipPanels.SetActive(false); //UI���\��
            PlayerLock(false); //�v���C���[�Œ����

            //�f�B�X�v���C����������
            _displayState = DisplayState.Close;
        }

        /// <summary>
        /// �v���C���[�̏�Ԃ�ύX����
        /// </summary>
        /// <param name="state"></param>
        private void PlayerLock(bool state)
        {
            var playerMove = FindObjectOfType<PlayerMove>();
            playerMove.MoveControl(!state);
            playerMove.RotateControl(!state);
        }
    }
}
