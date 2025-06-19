using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemTips : MonoBehaviour
{
    [SerializeField] private GameObject tooltipPanel; // オーバーレイのパネル
    [SerializeField] private TMP_Text itemNameText;       // アイテム名を表示するText
    [SerializeField] private TMP_Text itemDescriptionText; // 説明を表示するText

    [SerializeField] private string itemName;         // アイテム名
    [TextArea]
    [SerializeField] private string itemDescription;  // アイテムの説明

    private RectTransform tooltipRectTransform;

    // アイテム画像にアタッチ
    // tooltipPanel: オーバーレイのPanel
    // itemNameText: アイテム名を表示するText
    // itemDescriptionText: 説明を表示するText
    // アイテム画像にEventTriggerを追加

    void Start()
    {
        tooltipRectTransform = tooltipPanel.GetComponent<RectTransform>();
        tooltipPanel.SetActive(false); // 初期状態では非表示
    }

    void Update()
    {
        if (tooltipPanel.activeSelf)
        {
            // マウス位置に追従
            Vector2 mousePosition = Input.mousePosition;
            tooltipRectTransform.position = mousePosition + new Vector2(50, -50); // カーソルからずらす
        }
    }

    public void OnMouseEnter()
    {
        // オーバーレイを表示、Tipsを設定
        tooltipPanel.SetActive(true);
        itemNameText.text = itemName;
        itemDescriptionText.text = itemDescription;
    }

    public void OnMouseExit()
    {
        // オーバーレイを非表示
        tooltipPanel.SetActive(false);
    }
}

