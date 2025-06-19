using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPanelSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject[] panels; // 複数のパネルを格納する配列

    // 特定のパネルを表示するメソッド
    public void ShowPanel(int panelIndex)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (i == panelIndex)
            {
                panels[i].SetActive(true); // 指定されたパネルを表示
            }
            else
            {
                panels[i].SetActive(false); // 他のパネルは非表示
            }
        }
    }
}

