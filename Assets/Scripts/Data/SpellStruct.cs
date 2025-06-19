public struct SpellStruct
{
    private int _spellId;
    private string _name;
    private string _explanation;
    private string _unlockExplanation;
    private int _price;

    public int SpellId { get => _spellId; }
    public string Name { get => _name; }
    public string Explanation { get => _explanation; }
    public string unlockExplanation { get => _unlockExplanation; }
    public int Price { get => _price; }
    public void SpellDataSet(int spellId, string name, string explanation, string unlockExplanation)
    {
        _spellId = spellId;
        _name = name;
        _explanation = explanation;
        _unlockExplanation = unlockExplanation;
    }
}
