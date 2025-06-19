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
    /// アイテムUIに関する処理をまとめたクラス
    /// UIをポップアップ表示
    /// </summary>
    public class PlayerPopupUI : MonoBehaviour
    {
        //UI関係
        // [SerializeField] GameObject _itemUI;
        //Ray関連
        [SerializeField] Camera _mainCamera;//playerの目線を担うカメラ
        [SerializeField] private float _getItemRange;//アイテムを入手できる距離

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
            if (Input.GetMouseButtonDown(1)) // 左クリック
            {
                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, _getItemRange) && DisplayStateClose())
                {
                    if (hit.collider.CompareTag("Map")) //Mapタグをクリック
                    {
                        //非表示→表示
                        _interactMapCs.OnEnableDisplay();
                    }
                    else if (hit.collider.CompareTag("Shop")) //Shopタグをクリック
                    {
                        //非表示→表示
                        _interactLoptopCs.OnEnableDisplay();
                    }
                    else if (hit.collider.CompareTag("ItemEquip")) //ItemEquipタグをクリック
                    {
                        //非表示→表示
                        _itemEquipCs.OnEnableDisplay();
                    }
                }
            }
        }

        //パネルを開いているか確認
        //開いていたらfalseを返す
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

