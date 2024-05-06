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
    GameController gameController;

    private Dictionary<DiceType, Action<string>> Processes;
    private MiniScript SelectedEncounter;

    public Transform diceScorePos;
    private List<GameObject> DiceBeingScored = new List<GameObject>();
    public Vector3 diceScorePosStart;
    private int diceInPosition = 0;

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
        gameController = FindObjectOfType<GameController>();
        diceScorePosStart = diceScorePos.position;
       
    }

    private void Update(){
        if(diceInPosition == DiceBeingScored.Count){

            diceInPosition = 0;

            foreach(GameObject dice in DiceBeingScored){
                
                DiceRoll diceScript = dice.GetComponent<DiceRoll>();
                string value = diceScript.faceName;
                DiceType type = diceScript.diceTemplate.diceType;

                ProcessDice(type,dice,value);
            } 

        }
    }

    public IEnumerator IterateOrderedDice(List<GameObject> orderedDice){
        DiceBeingScored = orderedDice;
        foreach(GameObject dice in orderedDice){

            dice.GetComponent<DiceRoll>().inScoringPhase = true;

            StartCoroutine(MoveDiceToScorePos(dice));

            yield return new WaitForSeconds(1.5f);
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

    private IEnumerator MoveDiceToScorePos(GameObject die){
        die.transform.parent = null;
        
        float TimeValue = 0.3f;
        float ElapsedTime = 0f;
        float diceWidth = die.GetComponent<DiceRoll>().diceTemplate.diceWidth;
        float xTransformOffset = 0f;
        float diceOffset = diceInPosition * diceWidth;
        

        if(diceInPosition % 2 != 0 && diceInPosition > 0){
            xTransformOffset = diceWidth/2f;
        }else if(diceInPosition > 0){
            xTransformOffset = diceWidth;
        }

        Vector3 targetPos = diceScorePos.position - new Vector3(xTransformOffset,0,0);
        

        if(diceInPosition > 0){
            while(ElapsedTime < TimeValue){
                diceScorePos.position = Vector3.Lerp(diceScorePos.position, targetPos, ElapsedTime / TimeValue);
                ElapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        
            
        TimeValue = 1f;
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
    }

    private void Basic(string value){
        gameController.diceResults.Add(int.Parse(value));
        //sets the scoreText to the new score
        scoreScript.score += int.Parse(value);
        scoreScript.scoreText.text = scoreScript.score.ToString();
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
            case "RC":
                value = "Even";
                //checks if even
                if(scoreScript.score % 2 == 0){
                    scoreScript.UpdateMulti(4);
                }
            break;
            case "BC":
                value = "Odd";
                //checks if odd
                if(scoreScript.score % 2 != 0){
                    scoreScript.UpdateMulti(4);
                }
            break;
            case "GC":
                value = "Any";
                scoreScript.UpdateMulti(4);
            break;
        }
    }



    private void Poker(string value){
        switch(value){
            case "Joker":
                Basic("0");
            break;
            
            case "Ace":
                Basic("11");
            break;

            default:
                Basic("10");
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
            break;
            case "Luck":
            break;
        }
    }
}
