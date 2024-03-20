using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapEventTemplate", menuName = "MapEvent", order = 0)]
public class MapEventTemplate : ScriptableObject {
    public new string name;

    public GameObject eventPrefab;
    public GameObject scenePrefab;

    public Texture2D MapMaterial;
}

