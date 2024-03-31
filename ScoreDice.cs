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

    void Start(){
        diceRoller = FindObjectOfType<DiceRoller>();
        scoreScript = FindObjectOfType<Score>();
    }

    private Dictionary<DiceType, Action<GameObject, string>> Processes;

    public ScoreDice()
    {
        Processes = new Dictionary<DiceType, Action<GameObject, string>>{
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

        Processes[diceType](die,value);
    }

    private void Basic(GameObject die, string value){
        scoreScript.diceResults.Add(int.Parse(value));
        //sets the scoreText to the new score
        scoreScript.score += int.Parse(value);
        scoreScript.scoreText.text = scoreScript.score.ToString();
    }
    private void Multi(GameObject die, string value){
        if(Regex.Match(value, @"\D").Success){
            scoreScript.UpdateMulti(int.Parse(Regex.Match(value, @"\d+").Value));
        }else{
            scoreScript.diceResults.Add(int.Parse(value));
            scoreScript.score += int.Parse(value);
            scoreScript.scoreText.text = scoreScript.score.ToString();
        }
    }
    private void Roulette(GameObject die, string value){
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
    private void Poker(GameObject die, string value){

    }
    private void Explosive(GameObject die, string value){

    }
    private void Elemental(GameObject die, string value){

    }
    private void ReRoll(GameObject die, string value){

    }
    private void Luck(GameObject die, string value){

    }
}
