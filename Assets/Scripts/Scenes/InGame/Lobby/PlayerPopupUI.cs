using Scenes.Lobby.RoomSettingPanel;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace Scenes.Ingame.PlayerUI
{
    /// <summary>
    /// �A�C�e��UI�Ɋւ��鏈�����܂Ƃ߂��N���X
    /// UI���|�b�v�A�b�v�\��
    /// </summary>
    public class PlayerPopupUI : MonoBehaviour
    {
        //UI�֌W
        // [SerializeField] GameObject _itemUI;
        //Ray�֘A
        [SerializeField] Camera _mainCamera;//player�̖ڐ���S���J����
        [SerializeField] private float _getItemRange;//�A�C�e�������ł��鋗��

        [SerializeField] private InteractMap _interactMapCs;
        [SerializeField] private InteractLoptop _interactLoptopCs;
        [SerializeField] private ItemEquip _itemEquipCs;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(1)) // ���N���b�N
            {
                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, _getItemRange) && DisplayStateClose())
                {
                    if (hit.collider.CompareTag("Map")) //Map�^�O���N���b�N
                    {
                        //��\�����\��
                        _interactMapCs.OnEnableDisplay();
                    }
                    else if (hit.collider.CompareTag("Shop")) //Shop�^�O���N���b�N
                    {
                        //��\�����\��
                        _interactLoptopCs.OnEnableDisplay();
                    }
                    else if (hit.collider.CompareTag("ItemEquip")) //ItemEquip�^�O���N���b�N
                    {
                        //��\�����\��
                        _itemEquipCs.OnEnableDisplay();
                    }
                }
            }
        }

        //�p�l�����J���Ă��邩�m�F
        //�J���Ă�����false��Ԃ�
        private bool DisplayStateClose()
        {
            if (_interactMapCs._displayState != InteractMap.DisplayState.Close)
            {
                return false;
            }
            else if (_interactLoptopCs._displayState != InteractLoptop.DisplayState.Close)
            {
                return false;
            }
            else if (_itemEquipCs._displayState != ItemEquip.DisplayState.Close)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}

