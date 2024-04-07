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
    public List<int> diceResults = new List<int>();
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
        multiText.text = Regex.IsMatch(multiText.text, @"\d+") ? "x" + (int.Parse(Regex.Match(multiText.text, @"\d+").Value) + value).ToString() : "x" + value.ToString();

    }

    
    //gets the active cards text elements and edits them accordingly    
    IEnumerator ScoreFaces(){
        diceResults.Clear();
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
            StartCoroutine(ScoreCards());
            hasRerolled = false;
            shouldReroll = false;
        }

    }


    //scores cards based on their struct
    IEnumerator ScoreCards(){

        //default placeholders
        int currentMultiplier;
        int newMultiplier = 0;
        int scoreAddition = 0;

        //loops through cards in possession 
        foreach(GameObject card in cardHolder.CardsHeld){

            CardController controller = card.GetComponent<CardController>();

            string CardName = controller.cardTemplate.name;

            //switches through the card name
            switch(CardName){
                case "The Jinx":

                    //checks the dice results list for any duplicates
                    var JinxResults = diceResults.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();

                    //if dupes then add equal value to the score 
                    if(JinxResults.Count > 0){
                        int score = int.Parse(scoreText.text);

                        foreach(var Jinx in JinxResults){
                            controller.AdditionValue += Jinx;
                        }
                        controller.CardTriggered = true;
                    }
                break;

                case "FizzBuzz":
                    //does what it says really, just fizzbuzz-core

                    int[] FizzBuzzNums = new int[] {15,3,5};
                    
                    if (FizzBuzzNums.Contains(FizzBuzz(int.Parse(scoreText.text)))){
                        int MultiToAdd = FizzBuzz(int.Parse(scoreText.text));
                        controller.Multiplier += MultiToAdd;
                        controller.CardTriggered = true;
                    }



                break;

                case "High Roller":
                    //checks if dice rolled highest face value, if it did accumulate value on cards multi

                    bool highestMatched = false;
                    foreach(GameObject dice in diceRoller.DiceHeld){
                        int num;
                        if (int.TryParse(dice.GetComponent<DiceRoll>().faceName, out num)){
                            if(num == dice.GetComponent<DiceRoll>().diceTemplate.hiVal){
                                controller.Multiplier += 1;
                                highestMatched = true;
                                controller.CardTriggered = true;
                            }
                        }
                        
                    } 
                    //if didnt roll highest value, halves the multi.
                    if(!highestMatched){
                        controller.Multiplier = Mathf.RoundToInt(controller.Multiplier / 2);
                    }
                    
                break;
                case "Rich Rolled":
                    //similar to above but with the lowest values, and adds to sell value instead, Basically an investment card
                    foreach(GameObject dice in diceRoller.DiceHeld){
                        int num;
                        if (int.TryParse(dice.GetComponent<DiceRoll>().faceName, out num)){
                            if(num == dice.GetComponent<DiceRoll>().diceTemplate.loVal){
                                controller.SellValue += 1;
                                controller.CardTriggered = true;
                            }
                        }
                        
                    }
                break;
            }
        } 

        //once all effects are applied to the cards values, process them
        foreach(GameObject card in cardHolder.CardsHeld){

            //script on card
            CardController controller = card.GetComponent<CardController>();

            newMultiplier += controller.Multiplier;
            scoreAddition += controller.AdditionValue;

            controller.additionText.text = "+" + (controller.AdditionValue).ToString();
            controller.multiText.text = "x" + (controller.Multiplier).ToString();
            controller.sellText.text = "$" + (controller.SellValue).ToString();

            if(newMultiplier > 0){
                UpdateMulti(newMultiplier);
            }

            scoreText.text = (int.Parse(scoreText.text) + scoreAddition).ToString();
            

            if(controller.CardTriggered){
                //moves card up when scoring
                StartCoroutine(controller.ScoreCard());

                yield return new WaitForSeconds(0.3f);
            }
            
            controller.CardTriggered = false;


            //if card isnt meant to retain values then clear them
            if(controller.cardTemplate.shouldReset){
                controller.Multiplier = 0;
                controller.AdditionValue = 0;
            }

            controller.additionText.text = "+" + (controller.AdditionValue).ToString();
            controller.multiText.text = "x" + (controller.Multiplier).ToString();
            controller.sellText.text = "$" + (controller.SellValue).ToString();
        }

        yield return new WaitForSeconds(0.2f);

        //check if multi on card has already been edited if not then set it to this value or add it on to the existing one
        if(Regex.Match(multiText.text, @"\d+").Success){

            currentMultiplier = int.Parse(Regex.Match(multiText.text, @"\d+").Value);

        }else{
            currentMultiplier = 1;
        }
        
        //edits text, doesnt need the medieval contraption above as this is always containing some value
        scoreText.text = (int.Parse(Regex.Match(scoreText.text, @"\d+").Value) * currentMultiplier).ToString();
        multiText.text = "";

        //returns dice to the tray
        StartCoroutine(ReturnDice());

        //stop scoring dice
        ScoringDice = false;

        //dissolves card
        StartCoroutine(DissolveCard());
    }

    //moves dice to respective positions in the dice holder
    IEnumerator ReturnDice(){
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
    IEnumerator DissolveCard(){
        float StartValue = 0f;
        float EndValue = 1f;
        float TimeValue = 1f;
        float Elapsed = 0f;
        bool swordSpawned = false;

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

        yield return new WaitForSeconds(3f);
        
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
