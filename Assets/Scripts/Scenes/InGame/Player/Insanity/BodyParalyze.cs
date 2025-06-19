using Fusion;
using UnityEngine;

namespace Scenes.Ingame.Player
{
    /// <summary>
    /// �����̎�ނ�1��
    /// 1.���炷�X���b�g�ɃA�C�e���������Ă���Ƃ��̏�ɗ��Ƃ�
    /// 2.�g�̂̃}�q
    /// </summary>
    public class BodyParalyze : NetworkBehaviour, IInsanity
    {
        private int[] _randoms = new int[2];
        private PlayerItem _myPlayerItem;
        private MultiPlayerMove _myMultiPlayerMove;
        private PlayerStatus _myPlayerStatus;

        private ItemSlotStruct _unavailableSlot;
        

        public void Setup()
        {
            _myPlayerItem = GetComponent<PlayerItem>();
            _myMultiPlayerMove = GetComponent<MultiPlayerMove>();
            _unavailableSlot = new ItemSlotStruct(null, ItemSlotStatus.unavailable);


            _randoms[0] = Random.Range(0, 7);
            while (true)
            {
                _randoms[1] = Random.Range(0, 7);
                if (_randoms[0] == _randoms[1])
                    continue;
                else
                    break;
            }

            Debug.Log("�I�΂ꂽ�X���b�gNo�F" + _randoms[0] + " , " + _randoms[1]);
        }

        public void Active()
        {
            //�u���炷�X���b�g�ɃA�C�e���������Ă���Ƃ��̏�ɗ��Ƃ��v�����̎���
            for (int i = 0; i < 2; i++)
            {
                var itemSlot = _myPlayerItem.ItemSlots[_randoms[i]];
                if (itemSlot.myItemData != null)
                {
                    //����Ɏ����Ă���A�C�e����������
                    if (_randoms[i] == _myPlayerItem.nowIndex)
                    {
                        _myPlayerItem.nowBringItem.GetComponent<ItemEffect>().OnThrow();

                        //�A�C�e����^���ɗ��Ƃ�
                        _myPlayerItem.nowBringItem.transform.parent = null;
                        var rb = _myPlayerItem.nowBringItem.GetComponent<Rigidbody>();
                        rb.useGravity = true;

                        _myPlayerItem.nowBringItem = null;
                    }
                    else //��Ɏ����Ă��Ȃ��A�C�e����������
                    {
                        var obj = RunnerSpawner.RunnerInstance.Spawn(itemSlot.myItemData.prefab, this.gameObject.transform.position + new Vector3(0,1,0), itemSlot.myItemData.prefab.transform.rotation);
                        Debug.Log("�����I�Ɏ̂Ă�ꂽ�A�C�e�����F" + obj.name);
                        obj.GetComponent<ItemEffect>().ownerPlayerStatus = _myPlayerStatus;
                        obj.GetComponent<ItemEffect>().OnThrow();                       
                    }
                }

                //�����_����2�̃X���b�g�����������p�s�\�ɂ���
                _myPlayerItem.ChangeListValue(_randoms[i], _unavailableSlot);
            }

            //�u�g�̂̃}�q�v�@�\�̎���
            _myMultiPlayerMove.Paralyze(true);
        }

        public void Hide()
        {
            //���炷�X���b�g�ɃA�C�e���������Ă���Ƃ��̏�ɗ��Ƃ������̉���
            for (int i = 0; i < 2; i++)
            {
                _myPlayerItem.ChangeListValue(_randoms[i], new ItemSlotStruct(null, ItemSlotStatus.available));
            }

            //�u�g�̂̃}�q�v����
            _myMultiPlayerMove.Paralyze(false);
            _myMultiPlayerMove.MoveControl(true);
        }
    }
}


