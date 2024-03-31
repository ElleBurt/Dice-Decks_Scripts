using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Rarity{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    CurrentlyImpossible
}

public enum DiceType{
    Basic,
    Multi,
    Roulette,
    Poker,
    Explosive,
    Elemental,
    ReRoll,
    Luck,
}

[CreateAssetMenu(fileName = "DiceTemplate", menuName = "Dice", order = 0)]
public class DiceTemplate : ScriptableObject {
    
    public new string name;
    public int hiVal;
    public int loVal;
    public bool HasEffect;
    [TextAreaAttribute]
    public string description;
    public DiceType diceType;
    public GameObject dice;
    public Rarity itemRarity;
    
}
