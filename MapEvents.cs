using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface EventMedium{
    public void ExecuteEvent();
}

public class MapEvents : MonoBehaviour {

    [Header("General")]
    public Transform EventSpawn;
    public GameObject ScrollPrefab;
    private GameObject Scroll;
    private GameController gameController;
    private GameObject currentEevnt;
    private GameObject currentScene;

    [Header("Encounter Related")]
    public GameObject SelectedEncounter;
    public GameObject SelectedSword;
    public List<EnemyTemplate> enemyTemplates = new List<EnemyTemplate>();

    private GenMap genMap;


    


    //int is the row number, string array contains the minis names that can appear in the row
    private Dictionary<int,string[]> EncounterRowFilters = new Dictionary<int,string[]>(){
        {1, new string[] {"Wolf"}},
        {2, new string[] {"Wolf"}},
        {3, new string[] {"PackOfWolves"}},
    };

    void Start(){
        gameController = FindObjectOfType<GameController>();
        genMap = FindObjectOfType<GenMap>();
    }


    public void SpawnMap(GameObject map, Texture2D texture){
        //spwans scroll
        Scroll = GameObject.Instantiate(map, new Vector3(4.9f, 1.3f, 113.2f), Quaternion.identity);
        Scroll.transform.rotation = Quaternion.Euler(-90f, -90f, 0f);
        Material newMat = new Material(Scroll.GetComponent<SkinnedMeshRenderer>().material);
        newMat.SetTexture("_BaseColorMap", texture);
        Scroll.GetComponent<SkinnedMeshRenderer>().material = newMat;
    }




    public void SelectMiniToAttack(GameObject encounter){
        
        foreach(GameObject mini in GameObject.FindGameObjectsWithTag("Encounter")){
            mini.GetComponent<MiniScript>().selected = false;
        }

        encounter.GetComponent<MiniScript>().selected = true;
        
        GameObject sword = GameObject.Instantiate(SelectedSword, encounter.transform.position + new Vector3(0,encounter.transform.localScale.y + 10f,0),Quaternion.identity * Quaternion.Euler(90,0,0));
        StartCoroutine(SpinSword(sword));
        SelectedEncounter = encounter;
    }


    public IEnumerator SpawnEnemy(int CurrentRound){

        yield return new WaitForSeconds(1f);

        string[] PossibleEncounters = EncounterRowFilters[CurrentRound];
        
        string EncounteredType = PossibleEncounters[Random.Range(0, PossibleEncounters.Length)];

        EnemyTemplate template = null;

        foreach(EnemyTemplate Enemy in enemyTemplates){
            
            if(Enemy.name == EncounteredType){
                template = Enemy;
            }
        }

        SpawnMap(ScrollPrefab,template.mapTexture);

        Vector3 e_spawnPos = EventSpawn.position + new Vector3(0,50,0);
        Vector3 s_spawnPos = EventSpawn.position - new Vector3(0,50,0);

        GameObject newEnemy = GameObject.Instantiate(template.EnemyPrefab, e_spawnPos, Quaternion.Euler(0,90,0));

        GameObject newScene = GameObject.Instantiate(template.ScenePrefab, s_spawnPos, Quaternion.identity);

        MiniScript enemyScript = newEnemy.GetComponent<MiniScript>();

        enemyScript.atkPow = template.atkPower;
        enemyScript.MaxHealth = template.MaxHealth; 
        enemyScript.CurrentHealth = template.MaxHealth;

        StartCoroutine(DropEvent(newEnemy, e_spawnPos, false));
        StartCoroutine(DropEvent(newScene, s_spawnPos, true));
    }


    public IEnumerator SpawnEvent(MapEventTemplate template){

        yield return new WaitForSeconds(1f);


        SpawnMap(ScrollPrefab,template.MapMaterial);

        Vector3 e_spawnPos = EventSpawn.position + new Vector3(0,50,0);
        Vector3 s_spawnPos = EventSpawn.position - new Vector3(0,50,0);

        GameObject newEvent = GameObject.Instantiate(template.eventPrefab, e_spawnPos, Quaternion.Euler(0,90,0));

        GameObject newScene = GameObject.Instantiate(template.scenePrefab, s_spawnPos, Quaternion.identity);

        currentEevnt = newEvent;
        currentScene = newScene;

        StartCoroutine(DropEvent(newEvent, e_spawnPos, false));
        StartCoroutine(DropEvent(newScene, s_spawnPos, true));

        StartCoroutine(ActivateEvent());
    }


    public IEnumerator DropEvent(GameObject prefab, Vector3 spawnPos, bool isUp){

        yield return new WaitForSeconds(1.5f);

        if(isUp){
            while(Vector3.Distance(prefab.transform.position, (spawnPos + new Vector3(0,51f,0))) > 0.1f){

                prefab.transform.position = Vector3.Lerp(prefab.transform.position, (spawnPos + new Vector3(0,51f,0)), 10f * Time.deltaTime);

                yield return null;
            }
        }else{
            while(Vector3.Distance(prefab.transform.position, (spawnPos - new Vector3(0,51f,0))) > 0.1f){

                prefab.transform.position = Vector3.Lerp(prefab.transform.position, (spawnPos - new Vector3(0,51f,0)), 10f * Time.deltaTime);

                yield return null;
            }
        }

    }


    public IEnumerator MiniDamaged(int DmgDealt){

        yield return new WaitForSeconds(2f);

        SelectedEncounter.GetComponent<MiniScript>().UpdateHealth(DmgDealt, true);
        
        StartCoroutine(AttackPhase());
    }

    private IEnumerator AttackPhase(){

        yield return new WaitForSeconds(1.5f);

        int Damage = 0;
        foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Encounter")){
            Damage += enemy.GetComponent<MiniScript>().atkPow;
        }
        gameController.UpdateHealth(Damage,true);
    }

    private IEnumerator SpinSword(GameObject sword){
        while(sword != null){
            sword.transform.rotation *=  Quaternion.Euler(0,0,1);
            yield return null;
        }
    }


    public IEnumerator ActivateEvent(){
        yield return new WaitForSeconds(3f);

        EventMedium eMid = currentEevnt.GetComponent<EventMedium>();
        eMid.ExecuteEvent();
    }

    public IEnumerator EventEnded(){

        StartCoroutine(DropEvent(currentEevnt, currentEevnt.transform.position, true));
        StartCoroutine(DropEvent(currentScene, currentEevnt.transform.position, false));

        yield return new WaitForSeconds(1f);

        Scroll.GetComponent<Animator>().SetBool("IconSelected", true);

        yield return new WaitForSeconds(3f);
        Destroy(Scroll,1f);

        gameController.RoundConclusion();
    }
}
