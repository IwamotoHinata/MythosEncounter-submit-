public struct EnemyAttackStruct
{
    public int attackId { get; private set; }
    public string name { get; private set; }
    public int directDamage { get; private set; }
    public int bleedingDamage { get; private set; }
    public float stanTime { get; private set; }
    public float range { get; private set; }
    public int attackProbability { get; private set; }
    public int accuracy { get; private set; }
    public float speed { get; private set; }


    public EnemyAttackStruct(int attackId, string name, int directDamage, int bleedingDamage, float stanTime, float range, int attackProbability, int accuracy, float speed)
    {
        this.attackId = attackId;
        this.name = name;
        this.directDamage = directDamage;
        this.bleedingDamage = bleedingDamage;
        this.stanTime = stanTime;
        this.range = range;
        this.attackProbability = attackProbability;
        this.accuracy = accuracy;
        this.speed = speed;
    }
}
