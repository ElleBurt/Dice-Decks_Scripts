using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Text.RegularExpressions;


public class Score : MonoBehaviour
{   
    //script references
    DiceRoll dice;
    DiceRoller diceRoller;
    AtkCardHolder atkCardHolder;
    CardHolder cardHolder;
    GameController gameController;
    ScoreDice scoreDice;
    ScoreCards scoreCards;

    MapEvents mapEvents;

    //audio source
    public AudioClip[] SFXs;
    public float pitch = 0.9f;

    //text for scoring
    public TMP_Text scoreText, multiText;
    
    //references to the current dice
    private GameObject[] diceCollection;

    //if scoring dice currently
    public bool ScoringDice = false;

    

    //if the script can start basically
    public bool canStartScoring = false;

    public int score;

    //order in which dice are scored based on effects
    Dictionary<DiceType,int> effectOrder = new Dictionary<DiceType,int>(){

        {DiceType.Basic, 0},
        {DiceType.Elemental, 1},
        {DiceType.Poker, 2},
        {DiceType.Luck, 3},
        {DiceType.Multi, 4},
        {DiceType.Roulette, 5},
        {DiceType.ReRoll, 6},
    
    };


    //list of the results this round
    
    public bool shouldReroll = false;
    private bool hasRerolled = false;


    //gets scripts
    private void Awake(){
        diceRoller = FindObjectOfType<DiceRoller>();
        cardHolder = FindObjectOfType<CardHolder>();
        atkCardHolder = FindObjectOfType<AtkCardHolder>();
        gameController = FindObjectOfType<GameController>();
        mapEvents = FindObjectOfType<MapEvents>();
        scoreDice = FindObjectOfType<ScoreDice>();
        scoreCards = FindObjectOfType<ScoreCards>();
    }   

  

    private void Update(){

        //if can start scoring then check if any dice arent level, if they are then stop rolling abilities and score dice
        if(canStartScoring){
            diceCollection = GameObject.FindGameObjectsWithTag("Dice");

            bool allRollsFinished = true;

            foreach(GameObject die in gameController.DiceHeld){
                
                if (!die.GetComponent<DiceRoll>().accountedFor){

                    allRollsFinished = false;
                    break;

                }
            
            }
            

            if (allRollsFinished && !ScoringDice){
                StartCoroutine(ScoreFaces());
                ScoringDice = true;
                canStartScoring = false;
                diceRoller.canRoll = false;

            }
           
        }
        
    }

    //updates the multi with the give value
    public void UpdateMulti(int value){
        
        //basically just checks if the multi already contains a number, if not then we must set it
        multiText.text = Regex.IsMatch(multiText.text, @"\d+") ? $"x{int.Parse(Regex.Match(multiText.text, @"\d+").Value) + value}" : $"x{value}";

    }

    public IEnumerator UpdateScore(int value){

        string opperator = value >= 0 ? "+" : "-";
        
        bool hasText = Regex.IsMatch(scoreText.text, @"\d+");

        scoreText.text = hasText ? $"{score} {opperator} {Mathf.Abs(value)}" : $"{value}";

        yield return new WaitForSeconds(0.6f);

        scoreText.text = $"{score + value}";

        score += value;

    }

    public void ApplyMulti(){
        int multi = Regex.IsMatch(multiText.text, @"\d+") ? int.Parse(Regex.Match(multiText.text, @"\d+").Value) : 1;
        scoreText.text = $"{score * multi}";
    }
   

    List<GameObject> OrderedList = new List<GameObject>();
    //gets the active cards text elements and edits them accordingly    
    private IEnumerator ScoreFaces(){
        gameController.diceResults.Clear();
        pitch = 0.9f;

        if(!hasRerolled){
            score=0;
        }

        yield return new WaitForSeconds(1f);

        //orders the dice by which effect they have
        OrderedList = gameController.DiceHeld.OrderBy(dice => effectOrder[dice.GetComponent<DiceRoll>().diceTemplate.diceType]).ToList();


        StartCoroutine(scoreDice.IterateOrderedDice(OrderedList));
        
    }

    public IEnumerator ContinueDice(){
        if(shouldReroll && !hasRerolled){
            hasRerolled = true;

            yield return new WaitForSeconds(1f);

            foreach(GameObject dice in diceCollection){
                dice.transform.SetParent(dice.GetComponent<DiceRoll>().DiceSlot);
                dice.GetComponent<DiceRoll>().inScoringPhase = false;
                dice.GetComponent<DiceRoll>().hasBeenRolled = false;
                dice.GetComponent<DiceRoll>().accountedFor = false;
                canStartScoring = true;
                ScoringDice = false;
                diceRoller.callReroll(dice);
                yield return new WaitForSeconds(0.3f);
            }
            yield return new WaitForSeconds(0.2f);
            scoreDice.diceScorePos.position = scoreDice.diceScorePosStart;
        }else{

            yield return new WaitForSeconds(0.5f);
            
            StartCoroutine(scoreCards.ProcessCards(gameController.diceResults));
            hasRerolled = false;
            shouldReroll = false;
        }

    }

    

    //moves dice to respective positions in the dice holder
    public IEnumerator ReturnDice(){
        foreach(GameObject dice in gameController.DiceHeld){

            dice.GetComponent<DiceRoll>().inScoringPhase = false;
            dice.transform.SetParent(dice.GetComponent<DiceRoll>().DiceSlot);

            string slot = dice.transform.parent.name;

            Transform slotPos = GameObject.Find("diceDisplay").transform.Find(slot);

            DiceDisplay diceDisplay = slotPos.GetComponent<DiceDisplay>();

            dice.GetComponent<DiceRoll>().accountedFor = false;
            dice.GetComponent<DiceRoll>().faceName = "";
            dice.GetComponent<DiceRoll>().hasBeenRolled = false;


            diceDisplay.DiceAdded(dice,ObjectState.Sell);
            yield return new WaitForSeconds(0.2f);
        }
    }

    public IEnumerator FinishRoll(){

        StartCoroutine(mapEvents.MiniDamaged(int.Parse(scoreText.text)));

        score = 0;
        scoreText.text = "";
        OrderedList.Clear();
        

        canStartScoring = true;
        ScoringDice = false;
        hasRerolled = false;
        shouldReroll = false;

        yield return new WaitForSeconds(1f);

        scoreDice.diceScorePos.position = scoreDice.diceScorePosStart;
        
        //let player roll again
        diceRoller.canRoll = gameController.CurrentHealth <= 0 ? false : true;

    }

    //yes this is how fizzbuzz works
    private int FizzBuzz(int num){
        int res = num % 15 == 0 ? 15 
                    : num % 3 == 0 ? 3 
                    : num % 5 == 0 ? 5
                    : 0;
        return res;
    }


}
