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
        public enum DisplayState //ディスプレイ制御フラグ
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
        public DisplayState _displayState = DisplayState.Close; //ディスプレイの表示状態
        private Vector3 _displayPosition = Vector3.zero; //ディスプレイのTransform
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
        /// ディスプレイに注目する
        /// </summary>
        public async void OnEnableDisplay()
        {
            //操作後一回しか通らないように制御
            if (_displayState != DisplayState.Close) return;
            else _displayState = DisplayState.CameraMove;

            //モーション前処理
            _displayCamera.enabled = true; //ディスプレイカメラ有効化
            PlayerLock(true); //プレイヤーを固定

            //処理待機
            await UniTask.WhenAll(
                CameraMove(Camera.main.transform.position, _displayPosition),
                CameraRotate(Camera.main.transform.rotation, _displayRotation)
            //_roomSettingManagerCs.BootRunner()); //Runnerの起動
            );

            //モーション後処理
            Cursor.visible = true; //カーソル有効化
            Cursor.lockState = CursorLockMode.None; //カーソル固定解除
            _mapPanels.SetActive(true); //UIを表示
            //_roomSettingManagerCs.SwitchPanel(0);

            //ディスプレイ有効化完了
            _displayState = DisplayState.Open;
        }

        /// <summary>
        /// 視点を戻す
        /// </summary>
        public async void OnDisableDisplay()
        {
            //操作後一回しか通らないように制御
            if (_displayState != DisplayState.Open) return;
            else _displayState = DisplayState.CameraMove;

            //モーション前処理
            Cursor.visible = false; //カーソル無効化
            Cursor.lockState = CursorLockMode.Locked; //カーソルの固定
            _mapPanels.SetActive(false); //ルーム設定UIを非表示
            //_roomSettingManagerCs.DiscardRunner(); //Runnerの停止

            //処理待機
            await UniTask.WhenAll(
                CameraMove(_displayPosition, Camera.main.transform.position),
                CameraRotate(_displayRotation, Camera.main.transform.rotation));

            //モーション後処理
            _displayCamera.enabled = false; //ディスプレイカメラを無効化
            PlayerLock(false); //プレイヤー固定解除

            //ディスプレイ無効化完了
            _displayState = DisplayState.Close;
        }

        /// <summary>
        /// カメラ移動
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
        /// カメラ回転
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
        /// プレイヤーの状態を変更する
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

