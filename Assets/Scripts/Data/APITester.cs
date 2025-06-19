using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UniRx;
using Data;
using System;
public class APITester : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _textMeshPro;
    [SerializeField]
    private Button _itemTableButton;
    [SerializeField]
    private Button _SpellTableButton;
    [SerializeField]
    private Button _PlayerTableButton;
    [SerializeField]
    private Button _EnemyTableButton;
    [SerializeField]
    private Button _itemFacadeButton;
    [SerializeField]
    private Button _SpellFacadeButton;
    [SerializeField]
    private Button _EnemyFacadeButton;
    [SerializeField]
    private Button _EnemyAttackButton;
    [SerializeField]
    private Button _PutPlayerDataButton;
    [SerializeField]
    private TMP_InputField _IdInput;
    [SerializeField]
    private TMP_InputField _NameIput;
    void Start()
    {
        _itemTableButton.OnClickAsObservable().Subscribe(_ => ViewItemTable()).AddTo(this);
        _SpellTableButton.OnClickAsObservable().Subscribe(_ => ViewSpellTable()).AddTo(this);
        _PlayerTableButton.OnClickAsObservable().Subscribe(_ => ViewPlayerTable()).AddTo(this);
        _EnemyTableButton.OnClickAsObservable().Subscribe(_ => ViewEnemyTable()).AddTo(this);
        _EnemyAttackButton.OnClickAsObservable().Subscribe(_ => ViewEnemyAttackTable()).AddTo(this);

        _itemFacadeButton.OnClickAsObservable().Subscribe(_ => ViewItemFacade()).AddTo(this);
        _SpellFacadeButton.OnClickAsObservable().Subscribe(_ => ViewSpellFacade()).AddTo(this);
        _EnemyFacadeButton.OnClickAsObservable().Subscribe(_ => ViewEnemyFacade()).AddTo(this);
        _PutPlayerDataButton.OnClickAsObservable().Subscribe(_ => PutPlayerDebugData()).AddTo(this);
    }

    private void ViewItemTable()
    {
        var data = WebDataRequest.GetItemDataArrayList;
        Debug.Log($"ViewItemTable : {data.Count}");
        _textMeshPro.text = "id,name,explanation,catgory,price\n";
        foreach (var item in data)
        {
            _textMeshPro.text += $"{item.ItemId},{item.Name},{item.Description},{item.ItemCategory},{item.Price}\n";
        }
    }
    private void ViewSpellTable()
    {
        var data = WebDataRequest.GetSpellDataArrayList;
        Debug.Log($"ViewSpellTable : {data.Count}");
        _textMeshPro.text = "id,name,explanation,unlockExplanation\n";
        foreach (var item in data)
        {
            _textMeshPro.text += $"{item.SpellId},{item.Name},{item.Explanation},{item.unlockExplanation}\n";
        }
    }
    private void ViewPlayerTable()
    {
        var data = WebDataRequest.GetPlayerDataArrayList;
        Debug.Log($"ViewPlayerTable : {data.Count}");
        _textMeshPro.text = "id,name,createdDate,endDate,money,item,enemy,spell\n";
        foreach (var item in data)
        {
            _textMeshPro.text += $"{item.Id},{item.Name},{item.Created},{item.Ended},{item.Money},{item.Items.Length},{item.MythCreature.Length},{item.Spell.Length}\n";
        }
    }
    private void ViewEnemyTable()
    {
        var data = WebDataRequest.GetEnemyDataArrayList;
        Debug.Log($"ViewPlayerTable : {data.Count}");
        _textMeshPro.text = "id,name,hp,stamia,armor,walkSpeed,dashSpeed,attack,actionCoolTime,sapell,san,feature,attackMethod\n";
        foreach (var item in data)
        {
            _textMeshPro.text += $"{item.EnemyId},{item.Name},{item.Stamina},{item.Armor},{item.WalkSpeed},{item.DashSpeed},{item.AttackPower},{item.ActionCooltime},{item.Spell.Length},{item.San},{item.Feature.Length},{item.AttackMethod.Length}\n";
        }
    }
    private void ViewItemFacade()
    {
        _textMeshPro.text = "Owned\nid,name,explanation,catgory,price\n";
        var data = PlayerInformationFacade.Instance.GetItem(PlayerInformationFacade.ItemRequestType.Owned);
        foreach (var item in data)
        {
            _textMeshPro.text += $"id {item.Key} ???A?C?e???? {item.Value} ??\n";
        }
        _textMeshPro.text += "not Owned\nid,name,explanation,catgory,price\n";
        data = PlayerInformationFacade.Instance.GetItem(PlayerInformationFacade.ItemRequestType.NotOwned);
        foreach (var item in data)
        {
            _textMeshPro.text += $"id {item.Key} ???A?C?e???? {item.Value} ??\n";
        }
    }
    private void ViewSpellFacade()
    {
        _textMeshPro.text = "Owned\nid,name,explanation,unlockExplanation\n";
        var data = PlayerInformationFacade.Instance.GetSpell(PlayerInformationFacade.spellRequestType.Owned);
        foreach (var item in data.Values)
        {
            _textMeshPro.text += $"{item.SpellId},{item.Name},{item.Explanation},{item.unlockExplanation}\n";
        }
        _textMeshPro.text += "not Owned\nid,name,explanation,unlockExplanation\n";
        data = PlayerInformationFacade.Instance.GetSpell(PlayerInformationFacade.spellRequestType.NotOwned);
        foreach (var item in data.Values)
        {
            _textMeshPro.text += $"{item.SpellId},{item.Name},{item.Explanation},{item.unlockExplanation}\n";
        }
    }
    private void ViewEnemyFacade()
    {
        _textMeshPro.text = "Owned\nid,name,explanation,catgory,price\n";
        var data = PlayerInformationFacade.Instance.GetEnemy(PlayerInformationFacade.EnemyRequestType.Met);
        foreach (var item in data)
        {
            _textMeshPro.text += $"enemy id {item.Key} encountered {item.Value} times\n";
        }
        _textMeshPro.text += "not Owned\nid,name,explanation,catgory,price\n";
        data = PlayerInformationFacade.Instance.GetEnemy(PlayerInformationFacade.EnemyRequestType.NotMet);
        foreach (var item in data.Values)
        {
            _textMeshPro.text += $"not met enemy id is {item}\n";
        }
    }

    private void ViewEnemyAttackTable()
    {
        var data = WebDataRequest.GetEnemyAttacArrayList;
        Debug.Log($"ViewEnemyAttackTable : {data.Count}");
        _textMeshPro.text = "id,name,d_damage,b_damage,stanTime,probability,accuracy,speed\n";
        foreach (var item in data)
        {
            _textMeshPro.text += $"{item.attackId},{item.name},{item.directDamage},{item.bleedingDamage},{item.stanTime},{item.attackProbability},{item.accuracy},{item.speed}\n";
        }
    }

    private void PutPlayerDebugData()
    {
        if (_NameIput.text == "" || _IdInput.text == "")
        {
            Debug.LogError("Name or Id is null");
            return;
        }
        int id = int.Parse(_IdInput.text);
        if (id < 9000)
        {
            Debug.LogError("Debug id need use over 9000");
            return;
        }
        PlayerDataStruct sendData = new PlayerDataStruct();
        sendData.PlayerDataSet(id, _NameIput.text, DateTime.Now, DateTime.UtcNow, 9900, new string[3] { "1=1", "2=1", "3=2" }, new string[3] { "1=1", "2=1", "3=2" }, new string[3] { "1", "2", "3" }, 10);
        WebDataRequest.PutPlayerData(sendData).Forget();
    }
}
