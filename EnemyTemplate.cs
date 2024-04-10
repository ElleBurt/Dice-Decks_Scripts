using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyTemplate", menuName = "Enemy", order = 0)]
public class EnemyTemplate : ScriptableObject {
    
    [Header("General Details")]
    public new string name; 
    public float MaxHealth;
    public float width;
    public int atkPower;
    public Vector2 encouterRows;
    public int MoneyGain;

    [Header("Mini Details")]
    public GameObject EnemyPrefab;
    public GameObject ScenePrefab;
    public Texture2D mapTexture;
}
