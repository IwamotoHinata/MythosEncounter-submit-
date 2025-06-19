using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Scenes.Ingame.Player;

namespace Scenes.Lobby.RoomSettingPanel
{
    public class InteractMap : MonoBehaviour
    {
        public enum DisplayState //�f�B�X�v���C����t���O
        {
            Close,
            CameraMove,
            Open,
        }

        [Header("Scene Objects")]
        [SerializeField] private Camera _displayCamera;
        [SerializeField] private GameObject _mapPanels;
        [Header("Time Setting")]
        [SerializeField] private float _motionTime = 1f;

        // private RoomSettingManager _roomSettingManagerCs;
        public DisplayState _displayState = DisplayState.Close; //�f�B�X�v���C�̕\�����
        private Vector3 _displayPosition = Vector3.zero; //�f�B�X�v���C��Transform
        private Quaternion _displayRotation = Quaternion.identity;

        private void Start()
        {
            // _roomSettingManagerCs = GetComponent<RoomSettingManager>();
            _displayPosition = _displayCamera.transform.position;
            _displayRotation = _displayCamera.transform.rotation;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                //OnEnableDisplay();
            }
        }

        /// <summary>
        /// �f�B�X�v���C�ɒ��ڂ���
        /// </summary>
        public async void OnEnableDisplay()
        {
            //������񂵂��ʂ�Ȃ��悤�ɐ���
            if (_displayState != DisplayState.Close) return;
            else _displayState = DisplayState.CameraMove;

            //���[�V�����O����
            _displayCamera.enabled = true; //�f�B�X�v���C�J�����L����
            PlayerLock(true); //�v���C���[���Œ�

            //�����ҋ@
            await UniTask.WhenAll(
                CameraMove(Camera.main.transform.position, _displayPosition),
                CameraRotate(Camera.main.transform.rotation, _displayRotation)
            //_roomSettingManagerCs.BootRunner()); //Runner�̋N��
            );

            //���[�V�����㏈��
            Cursor.visible = true; //�J�[�\���L����
            Cursor.lockState = CursorLockMode.None; //�J�[�\���Œ����
            _mapPanels.SetActive(true); //UI��\��
            //_roomSettingManagerCs.SwitchPanel(0);

            //�f�B�X�v���C�L��������
            _displayState = DisplayState.Open;
        }

        /// <summary>
        /// ���_��߂�
        /// </summary>
        public async void OnDisableDisplay()
        {
            //������񂵂��ʂ�Ȃ��悤�ɐ���
            if (_displayState != DisplayState.Open) return;
            else _displayState = DisplayState.CameraMove;

            //���[�V�����O����
            Cursor.visible = false; //�J�[�\��������
            Cursor.lockState = CursorLockMode.Locked; //�J�[�\���̌Œ�
            _mapPanels.SetActive(false); //���[���ݒ�UI���\��
            //_roomSettingManagerCs.DiscardRunner(); //Runner�̒�~

            //�����ҋ@
            await UniTask.WhenAll(
                CameraMove(_displayPosition, Camera.main.transform.position),
                CameraRotate(_displayRotation, Camera.main.transform.rotation));

            //���[�V�����㏈��
            _displayCamera.enabled = false; //�f�B�X�v���C�J�����𖳌���
            PlayerLock(false); //�v���C���[�Œ����

            //�f�B�X�v���C����������
            _displayState = DisplayState.Close;
        }

        /// <summary>
        /// �J�����ړ�
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="endPosition"></param>
        /// <returns></returns>
        private async UniTask CameraMove(Vector3 startPosition, Vector3 endPosition)
        {
            await _displayCamera.transform
                .DOMove(endPosition, _motionTime)
                .From(startPosition)
                .SetEase(Ease.InOutSine)
                .AsyncWaitForCompletion();
        }

        /// <summary>
        /// �J������]
        /// </summary>
        /// <param name="startRotation"></param>
        /// <param name="endRotation"></param>
        /// <returns></returns>
        private async UniTask CameraRotate(Quaternion startRotation, Quaternion endRotation)
        {
            await _displayCamera.transform
                .DORotate(endRotation.eulerAngles, _motionTime)
                .From(startRotation.eulerAngles)
                .SetEase(Ease.InOutSine)
                .AsyncWaitForCompletion();
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

