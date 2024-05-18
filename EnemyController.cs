using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum ElementalType{
    Poison,
    Fire,
    Ice,
    Spark,
}

public class EnemyController : MonoBehaviour
{
    private Dictionary<EnemyDiff, (float Health, int Attack, int MoneyGain, string Tier)> diffStats = new Dictionary<EnemyDiff, (float,int,int,string)>{
        {EnemyDiff.Easy, (15f,3,3,"I")},
        {EnemyDiff.Intermediate, (25f,4,5,"II")},
        {EnemyDiff.Medium, (35f,5,8,"III")},
        {EnemyDiff.Hard, (45f,6,13,"IV")},
        {EnemyDiff.Harder, (55f,8,21,"V")},
        {EnemyDiff.Hardest, (75f,10,34,"VI")},
    };

    private Dictionary<ElementalType, (int Duration, int Damage, bool Stun)> currentPoisons = new Dictionary<ElementalType, (int,int,bool)>{};

    
    private bool enemyTurn;
    private bool skipEnemyTurn = false;
    private Transform evnSpawn;
    private GenMapV2 genMapV2;
    private DiceRoller diceRoller;
    private GameObject SelectedEncounter;
    public bool SelectedEncounterDied = false;

    private void Start(){
        evnSpawn = GameObject.Find("EventSpawns").transform;
        genMapV2 = FindObjectOfType<GenMapV2>();
        diceRoller = FindObjectOfType<DiceRoller>();
    }
    
    public void SpawnEnemy(float gameProg){

        diceRoller.canRoll = false;

        EnemyDiff difficulty = EnemyDiff.Easy;

        foreach(EnemyDiff diff in System.Enum.GetValues(typeof(EnemyDiff))){
            if((int)diff > gameProg){
                difficulty = diff;
                break;
            }
        }
        
        List<EnemyTemplate> template = Resources.LoadAll<EnemyTemplate>($"Encounters/Templates/{difficulty}").ToList();

        Debug.Log($"gameProg: {gameProg} \n diff: {difficulty} \n list: {string.Join(", ", template)}");

        EnemyTemplate enemySelected = template[Random.Range(0,template.Count)];

        enemyTurn = enemySelected.enemyClass == EnemyClass.Flying ? true : false;

        GameObject enemy = GameObject.Instantiate(enemySelected.EnemyPrefab,evnSpawn.position + new Vector3(0,50,0),Quaternion.Euler(0,270,0));

        SelectedEncounter = enemy;

        MiniScript miniScript = enemy.GetComponent<MiniScript>();

        miniScript.MaxHealth = diffStats[difficulty].Health;
        miniScript.AttackPower = diffStats[difficulty].Attack;
        miniScript.MoneyGain = diffStats[difficulty].MoneyGain;
        miniScript.diffTier = diffStats[difficulty].Tier;

        
        StartCoroutine(DropEvent(enemy,enemy.transform.position));

    }

    private IEnumerator AttackEvent(){
        enemyTurn = false;
        int Damage = 0;

        foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Encounter")){
            Damage += enemy.GetComponent<MiniScript>().AttackPower;
            genMapV2.UpdateHealth(Damage,true);

            genMapV2.totalHitsTaken++;
            
            yield return new WaitForSeconds((int)genMapV2.gameSpeed);
        }
        
        ProcessEncounter();
    }

    public void DamageEvent(int Damage){
        enemyTurn = true;
        diceRoller.canRoll = false;
        MiniScript enemyScript = SelectedEncounter.GetComponent<MiniScript>();

        if(enemyScript.CurrentHealth - Damage > 0){
            ProcessEncounter();
        }else{
            EnemyKilled(SelectedEncounter);
        }

        SelectedEncounter.GetComponent<MiniScript>().UpdateHealth(Damage, true);
    }

    public void SelectEnemy(GameObject enemy){
        SelectedEncounter = enemy;
    }

    private void EnemyKilled(GameObject enemy){

    }

    private void ProcessEncounter(){
        foreach(KeyValuePair<ElementalType, (int Duration, int Damage, bool Stun)> type in currentPoisons){
            int duration = type.Value.Duration - 1;
            int damage = type.Value.Damage;

            if(type.Value.Stun){
                skipEnemyTurn = true;
            }

            if(duration == 0){
                currentPoisons.Remove(type.Key);
            }else{
                currentPoisons[type.Key] = (duration, damage, false);
            }

            DamageEvent(damage);
        }

        if(enemyTurn && !skipEnemyTurn){
            StartCoroutine(AttackEvent());
        }else{
            skipEnemyTurn = false;
            diceRoller.canRoll = true;
        }
    }

    private IEnumerator DropEvent(GameObject prefab, Vector3 spawnPos){

        yield return new WaitForSeconds((int)genMapV2.gameSpeed);

        float Elapsed = 0f;
    
        while(Elapsed < (int)(genMapV2.gameSpeed)){

            prefab.transform.position = Vector3.Lerp(prefab.transform.position, spawnPos - new Vector3(0,Vector3.Distance(spawnPos,evnSpawn.position),0),  Elapsed / (int)(genMapV2.gameSpeed));

            Elapsed += Time.deltaTime;
            yield return null;
        }
        ProcessEncounter();
    }


}
