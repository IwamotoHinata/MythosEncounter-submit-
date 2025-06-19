using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Scenes.Ingame.Manager;
using Scenes.Ingame.Player;
using UniRx;

namespace Scenes.Ingame.InGameSystem.UI
{
    /// <summary>
    /// 画面の暗転を自在に行うためのスクリプト.
    /// 主に死亡時に用いる
    /// </summary>
    public class FadeBlackImage : MonoBehaviour
    {
        [SerializeField] private Image _blackImage;
        [SerializeField] private Canvas _thisCanvas;
        [SerializeField] private RectTransform _imageRectTransform;

        public void SubscribeFadePanelEvent()
        {
            _blackImage.DOFade(0, 0f);
            PlayerStatus[] _playerStatuses = FindObjectsOfType<PlayerStatus>();
            foreach (PlayerStatus playerStatus in _playerStatuses)
            {
                playerStatus.OnPlayerSurviveChange
                    .Skip(1)
                    .Subscribe(x =>
                    {
                        if (x)//生き返ったとき
                        {
                            FadeOutImage();
                        }
                        else //死んだとき
                        {
                            FadeInImage();
                        }
                    }).AddTo(this);
            }
        }

        /// <summary>
        /// 画面暗転
        /// </summary>
        public void FadeInImage()
        {
            _thisCanvas.sortingOrder = 99;
            _blackImage.DOFade(1, 2f).SetDelay(2.5f);
        }

        /// <summary>
        /// 画面暗転の解除
        /// </summary>
        public void FadeOutImage()
        {
            _thisCanvas.sortingOrder = -1;
            var sequence = DOTween.Sequence(); //Sequence生成

            //Tweenをつなげる
            sequence.Append(_blackImage.DOFade(0.3f, 4))
                    .Append(_blackImage.DOFade(0.95f, 3))
                    .Append(_blackImage.DOFade(0f, 5));

            sequence.Play();

        }

        /// <summary>
        /// canvasを無効化
        /// </summary>
        public void DisableCanvas()
        {
            _thisCanvas.enabled = false;
        }

        /// <summary>
        /// 画面暗転(ホイッスル使用時)
        /// </summary>
        /// <param name="playerItem"></param>
        public void FadeInWarp()
        {
            _imageRectTransform.anchoredPosition = new Vector2(-_imageRectTransform.sizeDelta.x, 0f);
            _blackImage.color = new Color(_blackImage.color.r, _blackImage.color.g, _blackImage.color.b, 1f);// blackImageの透明化を切る
            var sequence = DOTween.Sequence();

            sequence.Append(_imageRectTransform.DOAnchorPosX(0f, 0.25f))//左から右へ暗くする
                    .AppendInterval(0.4f)
                    .Append(_imageRectTransform.DOAnchorPosX(_imageRectTransform.sizeDelta.x, 0.25f))
                    .AppendCallback(() =>
                    {
                        _blackImage.color = new Color(_blackImage.color.r, _blackImage.color.g, _blackImage.color.b, 0f);// blackImageの状態を元に戻しておく
                        _imageRectTransform.anchoredPosition = Vector2.zero;
                    })
                    .SetDelay(1f);

            sequence.Play();
        }
    }
}
