using Scenes.Ingame.Enemy.Trace;

public struct EnemyDataStruct
{
    private int _enemyId;
    private string _name;
    private int _hp;
    private int _stamina;
    private int _armor;
    private float _walkSpeed;
    private float _dashSpeed;
    private int _attackPower;
    private int _actionCooltime;
    private int _hearing;
    private int _vision;
    private string[] _spell;
    private float _san;
    private TraceType[] _fetaure;
    private string[] _attackMethod;

    public int EnemyId { get => _enemyId; }//?L?????N?^?[ID
    public string Name { get => _name; }//?_?b??????
    public int Hp { get => _hp; }//????
    public int Stamina { get => _stamina; }//?X?^?~?i
    public int Armor { get => _armor; }//?h??
    public float WalkSpeed { get => _walkSpeed; }//???s???x
    public float DashSpeed { get => _dashSpeed; }//???????x
    public int AttackPower { get => _attackPower; }//?U????
    public int ActionCooltime { get => _actionCooltime; }//?A?N?V?????N?[???^?C??
    public int Hearing { get => _hearing; }//???o
    public int Vision { get => _vision; }//???o
    public string[] Spell { get => _spell; }//????
    public float San { get => _san; }//???F??????SAN?l
    public TraceType[] Feature { get => _fetaure; }//
    public string[] AttackMethod { get => _attackMethod; }//????

    public void EnemyDataSet(int enemyId, string name, int hp, int stamina, int armor, float walkSpeed, float dashSpeed, int attackPower, int actionCooltime, int hearing, int vision, string[] spell, float san, TraceType[] feature, string[] attackMethod)
    {
        _enemyId = enemyId;
        _name = name;
        _hp = hp;
        _stamina = stamina;
        _armor = armor;
        _walkSpeed = walkSpeed;
        _dashSpeed = dashSpeed;
        _attackPower = attackPower;
        _actionCooltime = actionCooltime;
        _hearing = hearing;
        _vision = vision;
        _spell = spell;
        _san = san;
        _fetaure = feature;
        _attackMethod = attackMethod;
    }
}
