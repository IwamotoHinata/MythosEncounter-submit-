using UnityEngine;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// �����̎�ނ�1��.�S���������ɂ�舫�e�����o��
    /// 1.�X�^�~�i�̏���x�Əo���Ŏ󂯂�_���[�W�̌������x��2�{�ɂȂ�.
    /// 2.���ǂ���ƃv���C���[��1�b��1�x�̃y�[�X�Ŕ�������f���悤�ɂȂ�.
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
            //�X�^�~�i�̏���x��2�{��
            _myMultiPlayerMove.Pulsation(true);

            //�o���Ŏ󂯂�_���[�W��2�{��
            _myPlayerStatus.PulsationBleeding(true);

            //���������͂�����
            //Todo�F�����������

            Debug.Log("�S��������");
        }

        public void Hide()
        {
            _myMultiPlayerMove.Pulsation(false);
            _myPlayerStatus.PulsationBleeding(false);
        }
    }

}