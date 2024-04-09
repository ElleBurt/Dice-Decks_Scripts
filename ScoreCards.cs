using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Text.RegularExpressions;

public class ScoreCards : MonoBehaviour
{

    CardHolder cardHolder;
    Score score;
    DiceRoller diceRoller;
    
    private List<int> diceFaceValues = new List<int>();
    private Dictionary<CardType, Action<CardController,GameObject>> Processes;
    private TMP_Text scoreText, multiText;

    void Start(){
        cardHolder = FindObjectOfType<CardHolder>();
        score = FindObjectOfType<Score>();
        diceRoller = FindObjectOfType<DiceRoller>();
    }  

    public ScoreCards()
    {
        Processes = new Dictionary<CardType, Action<CardController,GameObject>>{
            //{CardType.CloseCall, CloseCall},
            {CardType.HighRoller, HighRoller},
            {CardType.RollingRich, RollingRich},
            {CardType.Jinx, Jinx},
            {CardType.FizzBuzz, FizzBuzz},
            //{CardType.CorruptCoins, CorruptCoins},
            //{CardType.DefenceForce, DefenceForce},
            //{CardType.PlagueDoctor, PlagueDoctor},
            //{CardType.Economics, Economics},
            //{CardType.EldritchRage, EldritchRage},
            //{CardType.MilitaryInvestment, MilitaryInvestment},
            //{CardType.StrengthRitual, StrengthRitual},
        };
    }


    public void ProcessCards(List<int> diceValues){

        scoreText = score.scoreText;
        multiText = score.multiText;

        diceFaceValues = diceValues;

        foreach(GameObject card in cardHolder.CardsHeld){

            CardController controller = card.GetComponent<CardController>();
            CardType cardType = controller.cardType;

            Processes[cardType](controller,card);
        }
        StartCoroutine(ScoreAllCards());
    }

    private void HighRoller(CardController controller, GameObject card){
        foreach(GameObject dice in diceRoller.DiceHeld){

            int num;

            if (int.TryParse(dice.GetComponent<DiceRoll>().faceName, out num)){
                if(num == dice.GetComponent<DiceRoll>().diceTemplate.hiVal){
                    controller.AdditionValue += 1;
                    
                }
            }
            
        } 
    }

    private void FizzBuzz(CardController controller, GameObject card){
        int[] FizzBuzzNums = new int[] {15,3,5};
                    
        if (FizzBuzzNums.Contains(FizzBuzz(int.Parse(scoreText.text)))){
            int MultiToAdd = FizzBuzz(int.Parse(scoreText.text));
            controller.Multiplier += MultiToAdd;
            controller.CardTriggered = true;
        }
    }
    private int FizzBuzz(int num){
        int res = num % 15 == 0 ? 15 
                    : num % 3 == 0 ? 3 
                    : num % 5 == 0 ? 5
                    : 0;
        return res;
    }

    private void RollingRich(CardController controller, GameObject card){
        foreach(GameObject dice in diceRoller.DiceHeld){

            int num;

            if (int.TryParse(dice.GetComponent<DiceRoll>().faceName, out num)){
                if(num == dice.GetComponent<DiceRoll>().diceTemplate.loVal){
                    controller.SellValue += 1;
                    controller.CardTriggered = true;
                }
            }
            
        }
    }

    private void Jinx(CardController controller, GameObject card){
        //checks the dice results list for any duplicates
        var JinxResults = diceFaceValues.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();

        //if dupes then add equal value to the score 
        if(JinxResults.Count > 0){
            int score = int.Parse(scoreText.text);

            foreach(var Jinx in JinxResults){
                controller.AdditionValue += Jinx;
            }
            controller.CardTriggered = true;
        }
    }


    IEnumerator ScoreAllCards(){
        //default placeholders
        int currentMultiplier;
        int newMultiplier = 0;
        int scoreAddition = 0;

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
                score.UpdateMulti(newMultiplier);
            }

            scoreText.text = (int.Parse(scoreText.text) + scoreAddition).ToString();
            

            if(controller.CardTriggered){
                //moves card up when scoring
                StartCoroutine(controller.ScoreCard());
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

            yield return new WaitForSeconds(1f);
        }

        

        //check if multi on card has already been edited if not then set it to this value or add it on to the existing one
        currentMultiplier = Regex.Match(multiText.text, @"\d+").Success ? int.Parse(Regex.Match(multiText.text, @"\d+").Value) : 1;
        
        //edits text, doesnt need the medieval contraption above as this is always containing some value || (future me: - honestly confused myself with this one)
        scoreText.text = (int.Parse(Regex.Match(scoreText.text, @"\d+").Value) * currentMultiplier).ToString();
        multiText.text = "";

        //returns dice to the tray
        StartCoroutine(score.ReturnDice());

        //stop scoring dice
        score.ScoringDice = false;

        //dissolves card
        StartCoroutine(score.DissolveCard());
    }


}
