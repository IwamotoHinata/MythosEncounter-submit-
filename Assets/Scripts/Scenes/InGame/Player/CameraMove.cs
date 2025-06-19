using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DG.Tweening;


namespace Scenes.Ingame.Player
{
    /// <summary>
    /// カメラ（プレイヤーの視点）を変更するクラス
    /// playerについているカメラにアタッチする
    /// </summary>
    public class CameraMove : MonoBehaviour
    {
        //カメラの揺れに関する設定
        [Tooltip("カメラを揺らす幅")][SerializeField] private float _offSet;
        [Tooltip("待機状態での視点移動の周期")][SerializeField] private float _idleCycle;

        private Vector3 _cameraPositionDefault;
        private Sequence sequence;

        private void Start()
        {
            //視線の初期の場所を記録
            _cameraPositionDefault = this.transform.localPosition;
            ChangeViewPoint(0);
        }

        /// <summary>
        /// クリップの時間の長さを基に、カメラ移動の周期を決定する。
        /// 注意：足音のクリップは基本足音２回のセットになっている。１回分にするにはclipTime / 2を使うこと
        /// </summary>
        /// <param name="clipTime"></param>
        public void ChangeViewPoint(float clipTime)
        {
            //待機状態・移動状態共通の処理を記述
            sequence.Kill();
           
            //最初にカメラをDefaultの位置に戻す処理を追加
            sequence = DOTween.Sequence();
            sequence.Append(this.transform.DOLocalMove(new Vector3(0, _cameraPositionDefault.y, _cameraPositionDefault.z), 0));

            //動かない状態の時は引数に0を入れている
            //待機状態ではほんの少しだけゆっくりと視点が変わる
            if (clipTime == 0)
            {
                sequence.Append(this.transform.DOLocalMove(new Vector3(0, _cameraPositionDefault.y - _offSet / 2, _cameraPositionDefault.z), _idleCycle / 2));
                sequence.Append(this.transform.DOLocalMove(new Vector3(0, _cameraPositionDefault.y, _cameraPositionDefault.z), _idleCycle / 2));
                sequence.Play().SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                //今回設定する挙動を作成
                sequence.Append(this.transform.DOLocalMove(new Vector3(0, _cameraPositionDefault.y - _offSet, _cameraPositionDefault.z), clipTime / 2));
                sequence.Append(this.transform.DOLocalMove(new Vector3(0, _cameraPositionDefault.y, _cameraPositionDefault.z), clipTime / 2));
                sequence.Play().SetLoops(-1, LoopType.Yoyo);
            }    
        }
    }
}
    
