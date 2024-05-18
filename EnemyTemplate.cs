using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyDiff{ //the int is an indication of the game progression % eg: under 10% is easy || under 85% but above 60% is harder
    NA = 0, 
    Easy = 10,
    Intermediate = 25,
    Medium = 40,
    Hard = 60,
    Harder = 85,
    Hardest = 100,
}

public enum EnemyClass {
    Ground,
    Flying,
    Heavy,
    Poison,
    MiniBoss,
    Boss,
}


[CreateAssetMenu(fileName = "EnemyTemplate", menuName = "Enemy", order = 0)]
public class EnemyTemplate : ScriptableObject {
     
    [Header("General Details")]
    public new string name;
    public float width;
    public float height;
    public EnemyDiff enemyDiff;
    public EnemyClass enemyClass;

    [Header("Mini Details")]
    public GameObject EnemyPrefab;
    public GameObject ScenePrefab;
    public Texture2D mapTexture;
}
