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
        public enum DisplayState //ディスプレイ制御フラグ
        {
            Close,
            CameraMove,
            Open,
        }

        [Header("Scene Objects")]
        [SerializeField] private GameObject _itemEquipPanels;

        public DisplayState _displayState = DisplayState.Close; //ディスプレイの表示状態
        private Vector3 _displayPosition = Vector3.zero; //ディスプレイのTransform
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
        /// ディスプレイに注目する
        /// </summary>
        public void OnEnableDisplay()
        {
            //操作後一回しか通らないように制御
            if (_displayState != DisplayState.Close) return;
            else _displayState = DisplayState.CameraMove;

            PlayerLock(true); //プレイヤーを固定
            Cursor.visible = true; //カーソル有効化
            Cursor.lockState = CursorLockMode.None; //カーソル固定解除
            _itemEquipPanels.SetActive(true); //UIを表示

            //ディスプレイ有効化完了
            _displayState = DisplayState.Open;
        }

        /// <summary>
        /// 視点を戻す
        /// </summary>
        public void OnDisableDisplay()
        {
            //操作後一回しか通らないように制御
            if (_displayState != DisplayState.Open) return;
            else _displayState = DisplayState.CameraMove;

            Cursor.visible = false; //カーソル無効化
            Cursor.lockState = CursorLockMode.Locked; //カーソルの固定
            _itemEquipPanels.SetActive(false); //UIを非表示
            PlayerLock(false); //プレイヤー固定解除

            //ディスプレイ無効化完了
            _displayState = DisplayState.Close;
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
