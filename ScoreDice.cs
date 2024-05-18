using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Text.RegularExpressions;

public class ScoreDice : MonoBehaviour
{
    DiceRoller diceRoller;
    Score scoreScript;
    MapEvents events;
    GenMapV2 genMapV2;

    private Dictionary<DiceType, Action<string>> Processes;
    private MiniScript SelectedEncounter;

    public Transform diceScorePos;
    private List<GameObject> DiceBeingScored;
    public Vector3 diceScorePosStart;
    private int diceInPosition = 0;
    private int diceProcessed = 0;

    public ScoreDice()
    {
        Processes = new Dictionary<DiceType, Action<string>>{
            {DiceType.Basic, Basic},
            {DiceType.Multi, Multi},
            {DiceType.Roulette, Roulette},
            {DiceType.Poker, Poker},
            {DiceType.Explosive, Explosive},
            {DiceType.Elemental, Elemental},
            {DiceType.ReRoll, ReRoll},
            {DiceType.Luck, Luck},
        };
    }

    private void Start(){
        diceRoller = FindObjectOfType<DiceRoller>();
        scoreScript = FindObjectOfType<Score>();
        events = FindObjectOfType<MapEvents>();
        genMapV2 = FindObjectOfType<GenMapV2>();
        diceScorePosStart = diceScorePos.position;
       
    }

    public IEnumerator IterateOrderedDice(List<GameObject> orderedDice){
        DiceBeingScored = new List<GameObject>(orderedDice);
        int psuedoDIP = 0;
        foreach(GameObject dice in orderedDice){

            dice.GetComponent<DiceRoll>().inScoringPhase = true;

            StartCoroutine(MoveDiceToScorePos(dice));

            yield return new WaitForSeconds(1f);
        } 
    }

    private float lastDiceWidth = 0;
    private IEnumerator MoveDiceToScorePos(GameObject die){
        die.transform.parent = null;
        
        float TimeValue = 0.3f;
        float ElapsedTime = 0f;
        float diceWidth = (lastDiceWidth + die.GetComponent<DiceRoll>().diceTemplate.diceWidth)/2;
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
       
        Quaternion rotateToForward = Quaternion.FromToRotation(die.GetComponent<DiceRoll>().emptyFacingup.forward, -Vector3.forward);

        Quaternion rotateToText = Quaternion.FromToRotation(rotateToForward * die.GetComponent<DiceRoll>().emptyFacingup.right, -Vector3.up);

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

        if(diceInPosition == DiceBeingScored.Count){
            StartCoroutine(StartScoring());
            diceInPosition = 0;
        }

        lastDiceWidth = die.GetComponent<DiceRoll>().diceTemplate.diceWidth;
    }

    private IEnumerator StartScoring(){
        foreach(GameObject dice in DiceBeingScored){
                
            StartCoroutine(ScoreDiceAnim(dice));
            yield return new WaitForSeconds(1.5f);
        } 
    }

    private IEnumerator ScoreDiceAnim(GameObject die){
        lastDiceWidth = 0;
        
        Vector3 diceStartPos = die.transform.position;
        Vector3 diceOffsetPos = diceStartPos + new Vector3(0,1,0);

        DiceRoll diceScript = die.GetComponent<DiceRoll>();
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

        ProcessDice(type,die,value);

        yield return new WaitForSeconds(DownDelay);

        ElapsedTime = 0f;

        while(ElapsedTime < TimeValue){
            die.transform.position = Vector3.Lerp(die.transform.position, diceStartPos, ElapsedTime / TimeValue);
            ElapsedTime += Time.deltaTime;
            yield return null;
        }

        diceProcessed++;

        if (diceProcessed == DiceBeingScored.Count){
            StartCoroutine(scoreScript.ContinueDice());
            diceProcessed=0;
        }
    }

    private void ProcessDice(DiceType diceType, GameObject die, string value){
        SelectedEncounter = events.SelectedEncounter.GetComponent<MiniScript>();

        //gets sound source
        var sfx = diceRoller.GetComponentInParent<AudioSource>();


        //used to decide what audio is played
        int audioIndex = 0;

        //play audio and particle system
        sfx.clip = scoreScript.SFXs[audioIndex];
        sfx.pitch = scoreScript.pitch;
        sfx.Play();
        
        scoreScript.pitch += 0.1f;

        Processes[diceType](value);
    }


    private void Basic(string value){
        scoreScript.diceResults.Add(int.Parse(value));
        //sets the scoreText to the new score
        StartCoroutine(scoreScript.UpdateScore(int.Parse(value)));
    }

    private void Multi(string value){

        if(Regex.Match(value, @"\D").Success){
            scoreScript.UpdateMulti(int.Parse(Regex.Match(value, @"\d+").Value));
        }else{
            Basic(value);
        }

    }



    private void Roulette(string value){
        switch(value){
            case "Rr":
                value = "Even";
                //checks if even
                if(scoreScript.score % 2 == 0){
                    scoreScript.UpdateMulti(4);
                }
            break;
            case "Br":
                value = "Odd";
                //checks if odd
                if(scoreScript.score % 2 != 0){
                    scoreScript.UpdateMulti(4);
                }
            break;
            case "Gr":
                value = "Any";
                scoreScript.UpdateMulti(4);
            break;
        }
    }



    private void Poker(string value){
        switch(value){
            case "Joker":
                joker();
            break;
            
            case "Ace":
                Basic("11");
            break;

            default:
                Basic("10");
            break;
        }
    }

    private void joker(){
        switch(UnityEngine.Random.Range(1,4)){
            case 1:
                int moneyStolen = UnityEngine.Random.Range(1,6);
                genMapV2.UpdateMoney(moneyStolen, true);
            break;

            case 2:
                int scoreRemoved = UnityEngine.Random.Range(1,10);
                StartCoroutine(scoreScript.UpdateScore(-scoreRemoved));
            break;

            default:
                int healthStolen = UnityEngine.Random.Range(1,6);
                genMapV2.UpdateHealth(healthStolen, true);
            break;

        }
    }



    private void Explosive(string value){
        if(value == "Explosion"){

        }else{
            Basic(value);
        }
    }



    private void Elemental(string value){
        SelectedEncounter.EffectInflicted = value;
        switch(value){
            case "Poison":
                SelectedEncounter.TickDamage = 3;
            break;
            case "Fire":
                SelectedEncounter.TickDamage = 2;
            break;
            case "Freeze":
                SelectedEncounter.TickDamage = 1;
            break;
        }
    }



    private void ReRoll(string value){
        if(value=="ReRoll"){
            scoreScript.shouldReroll = true;
        }else{
            Basic(value);
        }
    }



    private void Luck(string value){
        switch(value){
            case "Voodoo":
                int healthStolen = UnityEngine.Random.Range(1,4);
                genMapV2.UpdateHealth(healthStolen, true);
                StartCoroutine(scoreScript.UpdateScore(-healthStolen));
            break;
            case "Clover":
                int moneyGiven = UnityEngine.Random.Range(1,4);
                genMapV2.UpdateMoney(moneyGiven, false);
                Basic($"{moneyGiven}");
            break;
        }
    }
}
