using System;

public struct PlayerDataStruct
{
    private int _recordId;
    private string _name;
    private DateTime _created;
    private DateTime _end;
    private int _money;
    private string[] _items;
    private string[] _spell;
    private string[] _mythCreature;
    private int _escape;
    private int _dispersingEscape;


    public int Id { get => _recordId; }//ID
    public string Name { get => _name; }//名前
    public int Money { get => _money; }//所持金
    public DateTime Created { get => _created; }//所持金
    public DateTime Ended { get => _end; }//所持金
    public string[] Items { get => _items; }//アイテム
    public string[] Spell { get => _spell; }//取得した呪文
    public int SpellCount { get => _spell.Length; }//取得した呪文の種類数
    public string[] MythCreature { get => _mythCreature; }//遭遇した神話生物
    public int MythCreatureCount { get => _mythCreature.Length; }//遭遇した神話生物の種類の数
    public int Escape { get => _escape; }//敵を討伐せずにクリアした回数
    public void PlayerDataSet(int recordId, string name, DateTime created, DateTime end, int money, string[] items, string[] mythCreature, string[] spell, int escape)
    {
        _recordId = recordId;
        _name = name;
        _created = created;
        _end = end;
        _money = money;
        _items = items;
        _spell = spell;
        _mythCreature = mythCreature;
        _escape = escape;
    }
}
