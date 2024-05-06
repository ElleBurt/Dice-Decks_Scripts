using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BoosterTemplate", menuName = "Booster", order = 0)]
public class BoosterTemplate : ScriptableObject {
    public new string name;
    public GameObject BoosterPrefab;
    public Vector3 positionOffset;
    public int basePrice;
    
    [Range(0f, 10f)]
    public float scaleFactor = 1.0f;
    [Range(0f, 10f)]
    public float offsetFactor = 1.0f;

    [TextAreaAttribute]
    public string description;
}