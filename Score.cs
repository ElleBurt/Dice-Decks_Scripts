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
        {DiceType.Poker, 0},
        {DiceType.Basic, 1},
        {DiceType.Multi, 2},
        {DiceType.Elemental, 3},
        {DiceType.Roulette, 4},
        {DiceType.ReRoll, 5},
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

            foreach(GameObject die in diceRoller.DiceHeld){
                
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
        multiText.text = Regex.IsMatch(multiText.text, @"\d+") ? $"x{int.Parse(Regex.Match(multiText.text, @"\d+").Value)}" : $"x{value}";

    }

    
    //gets the active cards text elements and edits them accordingly    
    IEnumerator ScoreFaces(){
        gameController.diceResults.Clear();
        pitch = 0.9f;

        if(!hasRerolled){
            atkCardHolder.DrawCard();
            score=0;
        }
        

        scoreText = atkCardHolder.ActiveCard.CardBase.transform.Find("Canvas").Find("Attack").GetComponent<TMP_Text>();
        multiText = atkCardHolder.ActiveCard.CardBase.transform.Find("Canvas").Find("Multi").GetComponent<TMP_Text>();

        yield return new WaitForSeconds(1f);

        //orders the dice by which effect they have
        List<GameObject> OrderedList = diceRoller.DiceHeld.OrderBy(dice => effectOrder[dice.GetComponent<DiceRoll>().diceTemplate.diceType]).ToList();

        //goes through list and gets SO values
        foreach(GameObject dice in OrderedList){
            DiceRoll diceScript = dice.GetComponent<DiceRoll>();
            string value = diceScript.faceName;
            DiceType type = diceScript.diceTemplate.diceType;

            scoreDice.ProcessDice(type,dice,value);

            yield return new WaitForSeconds(0.7f);
        } 

        if(shouldReroll && !hasRerolled){
            hasRerolled = true;

            yield return new WaitForSeconds(1f);

            foreach(GameObject dice in diceCollection){
                dice.GetComponent<DiceRoll>().hasBeenRolled = false;
                dice.GetComponent<DiceRoll>().accountedFor = false;
                canStartScoring = true;
                ScoringDice = false;
                diceRoller.callReroll(dice);
                yield return new WaitForSeconds(0.3f);
            }
        }else{
            StartCoroutine(scoreCards.ProcessCards(gameController.diceResults));
            hasRerolled = false;
            shouldReroll = false;
        }

    }


    

    //moves dice to respective positions in the dice holder
    public IEnumerator ReturnDice(){
        foreach(GameObject dice in diceRoller.DiceHeld){

            string slot = dice.transform.parent.name;

            Transform slotPos = GameObject.Find("diceDisplay").transform.Find(slot);

            DiceDisplay diceDisplay = slotPos.GetComponent<DiceDisplay>();

            dice.GetComponent<DiceRoll>().accountedFor = false;
            dice.GetComponent<DiceRoll>().faceName = "";
            dice.GetComponent<DiceRoll>().hasBeenRolled = false;


            diceDisplay.DiceAdded(dice);
            yield return new WaitForSeconds(0.2f);
        }
    }

    //fades text elements, adjusts step value on the disolve shader and spawns the weapon
    public IEnumerator DissolveCard(){
        float StartValue = 0f;
        float EndValue = 1f;
        float TimeValue = 1f;
        float Elapsed = 0f;

        GameObject card = atkCardHolder.ActiveCard.CardBase;

        Material imageAlpha = new Material(card.transform.Find("Canvas").Find("Icon").GetComponent<Image>().material);
        TMP_Text atkText = card.transform.Find("Canvas").Find("Attack").GetComponent<TMP_Text>();
        TMP_Text multiText = card.transform.Find("Canvas").Find("Multi").GetComponent<TMP_Text>();

        card.transform.Find("Canvas").Find("Icon").GetComponent<Image>().material = imageAlpha;

        while(Elapsed < TimeValue){
            float CurrentValue = Mathf.Lerp(StartValue, EndValue, Elapsed / TimeValue);
            float CurrentValueInvert = Mathf.Lerp(EndValue, StartValue, Elapsed / TimeValue);

            Elapsed += Time.deltaTime;
            card.GetComponent<MeshRenderer>().materials[0].SetFloat("_Step", CurrentValue);

            
            imageAlpha.SetFloat("_Alpha", CurrentValueInvert);

            atkText.color = new Color(atkText.color.r, atkText.color.g, atkText.color.b, CurrentValueInvert);

            multiText.color = new Color(multiText.color.r, multiText.color.g, multiText.color.b, CurrentValueInvert);

            yield return null;
        }

        StartCoroutine(mapEvents.MiniDamaged(int.Parse(atkText.text)));

        Destroy(card);

        atkCardHolder.ActiveCard = null;

        yield return new WaitForSeconds(1f);
        
        //let player roll again
        diceRoller.canRoll = true;

        if(atkCardHolder.lastCard){
            diceRoller.canRoll = false;
        }
        
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
