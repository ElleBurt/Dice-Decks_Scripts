using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BoosterTemplate", menuName = "Booster", order = 0)]
public class BoosterTemplate : ScriptableObject {
    public new string name;
    public GameObject BoosterPrefab;
    public Vector3 positionOffset;
    public float scaleFactor;
    public int basePrice;

    [TextAreaAttribute]
    public string description;
}