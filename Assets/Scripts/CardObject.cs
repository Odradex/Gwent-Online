using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "ScriptableObjects/Card", order = 1)]
public class CardObject : ScriptableObject
{
    public enum Row
    {
        Close,
        Ranged,
        Siege,
        Agile
    }
    public enum Type
    {
        Unit,
        Special,
        Leader
    }
    public enum Ability
    {
        Medic,
        MoraleBoost,
        Muster,
        Spy,
        TightBond,
        Scorch,
        Horn,
        None
    }

    public int basePower;
    public Sprite FullImage;
    public Sprite SmallImage;
    public new string name;
    public bool isHero;

    public Ability ability;
    public Type type;
    public Row row;


    public CardObject(int basePower, int power, Sprite fullImage, Sprite smallImage, string name, int ability, int type, int row)
    {
        this.basePower = basePower;
        FullImage = fullImage;
        SmallImage = smallImage;
        this.name = name;
        this.ability = (Ability)ability;
        this.type = (Type)type;
        this.row = (Row)row;
        isHero = false;
    }
}