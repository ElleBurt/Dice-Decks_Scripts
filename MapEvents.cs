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
    public GameObject CardPack;
    public GameObject DiceBox;
    public Vector3 BoosterSpawnPosition;
    private GameObject Scroll;
    private GameController gameController;
    private DiceRoller diceRoller;
    private GameObject currentEevnt;
    private GameObject currentScene;
    private AtkCardHolder atkCardHolder;
    

    [Header("Encounter Related")]
    public GameObject SelectedEncounter;
    public GameObject SelectedSword;
    public List<EnemyTemplate> enemyTemplates = new List<EnemyTemplate>();
    public bool SelectedMiniDied = false;
    private GenMap genMap;
    private GameObject activeSword;
    



    void Start(){
        gameController = FindObjectOfType<GameController>();
        genMap = FindObjectOfType<GenMap>();
        diceRoller = FindObjectOfType<DiceRoller>();
        atkCardHolder = FindObjectOfType<AtkCardHolder>();
    }


    public void SpawnMap(GameObject map, Texture2D texture){
        //spwans scroll
        Scroll = GameObject.Instantiate(map, new Vector3(4.9f, 1.3f, 113.2f), Quaternion.identity);
        Scroll.transform.rotation = Quaternion.Euler(-90f, -90f, 0f);
        Material newMat = new Material(Scroll.GetComponent<SkinnedMeshRenderer>().material);
        newMat.SetTexture("_BaseMap", texture);
        Scroll.GetComponent<SkinnedMeshRenderer>().material = newMat;
    }

    public void SpawnBooster(){
        GameObject cardPack = GameObject.Instantiate(CardPack, BoosterSpawnPosition, Quaternion.identity);
        StartCoroutine(cardPack.GetComponent<CardBoosterController>().OpenSequence());
    }

    public void SpawnDiceBox(){
        GameObject diceBox = GameObject.Instantiate(DiceBox, BoosterSpawnPosition, Quaternion.identity);
        StartCoroutine(diceBox.GetComponent<DiceBoxController>().OpenSequence());
    }




    public void SelectMiniToAttack(GameObject encounter){
        
        foreach(GameObject mini in GameObject.FindGameObjectsWithTag("Encounter")){
            mini.GetComponent<MiniScript>().selected = false;
        }

        encounter.GetComponent<MiniScript>().selected = true;
        
        GameObject sword = GameObject.Instantiate(SelectedSword, encounter.transform.position + new Vector3(0,encounter.transform.localScale.y + 10f,0),Quaternion.identity * Quaternion.Euler(90,0,0));
        StartCoroutine(SpinSword(sword));
        SelectedEncounter = encounter;
        activeSword = sword;
    }


    public IEnumerator SpawnEnemy(int CurrentRound){

        yield return new WaitForSeconds(1f);

        EnemyTemplate template = null;

        List<EnemyTemplate> tempList = new List<EnemyTemplate>();

        foreach(EnemyTemplate Enemy in enemyTemplates){
            if(isInRange(CurrentRound,Enemy.encouterRows.x,Enemy.encouterRows.y)){
                tempList.Add(Enemy);
            }
        }

        template = tempList[Random.Range(0,tempList.Count)];

        SpawnMap(ScrollPrefab,template.mapTexture);

        Vector3 e_spawnPos = EventSpawn.position + new Vector3(0,50,0);
        Vector3 s_spawnPos = EventSpawn.position - new Vector3(0,50,0);

        GameObject newEnemy = GameObject.Instantiate(template.EnemyPrefab, e_spawnPos, Quaternion.Euler(0,90,0));

        GameObject newScene = GameObject.Instantiate(template.ScenePrefab, s_spawnPos, Quaternion.identity);
        currentScene = newScene;

        MiniScript enemyScript = newEnemy.GetComponent<MiniScript>();

        enemyScript.atkPow = template.atkPower;
        enemyScript.MaxHealth = template.MaxHealth; 
        enemyScript.CurrentHealth = template.MaxHealth;

        StartCoroutine(DropEvent(newEnemy, e_spawnPos, false, false));
        StartCoroutine(DropEvent(newScene, s_spawnPos, true, true));

        yield return new WaitForSeconds(2f);

        diceRoller.canRoll = true;

        if(newEnemy.transform.GetChild(0).CompareTag("Encounter")){
            SelectMiniToAttack(newEnemy.transform.GetChild(0).gameObject);
        }      
        else{
            Debug.Log("newEnemy to attack");
            SelectMiniToAttack(newEnemy);
        }
    }

    private static bool isInRange(int currentRound, float minRange, float maxRange){
        return currentRound >= minRange && currentRound <= maxRange;
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

        StartCoroutine(DropEvent(newEvent, e_spawnPos, false, false));
        StartCoroutine(DropEvent(newScene, s_spawnPos, true, true));

        StartCoroutine(ActivateEvent());
    }


    public IEnumerator DropEvent(GameObject prefab, Vector3 spawnPos, bool isUp, bool isScene){

        yield return new WaitForSeconds(1.5f);

        //okay this may look a little confusing but its rather simple
        float speed = isUp && isScene ? 3f //if its a scene moving up (this is when the event begins)
                    : isUp ? 4f //if its not a scene but is moving up (this is when the event concludes)
                    : isScene ? 2f //if its a scene moving down (this is when the event concludes)
                    : 5f; //if its not a scene moving down (these are encounters and event prefabs, and is at the start of new events)

        if(isUp){
            while(Vector3.Distance(prefab.transform.position, (spawnPos + new Vector3(0,51f,0))) > 0.1f){

                prefab.transform.position = Vector3.Lerp(prefab.transform.position, (spawnPos + new Vector3(0,51f,0)), speed * Time.deltaTime);

                yield return null;
            }
        }else{
            while(Vector3.Distance(prefab.transform.position, (spawnPos - new Vector3(0,51f,0))) > 0.1f){

                prefab.transform.position = Vector3.Lerp(prefab.transform.position, (spawnPos - new Vector3(0,51f,0)), speed * Time.deltaTime);

                yield return null;
            }
        }

    }


    public IEnumerator MiniDamaged(int DmgDealt){

        yield return new WaitForSeconds(2f);

        SelectedEncounter.GetComponent<MiniScript>().UpdateHealth(DmgDealt, true);
        
        if(!SelectedMiniDied){
            if(atkCardHolder.lastCard){
                Debug.Log("We arent always lucky i guess");
            }
            StartCoroutine(AttackPhase());
        }else{
            yield return new WaitForSeconds(1f);
            Destroy(SelectedEncounter);
            SelectedMiniDied = false;
            Destroy(activeSword);
            StartCoroutine(EventEnded(true));
        }   
    }

    private IEnumerator AttackPhase(){

        yield return new WaitForSeconds(1.5f);

        SelectedEncounter.GetComponent<MiniScript>().TickDamageInflicted();

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

    public IEnumerator EventEnded(bool isEncounter){

        if(!isEncounter){
            StartCoroutine(DropEvent(currentEevnt, currentEevnt.transform.position, true, false));
        }
        StartCoroutine(DropEvent(currentScene, currentScene.transform.position, false, true));

        yield return new WaitForSeconds(1f);

        Scroll.GetComponent<Animator>().SetBool("IconSelected", true);

        yield return new WaitForSeconds(3f);
        Destroy(Scroll,1f);
        Destroy(currentScene,1f);
        Destroy(currentEevnt,1f);

        gameController.RoundConclusion();
    }
}
