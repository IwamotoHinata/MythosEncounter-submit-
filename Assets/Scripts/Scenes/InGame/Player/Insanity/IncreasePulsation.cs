using UnityEngine;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// 発狂の種類の1つ.心拍数増加により悪影響が出る
    /// 1.スタミナの消費速度と出血で受けるダメージの減少速度が2倍になる.
    /// 2.発症するとプレイヤーが1秒に1度のペースで白い息を吐くようになる.
    /// </summary>
    public class IncreasePulsation : MonoBehaviour, IInsanity
    {
        private PlayerStatus _myPlayerStatus;
        private MultiPlayerMove _myMultiPlayerMove;


        public void Setup()
        {
            _myPlayerStatus = GetComponent<PlayerStatus>();
            _myMultiPlayerMove = GetComponent<MultiPlayerMove>();
        }

        public void Active()
        {
            //スタミナの消費速度が2倍に
            _myMultiPlayerMove.Pulsation(true);

            //出血で受けるダメージが2倍に
            _myPlayerStatus.PulsationBleeding(true);

            //白い息をはかせる
            //Todo：今後実装する

            Debug.Log("心拍数増加");
        }

        public void Hide()
        {
            _myMultiPlayerMove.Pulsation(false);
            _myPlayerStatus.PulsationBleeding(false);
        }
    }

}