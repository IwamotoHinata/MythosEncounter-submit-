using Scenes.Ingame.Enemy.Trace;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UnityEngine.InputSystem.XR;

public class TraceModel
{
    private List<EnemyDataStruct> enemyList; // 11��ނ̓G
    private List<List<TraceType>> _usedCombinations = new List<List<TraceType>>(); // �g�p�ς݂̑g�ݍ��킹
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
                    // 3�̓���������ɑI��
                    var selectedTraits = SelectRandomTraits(enemy.Feature.ToList(), 3);

                    // �g�ݍ��킹�𕶎��񉻂��ďd�����`�F�b�N
                    var combinationKey = string.Join(",", selectedTraits.OrderBy(t => t.ToString()));
                    var usedCombinationsKey = string.Join(",", _usedCombinations.OrderBy(t => t.ToString()));

                    // �d�����Ă��邩�`�F�b�N
                    if (!usedCombinationsKey.Contains(combinationKey))
                    {
                        _usedCombinations.Add(selectedTraits);
                        break; // �d�����Ȃ��ꍇ�A���[�v���I��
                    }
                    else
                    {
                        Debug.LogWarning($"���̑g�ݍ��킹�͂��łɎg�p����Ă��܂�: {combinationKey}");
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
