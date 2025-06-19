using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemTips : MonoBehaviour
{
    [SerializeField] private GameObject tooltipPanel; // �I�[�o�[���C�̃p�l��
    [SerializeField] private TMP_Text itemNameText;       // �A�C�e������\������Text
    [SerializeField] private TMP_Text itemDescriptionText; // ������\������Text

    [SerializeField] private string itemName;         // �A�C�e����
    [TextArea]
    [SerializeField] private string itemDescription;  // �A�C�e���̐���

    private RectTransform tooltipRectTransform;

    // �A�C�e���摜�ɃA�^�b�`
    // tooltipPanel: �I�[�o�[���C��Panel
    // itemNameText: �A�C�e������\������Text
    // itemDescriptionText: ������\������Text
    // �A�C�e���摜��EventTrigger��ǉ�

    void Start()
    {
        tooltipRectTransform = tooltipPanel.GetComponent<RectTransform>();
        tooltipPanel.SetActive(false); // ������Ԃł͔�\��
    }

    void Update()
    {
        if (tooltipPanel.activeSelf)
        {
            // �}�E�X�ʒu�ɒǏ]
            Vector2 mousePosition = Input.mousePosition;
            tooltipRectTransform.position = mousePosition + new Vector2(50, -50); // �J�[�\�����炸�炷
        }
    }

    public void OnMouseEnter()
    {
        // �I�[�o�[���C��\���ATips��ݒ�
        tooltipPanel.SetActive(true);
        itemNameText.text = itemName;
        itemDescriptionText.text = itemDescription;
    }

    public void OnMouseExit()
    {
        // �I�[�o�[���C���\��
        tooltipPanel.SetActive(false);
    }
}

