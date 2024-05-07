using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyDiff{
    Easy = 1,
    Medium = 2,
    Hard = 3,
    MiniBoss = 4,
    Boss = 5,
}

[CreateAssetMenu(fileName = "EnemyTemplate", menuName = "Enemy", order = 0)]
public class EnemyTemplate : ScriptableObject {
    
    [Header("General Details")]
    public new string name; 
    public float MaxHealth;
    public float width;
    public float height;
    public int atkPower;
    public EnemyDiff enemyDiff;
    public int MoneyGain;

    [Header("Mini Details")]
    public GameObject EnemyPrefab;
    public GameObject ScenePrefab;
    public Texture2D mapTexture;
}
