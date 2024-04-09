using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum CardClass{
    Standard,
    Blessed,
    Cursed,
    Celestial,
    BloodShard,
    Shield,
    Upgrade,
}

public enum CardType{
    CloseCall,
    HighRoller,
    RollingRich,
    Jinx,
    FizzBuzz,
    CorruptCoins,
    DefenceForce,
    PlagueDoctor,
    Economics,
    EldritchRage,
    MilitaryInvestment,
    StrengthRitual,
}

[CreateAssetMenu(fileName = "CardTemplate", menuName = "Card", order = 0)]
public class CardTemplate : ScriptableObject {
    public new string name;
    public CardType cardType;

    [TextAreaAttribute]
    public string description;

    public float width;

    public bool shouldReset;

    public Sprite imgOverlay;

    public CardClass cardClass;
    public int baseSellValue;
    public int basePrice;
}
