using Scenes.Ingame.Player;
using UnityEngine;
using DG.Tweening;

namespace Scenes.Ingame.Stage
{
    public class StageRack : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private DrawType drawType;
        private enum DrawType
        {
            RightOpen, //âEå¸Ç´Ç…Ç†ÇØÇÈ
            LeftOpen, //ç∂å¸Ç´Ç…Ç†ÇØÇÈ
            DrawOpen, // à¯Ç¢ÇƒäJÇØÇÈ
        }

        [SerializeField]
        private bool _initialStateOpen = true;
        private bool _isOpen = false;
        private bool _isAnimation = false;
        private Vector3 R_OPENVALUE = new Vector3(0, -90, 0);
        private Vector3 L_OPENVALUE = new Vector3(0, 90, 0);
        private Vector3 D_OPENVALUE = new Vector3(1, 0, 0);
        private BoxCollider _rackCollider;
        public void Intract(PlayerStatus status, bool processWithConditionalBypass)
        {
            if ((Input.GetMouseButtonDown(1) || processWithConditionalBypass) && _isAnimation == false)
            {
                _rackCollider.isTrigger = true;
                _isAnimation = true;
                switch (_isOpen)
                {
                    case true:
                        RackClose();
                        _isOpen = false;
                        break;
                    case false:
                        RackOpen();
                        _isOpen = true;
                        break;
                }
            }
        }

        void Awake()
        {
            _rackCollider = GetComponent<BoxCollider>();
            if (_initialStateOpen)
            {
                _rackCollider.isTrigger = false;
                RackOpen();
                _isOpen = true;
            }
        }

        private void AnimationComplete()
        {
            _rackCollider.isTrigger = false;
            _isAnimation = false;
        }

        private void RackOpen()
        {
            SoundManager.Instance.PlaySe("se_door00", transform.position);
            if (drawType == DrawType.RightOpen)
            {
                transform.DORotate(R_OPENVALUE, 1).SetRelative(true).SetEase(Ease.InOutSine).OnComplete(AnimationComplete);
            }
            else if (drawType == DrawType.LeftOpen)
            {
                transform.DORotate(L_OPENVALUE, 1).SetRelative(true).SetEase(Ease.InOutSine).OnComplete(AnimationComplete);
            }
            else if (drawType == DrawType.DrawOpen)
            {
                transform.DOLocalMove(0.3f * D_OPENVALUE, 1).SetRelative(true).SetEase(Ease.InOutSine).OnComplete(AnimationComplete);
            }
        }
        private void RackClose()
        {
            if (drawType == DrawType.RightOpen)
            {
                SoundManager.Instance.PlaySe("se_door01", transform.position);
                transform.DORotate(-R_OPENVALUE, 1).SetRelative(true).SetEase(Ease.InOutSine).OnComplete(AnimationComplete);
            }
            if (drawType == DrawType.LeftOpen)
            {
                SoundManager.Instance.PlaySe("se_door01", transform.position);
                transform.DORotate(-L_OPENVALUE, 1).SetRelative(true).SetEase(Ease.InOutSine).OnComplete(AnimationComplete);
            }
            if (drawType == DrawType.DrawOpen)
            {
                SoundManager.Instance.PlaySe("se_door00", transform.position);
                transform.DOLocalMove(0.3f * -D_OPENVALUE, 1).SetRelative(true).SetEase(Ease.InOutSine).OnComplete(AnimationComplete);
            }
        }
        public string ReturnPopString()
        {
            if (_isOpen)
                return "ï¬Ç∂ÇÈ";
            else
                return "äJÇ≠";
        }
    }
}

