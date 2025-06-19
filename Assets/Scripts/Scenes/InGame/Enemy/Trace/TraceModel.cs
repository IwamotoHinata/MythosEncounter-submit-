using Scenes.Ingame.Enemy.Trace;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UnityEngine.InputSystem.XR;

public class TraceModel
{
    private List<EnemyDataStruct> enemyList; // 11種類の敵
    private List<List<TraceType>> _usedCombinations = new List<List<TraceType>>(); // 使用済みの組み合わせ
    public List<List<TraceType>> usedCombinations { get { return _usedCombinations; } }
    public TraceType[] traceTypes(int id)
    {
        return _usedCombinations[id].ToArray();
    }

    public void Init(GameObject parent)
    {
        WebDataRequest.instance.OnEndLoad.Subscribe(_ =>
        {
            Debug.Log("TraceModel.Init");
            AssignTraits();
        }).AddTo(parent.gameObject);
    }

    void AssignTraits()
    {
        enemyList = WebDataRequest.GetEnemyDataArrayList;
        foreach (var enemy in enemyList)
        {
            int whilleCount = 0;
            while (whilleCount < 50) {
                {
                    // 3つの特徴をさらに選択
                    var selectedTraits = SelectRandomTraits(enemy.Feature.ToList(), 3);

                    // 組み合わせを文字列化して重複をチェック
                    var combinationKey = string.Join(",", selectedTraits.OrderBy(t => t.ToString()));
                    var usedCombinationsKey = string.Join(",", _usedCombinations.OrderBy(t => t.ToString()));

                    // 重複しているかチェック
                    if (!usedCombinationsKey.Contains(combinationKey))
                    {
                        _usedCombinations.Add(selectedTraits);
                        break; // 重複がない場合、ループを終了
                    }
                    else
                    {
                        Debug.LogWarning($"この組み合わせはすでに使用されています: {combinationKey}");
                    }
                    whilleCount++;
                }
            }
        }
    }
    List<TraceType> SelectRandomTraits(List<TraceType> uniqueTraits, int count)
    {
        var random = new System.Random();
        var selectedTraits = uniqueTraits.OrderBy(x => random.Next()).Take(count).ToList();
        return selectedTraits;
    }
}
