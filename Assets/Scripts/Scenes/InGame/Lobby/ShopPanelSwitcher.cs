using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPanelSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject[] panels; // �����̃p�l�����i�[����z��

    // ����̃p�l����\�����郁�\�b�h
    public void ShowPanel(int panelIndex)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (i == panelIndex)
            {
                panels[i].SetActive(true); // �w�肳�ꂽ�p�l����\��
            }
            else
            {
                panels[i].SetActive(false); // ���̃p�l���͔�\��
            }
        }
    }
}

