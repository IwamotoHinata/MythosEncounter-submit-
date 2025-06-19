using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scenes.Ingame.Enemy;
using UniRx;
using System;
using Fusion;
using Common.Player;

namespace Scenes.Ingame.Player
{
    public class DollEffect : ItemEffect
    {
        public override void OnPickUp()
        {

        }

        public override void OnThrow()
        {

        }

        public override void Effect()
        {

        }

        /// <summary>
        /// player�̗̑͂�0�ɂȂ����Ƃ��ɌĂяo�����֐�
        /// </summary>
        public void UniqueEffect(PlayerStatus status)
        {
            var PlayerItem = status.gameObject.GetComponent<PlayerItem>();
            //�A�C�e���ɃA�^�b�`����Ă���Effect�n�̃X�N���v�g�Ɏ擾�҂̏��𗬂��B
            ownerPlayerStatus = status;
            ownerPlayerItem = PlayerItem;
            ownerPlayerStatus.ReviveCharacter();
            SoundManager.Instance.PlaySe("se_voodoo00", transform.position);

            GameObject[] Enemys = GameObject.FindGameObjectsWithTag("Enemy");

            foreach (var enemy in Enemys) 
            { 
                if (enemy != null) 
                {
                    enemy.GetComponent<EnemyMove>().ResetPosition();
                }
            }

            //if(status.gameObject.GetComponent<NetworkObject>().HasInputAuthority)
            SpellUnlockSystem.Instance.IncreaseUseDollTimes();
        }




    }
}
