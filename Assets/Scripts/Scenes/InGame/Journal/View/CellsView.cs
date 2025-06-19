using Scenes.Ingame.Enemy.Trace;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CellsView : MonoBehaviour
{
    [SerializeField] private Image[] _cells;
    [SerializeField] private Sprite _fillCell;
    [SerializeField] private Sprite _checkCell;
    [SerializeField] private Sprite _checkCell2;
    [SerializeField] private TextMeshProUGUI _name;
    private TraceType[] _iInitalTrace;
    public void Init(TraceType[] traces,string name)
    {
        _iInitalTrace = traces;
        _name.text = name;
        for (int i = 0; i < _cells.Length; i++)
        {
            if (traces.Any(t => (int)t == i))
            {
            }
            else
            {
                _cells[i].sprite = _fillCell;
            }
        }
    }

    public void UpdateCells(TraceType[] traces)
    {
        var difference = _iInitalTrace.Except(traces).ToArray();
        for (int i = 0; i < _cells.Length; i++)
        {
            if (difference.Any(t => (int)t == i))
            {
                _cells[i].sprite = _fillCell;
            }
            else
            {
            }
        }
    }
}
