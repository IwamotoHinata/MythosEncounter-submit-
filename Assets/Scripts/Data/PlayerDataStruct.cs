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
    public string Name { get => _name; }//���O
    public int Money { get => _money; }//������
    public DateTime Created { get => _created; }//������
    public DateTime Ended { get => _end; }//������
    public string[] Items { get => _items; }//�A�C�e��
    public string[] Spell { get => _spell; }//�擾��������
    public int SpellCount { get => _spell.Length; }//�擾���������̎�ސ�
    public string[] MythCreature { get => _mythCreature; }//���������_�b����
    public int MythCreatureCount { get => _mythCreature.Length; }//���������_�b�����̎�ނ̐�
    public int Escape { get => _escape; }//�G�𓢔������ɃN���A������
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
