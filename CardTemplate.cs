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
    BigShield,
    SmallShield,
    Upgrade,
}

[CreateAssetMenu(fileName = "CardTemplate", menuName = "Card", order = 0)]
public class CardTemplate : ScriptableObject {
    public new string name;

    [TextAreaAttribute]
    public string description;

    public float width;

    public bool shouldReset;

    public Sprite imgOverlay;
    public Sprite effectOverlay;
    public Texture2D effectAlpha;

    public CardClass cardClass;
    
}
