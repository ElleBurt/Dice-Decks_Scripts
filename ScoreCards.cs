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
    GameController gameController;
    
    private List<int> diceFaceValues = new List<int>();
    private Dictionary<CardType, Action<CardController,GameObject>> Processes;
    private Dictionary<CardClass, Action<CardController,GameObject>> EffectProcesses;
    private List<CardType> ignoreCardTypes = new List<CardType>();
    private List<CardClass> ignoreCardClass = new List<CardClass>();
    private TMP_Text scoreText, multiText;

    void Start(){
        cardHolder = FindObjectOfType<CardHolder>();
        score = FindObjectOfType<Score>();
        diceRoller = FindObjectOfType<DiceRoller>();
        gameController = FindObjectOfType<GameController>();
    }  

    public ScoreCards()
    {
        Processes = new Dictionary<CardType, Action<CardController,GameObject>>{
            {CardType.HighRoller, HighRoller},
            {CardType.RollingRich, RollingRich},
            {CardType.Jinx, Jinx},
            {CardType.FizzBuzz, FizzBuzz},
            //{CardType.PlagueDoctor, PlagueDoctor},
            
        };

        ignoreCardTypes = new List<CardType>{
            {CardType.CorruptCoins},
            {CardType.CloseCall},
            {CardType.Economics},
            {CardType.MilitaryInvestment},
            {CardType.DefenceForce},
            {CardType.EldritchRage},
            {CardType.StrengthRitual},
            
        };

        EffectProcesses = new Dictionary<CardClass, Action<CardController,GameObject>>{
            {CardClass.Blessed, Blessed},
            {CardClass.Cursed, Cursed},
            {CardClass.Celestial, Celestial},
            {CardClass.BloodShard, BloodShard},
            {CardClass.Shield, Shield},
            {CardClass.Upgrade, Upgrade},
        };

        ignoreCardClass = new List<CardClass>{
            {CardClass.Standard},
        };
    }


    public IEnumerator ProcessCards(List<int> diceValues){

        scoreText = score.scoreText;
        multiText = score.multiText;

        diceFaceValues = diceValues;

        foreach(GameObject card in gameController.CardsHeld){

            CardController controller = card.GetComponent<CardController>();
            CardType cardType = controller.cardType;
            CardClass cardClass = controller.cardTemplate.cardClass;


            if(!ignoreCardTypes.Contains(cardType)){
                Processes[cardType](controller,card);
            }
            
            if(!ignoreCardClass.Contains(cardClass)){
                EffectProcesses[cardClass](controller,card);
            }

            StartCoroutine(CardTriggered(card));
            yield return new WaitForSeconds(0.75f);
        }
        //check if multi on card has already been edited if not then set it to this value or add it on to the existing one
        int currentMultiplier = Regex.Match(multiText.text, @"\d+").Success ? int.Parse(Regex.Match(multiText.text, @"\d+").Value) : 1;

        //edits text, doesnt need the medieval contraption above as this is always containing some value || (future me: - honestly confused myself with this one)
        scoreText.text = (int.Parse(Regex.Match(scoreText.text, @"\d+").Value) * currentMultiplier).ToString();
        multiText.text = "";

        //returns dice to the tray
        StartCoroutine(score.ReturnDice());

        //stop scoring dice
        score.ScoringDice = false;

        yield return new WaitForSeconds(0.35f);
        //dissolves card
        StartCoroutine(score.DissolveCard());
    }

    private void HighRoller(CardController controller, GameObject card){
        foreach(GameObject dice in gameController.DiceHeld){

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
        foreach(GameObject dice in gameController.DiceHeld){

            int num;

            if (int.TryParse(dice.GetComponent<DiceRoll>().faceName, out num)){
                if(num == dice.GetComponent<DiceRoll>().diceTemplate.loVal){
                    controller.SellValue += 1;
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
        }
    }




    //-----These Are Triggered Or Updated By External Actions-----

    public void ScoreAnim(CardType type){
        
        foreach(GameObject card in gameController.CardsHeld){
            if(card.GetComponent<CardController>().cardType == type){
                StartCoroutine(card.GetComponent<CardController>().ScoreCard());
            }
        }
        
    }

   

    

    










    private void Blessed(CardController controller, GameObject card){
        gameController.UpdateHealth(1,false);
    }

    private void Cursed(CardController controller, GameObject card){
        gameController.UpdateHealth(1,true);
    }

    private void Celestial(CardController controller, GameObject card){
        if(UnityEngine.Random.Range(1,3) == 1){
            controller.AdditionValue += 1;
        }else{
            controller.Multiplier += 1;
        }
    }

    private void BloodShard(CardController controller, GameObject card){
        controller.BloodShardTick(true);
    }

    private void Shield(CardController controller, GameObject card){
        
    }

    private void Upgrade(CardController controller, GameObject card){
        
    }











    IEnumerator CardTriggered(GameObject card){
        int newMultiplier = 0;
        int scoreAddition = 0;

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

        if(newMultiplier > 0 || scoreAddition > 0){
            StartCoroutine(controller.ScoreCard());
        }
        


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

    

    



}
