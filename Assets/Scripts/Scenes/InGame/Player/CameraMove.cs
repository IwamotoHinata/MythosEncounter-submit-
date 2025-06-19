using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DG.Tweening;


namespace Scenes.Ingame.Player
{
    /// <summary>
    /// �J�����i�v���C���[�̎��_�j��ύX����N���X
    /// player�ɂ��Ă���J�����ɃA�^�b�`����
    /// </summary>
    public class CameraMove : MonoBehaviour
    {
        //�J�����̗h��Ɋւ���ݒ�
        [Tooltip("�J������h�炷��")][SerializeField] private float _offSet;
        [Tooltip("�ҋ@��Ԃł̎��_�ړ��̎���")][SerializeField] private float _idleCycle;

        private Vector3 _cameraPositionDefault;
        private Sequence sequence;

        private void Start()
        {
            //�����̏����̏ꏊ���L�^
            _cameraPositionDefault = this.transform.localPosition;
            ChangeViewPoint(0);
        }

        /// <summary>
        /// �N���b�v�̎��Ԃ̒�������ɁA�J�����ړ��̎��������肷��B
        /// ���ӁF�����̃N���b�v�͊�{�����Q��̃Z�b�g�ɂȂ��Ă���B�P�񕪂ɂ���ɂ�clipTime / 2���g������
        /// </summary>
        /// <param name="clipTime"></param>
        public void ChangeViewPoint(float clipTime)
        {
            //�ҋ@��ԁE�ړ���ԋ��ʂ̏������L�q
            sequence.Kill();
           
            //�ŏ��ɃJ������Default�̈ʒu�ɖ߂�������ǉ�
            sequence = DOTween.Sequence();
            sequence.Append(this.transform.DOLocalMove(new Vector3(0, _cameraPositionDefault.y, _cameraPositionDefault.z), 0));

            //�����Ȃ���Ԃ̎��͈�����0�����Ă���
            //�ҋ@��Ԃł͂ق�̏��������������Ǝ��_���ς��
            if (clipTime == 0)
            {
                sequence.Append(this.transform.DOLocalMove(new Vector3(0, _cameraPositionDefault.y - _offSet / 2, _cameraPositionDefault.z), _idleCycle / 2));
                sequence.Append(this.transform.DOLocalMove(new Vector3(0, _cameraPositionDefault.y, _cameraPositionDefault.z), _idleCycle / 2));
                sequence.Play().SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                //����ݒ肷�鋓�����쐬
                sequence.Append(this.transform.DOLocalMove(new Vector3(0, _cameraPositionDefault.y - _offSet, _cameraPositionDefault.z), clipTime / 2));
                sequence.Append(this.transform.DOLocalMove(new Vector3(0, _cameraPositionDefault.y, _cameraPositionDefault.z), clipTime / 2));
                sequence.Play().SetLoops(-1, LoopType.Yoyo);
            }    
        }
    }
}
    
