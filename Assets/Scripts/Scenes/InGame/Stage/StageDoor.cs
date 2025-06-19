using Scenes.Ingame.Player;
using UnityEngine;
using DG.Tweening;

namespace Scenes.Ingame.Stage
{
    public class StageDoor : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private bool _initialStateOpen = true;
        private bool _isOpen = false;
        [SerializeField] private bool _isAnimation = false;
        private Vector3 OPENVALUE = new Vector3(0, 90, 0);
        private BoxCollider _doorCollider;

        public bool ReturnIsOpen { get { return _isOpen; } }
        public bool ReturnIsAnimation { get { return _isAnimation; } }

        public void Intract(PlayerStatus status,bool processWithConditionalBypass)
        {
            if ((Input.GetMouseButtonDown(1) || processWithConditionalBypass) && _isAnimation == false)
            {
                _doorCollider.isTrigger = true;
                _isAnimation = true;
                switch (_isOpen)
                {
                    case true:
                        DoorClose();
                        _isOpen = false;
                        break;
                    case false:
                        DoorOpen();
                        _isOpen = true;
                        break;
                }
            }
        }

        void Awake()
        {
            _doorCollider = GetComponent<BoxCollider>();
            if (_initialStateOpen)
            {
                _isAnimation = true;
                _doorCollider.isTrigger = false;
                QuickDoorOpen();
                _isOpen = true;
            }
        }
        private void DoorOpen()
        {
            transform.DORotate(OPENVALUE, 1).SetRelative(true).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                _doorCollider.isTrigger = false;
                _isAnimation = false;
            });
            SoundManager.Instance.PlaySe("se_door00", transform.position);
        }
        private void DoorClose()
        {
            transform.DORotate(-OPENVALUE, 1).SetRelative(true).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                _doorCollider.isTrigger = false;
                _isAnimation = false;
            });
            SoundManager.Instance.PlaySe("se_door00", transform.position);
        }

        private void QuickDoorOpen()
        {
            transform.DORotate(OPENVALUE, 0).SetRelative(true).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                _doorCollider.isTrigger = false;
                _isAnimation = false;
            });
        }

        private void QuickDoorClose()
        {
            transform.DORotate(-OPENVALUE, 0).SetRelative(true).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                _doorCollider.isTrigger = false;
                _isAnimation = false;
            });
        }

        public void ChangeDoorQuickOpen(bool open)
        {

            if (_isAnimation)
            {
                Debug.LogWarning("アニメーション中です");

            }
            else
            {

                if (open)
                {
                    if (!_isOpen)
                    {
                        _isAnimation = true;
                        QuickDoorOpen();
                        _isOpen = true;
                    }
                }
                else
                {
                    if (_isOpen)
                    {
                        _isAnimation = true;
                        QuickDoorClose();
                        _isOpen = false;
                    }
                }
            }
        }

        /// <summary>
        /// ドアを設定されていた初期状態に戻す
        /// </summary>
        public void ChangeDoorInitial()
        {
            if (_isAnimation)
            {
                Debug.LogWarning("アニメーション中です");

            }
            else
            {
                if (_initialStateOpen)
                {
                    if (!_isOpen)
                    {
                        _isAnimation = true;
                        QuickDoorOpen();
                        _isOpen = true;
                    }
                }
                else
                {
                    if (_isOpen)
                    {
                        _isAnimation = true;
                        QuickDoorClose();
                        _isOpen = false;
                    }
                }
            }
        }

        public string ReturnPopString()
        {
            if (_isOpen)
                return "閉じる";
            else
                return "開く";
        }

    }
}