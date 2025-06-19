using Scenes.Ingame.Enemy.Trace;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridView : MonoBehaviour
{
    [SerializeField] private CellsView cell;
    private List<CellsView> celss = new List<CellsView>();
    public void Init(List<EnemyDataStruct> enemyData)
    {
        foreach (EnemyDataStruct data in enemyData)
        {
            var gridCell = Instantiate(cell,this.transform);
            gridCell.Init(data.Feature, data.Name);
            celss.Add(gridCell);
        }
    }

    public void UpdateJournalList(List<List<TraceType>> types)
    {
        for (int i = 0; i < celss.Count; i++)
        {
            celss[i].UpdateCells(types[i].ToArray());
        }
    }
}
