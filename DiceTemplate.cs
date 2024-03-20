using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DiceTemplate", menuName = "Dice", order = 0)]
public class DiceTemplate : ScriptableObject {
    
    public new string name;
    public int hiVal;
    public int loVal;
    public bool HasEffect;
    public string EffectType;
    public GameObject dice;
    public Material material;
    public float scale;
    public AudioClip clip;
    public GameObject particleSystem;
    
}
