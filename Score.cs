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

    MapEvents mapEvents;

    //audio source
    AudioSource sfx;
    public AudioClip[] SFXs;
    float pitch = 0.9f;

    //particle system 
    private ParticleSystem ResultsFX;

    //text for scoring
    public TMP_Text scoreText, multiText;
    
    //references to the current dice
    private GameObject[] diceCollection;

    //if scoring dice currently
    public bool ScoringDice = false;

    //shows the dice score in gui above it
    public Vector3 GuiOffset = new Vector3(0f, 2f, 0f);
    public GameObject FaceValueDisplay;

    //list of the results this round
    List<int> diceResults = new List<int>();

    //if the script can start basically
    public bool canStartScoring = false;

    //order in which dice are scored based on effects
    Dictionary<string,int> effectOrder = new Dictionary<string,int>(){
        {"Basic", 0},
        {"Multi", 1},
        {"Roulette", 2},
    };

    //gets scripts
    private void Awake(){
        diceRoller = FindObjectOfType<DiceRoller>();
        cardHolder = FindObjectOfType<CardHolder>();
        atkCardHolder = FindObjectOfType<AtkCardHolder>();
        gameController = FindObjectOfType<GameController>();
        mapEvents = FindObjectOfType<MapEvents>();
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
                StartCoroutine(ScoreDice());
                ScoringDice = true;
                canStartScoring = false;
                diceRoller.canRoll = false;

            }
           
        }
        
    }

    //take the value provided, the effect of the dice and the dice itself and update the scores accordingly
    private void UpdateScore(string value, string effect, GameObject die){

        value = value.Contains(".") ? value.Substring(0, value.Length - 4) : value;

        //gets particle system
        ResultsFX = die.GetComponentInChildren<ParticleSystem>();
        var trailsFX = ResultsFX.trails;
        var rendererFX = ResultsFX.GetComponent<Renderer>();

        //gets sound source
        sfx = diceRoller.GetComponentInParent<AudioSource>();
        
        //gets script on the dice
        DiceRoll dice = die.GetComponent<DiceRoll>();

        //used to decide what audio is played
        int audioIndex = 0;

        //sets score equal to the value displayed by text
        int score = int.Parse(Regex.Match(scoreText.text, @"\d+").Value);

        //check the effects and applies the respective calculations
        switch(effect){
            
            //if multi, gets the number after the x and adds it to the multi
            case "Multi":

                if(Regex.Match(value, @"\D").Success){
                    UpdateMulti(int.Parse(Regex.Match(value, @"\d+").Value));
                    audioIndex = 1;
                }else{
                    diceResults.Add(int.Parse(value));
                    score += int.Parse(value);
                    scoreText.text = score.ToString();
                }
                
                
            break;

            //checks which face rolled on roulette dice 
            case "Roulette":
                Debug.Log(value);
                switch(value){
                    case "RC":
                        value = "Even";
                        //checks if even
                        if(score % 2 == 0){
                            UpdateMulti(4);
                        }
                    break;
                    case "BC":
                        value = "Odd";
                        //checks if odd
                        if(score % 2 != 0){
                            UpdateMulti(4);
                        }
                    break;
                    case "GC":
                        value = "Any";
                        UpdateMulti(4);
                    break;
                }
            break;

            //if effect is Basic then just score normally
            default:
                diceResults.Add(int.Parse(value));
                //sets the scoreText to the new score
                score += int.Parse(value);
                scoreText.text = score.ToString();

            break;
        }

        //show the value in small gui popup
       // StartCoroutine(DisplayFaceValue(value,die));

        //play audio and particle system
        sfx.clip = SFXs[audioIndex];
        sfx.pitch = pitch;
        sfx.Play();
        ResultsFX.Play();
        pitch += 0.1f;

    }

    //updates the multi with the give value
    private void UpdateMulti(int value){

        multiText.text = Regex.IsMatch(multiText.text, @"\d+") ? "x" + (int.Parse(Regex.Match(multiText.text, @"\d+").Value) + value).ToString() : "x" + value.ToString();

    }
    
    //gets the active cards text elements and edits them accordingly    
    IEnumerator ScoreDice(){
        diceResults.Clear();
        pitch = 0.9f;

        atkCardHolder.DrawCard();

        scoreText = atkCardHolder.ActiveCard.CardBase.transform.Find("Canvas").Find("Attack").GetComponent<TMP_Text>();
        multiText = atkCardHolder.ActiveCard.CardBase.transform.Find("Canvas").Find("Multi").GetComponent<TMP_Text>();

        

        scoreText.text = "0";

        yield return new WaitForSeconds(1f);

        //orders the dice by which effect they have
        List<GameObject> OrderedList = diceRoller.DiceHeld.OrderBy(dice => effectOrder[dice.GetComponent<DiceRoll>().diceTemplate.EffectType]).ToList();
        
        //goes through list and gets struct values
        foreach(GameObject dice in OrderedList){
            DiceRoll diceScript = dice.GetComponent<DiceRoll>();
            string value = diceScript.faceName;
            string effect = diceScript.diceTemplate.EffectType;

            UpdateScore(value, effect, dice);

            yield return new WaitForSeconds(0.7f);
        } 

        yield return new WaitForSeconds(0.2f);

        //now dice are scored we can score the cards
        StartCoroutine(ScoreCards());
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

    //spawns a world space gui above the dice with the respective number scored
    IEnumerator DisplayFaceValue(string faceText, GameObject dice){

        GameObject textPopup = Instantiate(FaceValueDisplay, dice.GetComponent<DiceRoll>().emptyFacingup.position + GuiOffset, Quaternion.identity, dice.GetComponent<DiceRoll>().emptyFacingup);
        textPopup.GetComponentInChildren<TMP_Text>().text = faceText;
        yield return new WaitForSeconds(1f);
        Destroy(textPopup);

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

            card.GetComponent<MeshRenderer>().materials[1].SetFloat("_Alpha", CurrentValueInvert);

            if(Elapsed < TimeValue - 0.2f && !swordSpawned){
                swordSpawned = true;
                GameObject weapon = GameObject.Instantiate(atkCardHolder.ActiveCard.Weapon, atkCardHolder.ActiveCard.CardBase.transform.position - new Vector3(-0.7f, 1.55f, -0.17f), Quaternion.identity);
            }

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
