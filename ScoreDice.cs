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

    void Start(){
        diceRoller = FindObjectOfType<DiceRoller>();
        scoreScript = FindObjectOfType<Score>();
        events = FindObjectOfType<MapEvents>();
        gameController = FindObjectOfType<GameController>();
       
    }

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

    public void ProcessDice(DiceType diceType, GameObject die, string value){
        SelectedEncounter = events.SelectedEncounter.GetComponent<MiniScript>();
         //gets particle system
        var ResultsFX = die.GetComponentInChildren<ParticleSystem>();
        var trailsFX = ResultsFX.trails;
        var rendererFX = ResultsFX.GetComponent<Renderer>();

        //gets sound source
        var sfx = diceRoller.GetComponentInParent<AudioSource>();
        
        //gets script on the dice
        DiceRoll dice = die.GetComponent<DiceRoll>();

        //used to decide what audio is played
        int audioIndex = 0;

        //play audio and particle system
        sfx.clip = scoreScript.SFXs[audioIndex];
        sfx.pitch = scoreScript.pitch;
        sfx.Play();
        ResultsFX.Play();
        scoreScript.pitch += 0.1f;

        Processes[diceType](value);
    }

    private void Basic(string value){
        scoreScript.diceResults.Add(int.Parse(value));
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
