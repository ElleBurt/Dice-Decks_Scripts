using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class CampFireController : MonoBehaviour, EventMedium
{   
    private GameController gameController;
    private MapEvents mapEvents;
    public int totalHDice = 1;
    public GameObject healDice;
    public Vector3 ForceDirection;
    public float forceMagnitude;
    public GameObject DiceSpawnPosition;
    public Transform diceScorePos;
    [SerializeField] private float maxRandomForceValue;
    private int totalHealed = 0;
    private int diceInPosition = 0;
    private int diceProcessed = 0;

    private float forceX, forceY,forceZ;
    private Score scoreScript;
    private DiceRoller diceRoller;
    private bool Scoring = false;

    private Vector3 diceScorePosStart;

    private List<GameObject> DiceList = new List<GameObject>();
    

    void Start(){
        gameController = FindObjectOfType<GameController>();
        mapEvents = FindObjectOfType<MapEvents>();
        scoreScript = FindObjectOfType<Score>();
        diceRoller = FindObjectOfType<DiceRoller>();
        
    }


    void Update(){
        if(DiceList.Count == totalHDice && !Scoring){
            StartCoroutine(IterateDice());
            Scoring = true;
        }
    }

    public void processRoll(GameObject die){
        DiceList.Add(die);
    }

    public void ExecuteEvent(){
        Scoring = false;
        DiceSpawnPosition = GameObject.Find("DiceSpawn");
        diceScorePos = GameObject.Find("DiceScorePos").transform;
        diceScorePosStart = diceScorePos.position;
        for(int i = 0; i < totalHDice; i++){
            GameObject HDice = GameObject.Instantiate(healDice,DiceSpawnPosition.transform.position,Quaternion.identity);
            HDice.transform.tag = "HealthDice";

            Rigidbody rb = HDice.GetComponent<Rigidbody>();

            rb.isKinematic = false;

            rb.AddForce(ForceDirection.normalized * forceMagnitude);

            forceX = Random.Range(0,maxRandomForceValue);
            forceY = Random.Range(0,maxRandomForceValue);
            forceZ = Random.Range(0,maxRandomForceValue);

            rb.AddTorque(forceX, forceY, forceZ);
        }

        
        
    }



    public IEnumerator IterateDice(){
        foreach(GameObject dice in DiceList){

            dice.GetComponent<HealthDice>().inScoringPhase = true;

            StartCoroutine(MoveDiceToScorePos(dice));

            yield return new WaitForSeconds(1f);
        } 
    }

    private IEnumerator MoveDiceToScorePos(GameObject die){
        die.transform.parent = null;
        
        float TimeValue = 0.3f;
        float ElapsedTime = 0f;
        float diceWidth = die.GetComponent<HealthDice>().diceTemplate.diceWidth;
        float xTransformOffset = 0;
        float diceOffset = diceInPosition * diceWidth;
        

        if (diceWidth > 0){
            xTransformOffset = diceWidth/2;
        }


        Vector3 targetPos = diceScorePos.position - new Vector3(xTransformOffset,0,0);
        

        if(diceInPosition > 0){
            while(ElapsedTime < TimeValue){
                diceScorePos.position = Vector3.Lerp(diceScorePos.position, targetPos, ElapsedTime / TimeValue);
                ElapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        
            
        TimeValue = 0.3f;
        ElapsedTime = 0f;
       
        Quaternion rotateToForward = Quaternion.FromToRotation(die.GetComponent<HealthDice>().emptyFacingup.forward, -Vector3.forward);

        Quaternion rotateToText = Quaternion.FromToRotation(rotateToForward * die.GetComponent<HealthDice>().emptyFacingup.right, -Vector3.up);

        Quaternion pseudoRot = rotateToText * rotateToForward;
        Quaternion finalRot = pseudoRot * die.transform.rotation;

        while(ElapsedTime < TimeValue){
            die.transform.position = Vector3.Lerp(die.transform.position, diceScorePos.position + new Vector3(diceOffset,0,0), ElapsedTime / TimeValue);
            die.transform.rotation = Quaternion.Slerp(die.transform.rotation,finalRot,ElapsedTime / TimeValue);
            ElapsedTime += Time.deltaTime;
            yield return null;
        }
        
        die.transform.SetParent(diceScorePos);
        diceInPosition++;

        if(diceInPosition == totalHDice){
            StartCoroutine(StartScoring());
            diceInPosition = 0;
        }
    }

    private IEnumerator StartScoring(){
        foreach(GameObject dice in GameObject.FindGameObjectsWithTag("HealthDice")){
            
            StartCoroutine(ScoreDiceAnim(dice));
            yield return new WaitForSeconds(1.5f);
        } 
    }

    private IEnumerator ScoreDiceAnim(GameObject die){
        
        Vector3 diceStartPos = die.transform.position;
        Vector3 diceOffsetPos = diceStartPos + new Vector3(0,1,0);

        HealthDice diceScript = die.GetComponent<HealthDice>();
        string value = diceScript.faceName;
        DiceType type = diceScript.diceTemplate.diceType;

        float TimeValue = 0.3f;
        float ElapsedTime = 0f;
        float DownDelay = 0.4f;

        while(ElapsedTime < TimeValue){
            die.transform.position = Vector3.Lerp(die.transform.position, diceOffsetPos, ElapsedTime / TimeValue);
            ElapsedTime += Time.deltaTime;
            yield return null;
        }

        ProcessDice(value);

        yield return new WaitForSeconds(DownDelay);

        ElapsedTime = 0f;

        while(ElapsedTime < TimeValue){
            die.transform.position = Vector3.Lerp(die.transform.position, diceStartPos, ElapsedTime / TimeValue);
            ElapsedTime += Time.deltaTime;
            yield return null;
        }

        diceProcessed++;

        if (diceProcessed == totalHDice){
            
            gameController.UpdateHealth(totalHealed,false);
            totalHealed = 0;
            diceProcessed = 0;
            scoreScript.score = 0;
            scoreScript.pitch = 0.9f;
            StartCoroutine(mapEvents.EventEnded());
            diceScorePos.position = diceScorePosStart;
            scoreScript.scoreText.text = "";
            foreach(GameObject dice in GameObject.FindGameObjectsWithTag("HealthDice")){
                dice.GetComponent<HealthDice>().DestroyMe = true;
            }
            
            
        }
    }

    private void ProcessDice(string value){
        var sfx = diceRoller.GetComponentInParent<AudioSource>();


        //used to decide what audio is played
        int audioIndex = 0;

        //play audio and particle system
        sfx.clip = scoreScript.SFXs[audioIndex];
        sfx.pitch = scoreScript.pitch;
        sfx.Play();
        
        scoreScript.pitch += 0.1f;

        totalHealed += int.Parse(value);

        StartCoroutine(scoreScript.UpdateScore(int.Parse(value)));
    }

    
}
