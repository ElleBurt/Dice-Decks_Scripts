using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EncounterController : MonoBehaviour
{
    /* 
    [Header("Encounter")]
    public GameObject ScrollPre;
    public GameObject PlayerTokenPrefab;
    public Texture2D grassMaterial;
    public Transform EnemySpwan;
    public GameObject Forest;

    public List<EnemyTemplate> enemyTemplates = new List<EnemyTemplate>();
    private List<GameObject> enemiesActive = new List<GameObject>();

    public int TokenMoveSpeed;

    private GameObject Scroll;

    private GameObject SelectedMini;

    AtkCardHolder atkCardHolder;

    public GameObject selectedIcon;

    //int is the row number, string array contains the minis names that can appear in the row
    private Dictionary<int,string[]> EncounterRowFilters = new Dictionary<int,string[]>(){
        {1, new string[] {"Wolf"}},
        {2, new string[] {"Wolf"}},
        {3, new string[] {"PackOfWolves"}},
    };

    private GameObject TempForest;

    GameController gameController;
    DiceRoller diceRoller;


    void Start()
    {
        gameController = FindObjectOfType<GameController>();
        diceRoller = FindObjectOfType<DiceRoller>();
        atkCardHolder = FindObjectOfType<AtkCardHolder>();
    }

    

    public IEnumerator TriggerEncounter(){

        //spwans scroll
        Scroll = GameObject.Instantiate(ScrollPre, new Vector3(4.9f, 1.3f, 113.2f), Quaternion.identity);
        Scroll.transform.rotation = Quaternion.Euler(-90f, -90f, 0f);
        Scroll.transform.localScale = new Vector3(5000f,5000f,5000f);
        Material newMat = new Material(Scroll.GetComponent<SkinnedMeshRenderer>().material);
        newMat.SetTexture("_BaseColorMap", grassMaterial);
        Scroll.GetComponent<SkinnedMeshRenderer>().material = newMat;

        yield return new WaitForSeconds(3f);

        //spawns surrounding
        TempForest = GameObject.Instantiate(Forest,EnemySpwan.position + new Vector3(0,-20f,12.4f), Quaternion.identity);
        
        StartCoroutine(moveUp(TempForest,TempForest.transform.position,-20f));

        DecideEncounter();

    }


    private void DecideEncounter(){
        string[] PossibleEncounters = EncounterRowFilters[gameController.currentRound];
        
        string EncounteredType = PossibleEncounters[Random.Range(0, PossibleEncounters.Length)];

        DropEncounterToken(EncounteredType,1);

    }


    private void DropEncounterToken(string EncounterName, int Amount) {

        List<Vector3> tokenPositions = new List<Vector3>();
        tokenPositions.Add(EnemySpwan.position);

        EnemyTemplate blueprint = null;

        foreach(EnemyTemplate Enemy in enemyTemplates){
            
            if(Enemy.name == EncounterName){
                blueprint = Enemy;
            }
        
        }

        while(tokenPositions.Count < Amount){
            Vector3 EncounterPos = RandomEncounterPosition();

            foreach(Vector3 position in tokenPositions){

                if(Vector3.Distance(position,EncounterPos) > blueprint.width){
                    tokenPositions.Add(EncounterPos);
                }

            }
            
        }
        
        foreach(Vector3 SpawnPosition in tokenPositions){
            

            GameObject Enemy = GameObject.Instantiate(blueprint.EnemyMini, SpawnPosition + new Vector3(0,40,0), Quaternion.Euler(0,90,0));
            Enemy.transform.name = blueprint.name;
            Enemy.transform.localScale *= blueprint.scale;
            Enemy.transform.tag = "Encounter";

            MiniScript enemyScript = Enemy.AddComponent<MiniScript>();
            enemyScript.MaxHealth = blueprint.MaxHealth; 
            enemyScript.CurrentHealth = blueprint.MaxHealth;

            CapsuleCollider capCol = Enemy.AddComponent<CapsuleCollider>();
            capCol.radius = 2;
            capCol.center = new Vector3(0,1,0);


            GameObject Base = Enemy.transform.Find("Base").gameObject;
            GameObject Mini = Enemy.transform.Find("Enemy").gameObject;
            GameObject Glass = Enemy.transform.Find("Glass").gameObject;
            GameObject Health = Glass.transform.Find("Health").gameObject;
            GameObject Terrain = Enemy.transform.Find("Terrain").gameObject;


            Base.GetComponent<MeshRenderer>().material = blueprint.BaseMaterial;
            Mini.GetComponent<MeshRenderer>().material = blueprint.EnemyMaterial;
            Glass.GetComponent<MeshRenderer>().material = blueprint.GlassMaterial;
            Health.GetComponent<MeshRenderer>().material = new Material(blueprint.HealthMaterial);
            Terrain.GetComponent<MeshRenderer>().material = blueprint.TerrainMaterial;

            Liquid enemyHealth = Health.AddComponent<Liquid>();

            enemyHealth.fillAmount = -5f;

            StartCoroutine(DropToken(Enemy, Enemy.transform.position));
            enemiesActive.Add(Enemy);
        }

        diceRoller.canRoll = true;
    }



    public Vector3 RandomEncounterPosition(){
        float Xpos = Random.Range(-10.5f, 16.5f);
        float Ypos = Mathf.Abs(Xpos)/30 - 2.2f;
        float Zpos = Random.Range(30f,49f);
        
        return new Vector3(Xpos,Ypos,Zpos);
    }

    

    public void SelectMiniToAttack(GameObject Mini){
        
        foreach(GameObject miniEnemy in GameObject.FindGameObjectsWithTag("Encounter")){
            miniEnemy.GetComponent<MiniScript>().selected = false;
        }
        Mini.GetComponent<MiniScript>().selected = true;
        
        GameObject sword = GameObject.Instantiate(selectedIcon, Mini.transform.position + new Vector3(0,Mini.transform.localScale.y + 10f,0),Quaternion.identity * Quaternion.Euler(90,0,0));
        StartCoroutine(SpinSword(sword));
        SelectedMini = Mini;
    }



    public IEnumerator MiniDamaged(int DmgDealt){

        yield return new WaitForSeconds(2f);

        SelectedMini.GetComponent<MiniScript>().UpdateHealth(DmgDealt, true);
        
        StartCoroutine(AttackPhase());
    }

    IEnumerator SpinSword(GameObject sword){
        while(sword != null){
            sword.transform.rotation *=  Quaternion.Euler(0,0,1);
            yield return null;
        }
    }

    private IEnumerator AttackPhase(){

        yield return new WaitForSeconds(1.5f);
        
        int Damage = 0;
        foreach(GameObject enemy in enemiesActive){

            foreach(Transform empty in DmgDice){
                if(Vector3.Distance(empty.forward, -Vector3.forward) < 0.1f){
                    Damage = int.Parse(empty.name);
                }
            }
        }

        gameController.UpdateHealth(Damage,true);
        
    }
    


     //move up sequence for the enviromental objects
    IEnumerator moveUp(GameObject ObjectToMove, Vector3 startPos, float Ypos) {


        while(Vector3.Distance(ObjectToMove.transform.position, (startPos + new Vector3(0,Mathf.Abs(Ypos),0))) > 0.1f){

            ObjectToMove.transform.position = Vector3.Lerp(ObjectToMove.transform.position, (startPos + new Vector3(0,Mathf.Abs(Ypos),0)) , 7f * Time.deltaTime);

            yield return null;
        }
    }

    //Drop sequence for the Enemy Tokens
    IEnumerator DropToken(GameObject ObjectToMove, Vector3 startPos) {

        while(Vector3.Distance(ObjectToMove.transform.position, (startPos - new Vector3(0,41.2f,0))) > 0.1f){

            ObjectToMove.transform.position = Vector3.Lerp(ObjectToMove.transform.position, (startPos - new Vector3(0,41.2f,0)), TokenMoveSpeed * Time.deltaTime);

            yield return null;
        }
    }
    */
}
