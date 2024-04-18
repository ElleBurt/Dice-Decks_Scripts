using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class MarketEventController : MonoBehaviour, EventMedium, IPointerClickHandler
{
    private GameController gameController;
    private MapEvents mapEvents;
    private DiceRoller diceRoller;
    private CardHolder cardHolder;

    public Vector3 spOffset;
    public GameObject DiceBagPrefab; //hey that has a catch to it
    public GameObject CardBoosterPrefab; 

    private TMP_Text moneyText;
    private TMP_Text checkoutText;

    private int totalValue = 0;
    public int boosterCount = 0;

    private List<GameObject> itemsAtCheckout = new List<GameObject>();

    public List<GameObject> diceSlots = new List<GameObject>();


    void Awake(){
        mapEvents = FindObjectOfType<MapEvents>();
        gameController = FindObjectOfType<GameController>();
        diceRoller = FindObjectOfType<DiceRoller>();
        cardHolder = FindObjectOfType<CardHolder>();

        moneyText = transform.Find("Money").Find("Value").GetComponent<TMP_Text>();
        checkoutText = transform.Find("Checkout").GetChild(0).GetComponent<TMP_Text>();
        moneyText.text = $"${gameController.MoneyHeld}";
        
    }

    public void ExecuteEvent(){
        
        gameController.SetItemWeights();
        SpawnDice();
        SpawnCards();
        SpawnBoosters();
        updateInventory();
        gameController.MoveCameraTo(transform.Find("MarketTableView"),Vector3.zero);
    }

    private void SpawnDice(){

        for(int i = 1; i < 5; i++){

            //gets the relative spawnPos for the dice
            Transform spawn = transform.Find($"DicePos{i}_buy");

            (Rarity, int) values = gameController.RandomItem("Dice");

            Rarity rarity = values.Item1;
            int index = values.Item2;

            //gets a random dice template from the dict of rarities and templates
            DiceTemplate diceTemp = gameController.ItemWeights[rarity].Item1[index];

            gameController.ItemWeights[rarity].Item1.RemoveAt(index);

            GameObject Dice = GameObject.Instantiate(diceTemp.dice,spawn.position,Quaternion.identity);
            Dice.transform.SetParent(spawn);
            Dice.transform.localScale /= 2.3f;
            Dice.GetComponent<DiceRoll>().diceTemplate = diceTemp;
            spawn.GetComponent<DiceBoxHover>().marketDice = true;
            Dice.GetComponent<MeshCollider>().enabled = false;
            Rigidbody rb = Dice.GetComponent<Rigidbody>();
            Dice.transform.rotation = new Quaternion(0,0,0,0);
            rb.isKinematic = true;
            
            
            foreach(Transform col in transform){
                if(col.CompareTag("DiceBoxSpawn")){
                    col.gameObject.GetComponent<DiceBoxHover>().animFin = true;
                }
            }
        }
    }

    private void SpawnCards(){

        for(float i = 1f; i < 4f; i++){

            Transform spawn = transform.Find($"CardPos{i}_buy");

            (Rarity, int) values = gameController.RandomItem("Card");

            Rarity rarity = values.Item1;
            int index = values.Item2;

            CardTemplate cardTemplate = gameController.ItemWeights[rarity].Item2[index];
            gameController.ItemWeights[rarity].Item2.RemoveAt(index);

            GameObject card = GameObject.Instantiate(gameController.cardPrefab, spawn.position + new Vector3(0,0.2f,0), Quaternion.Euler(-20,180,0));
            card.transform.SetParent(spawn);
            card.transform.localScale /= 2f;
            card.GetComponent<CardController>().cardTemplate = cardTemplate;
            card.GetComponent<CardController>().basePos = card.transform.position;
            card.GetComponent<BoxCollider>().enabled = false;
            card.GetComponent<CardController>().SetupCard();
        }
        
    }

    private void SpawnBoosters(){

        for (int i = 1; i < 3; i++){
            BoosterTemplate boosterTemplate = gameController.boosters[Random.Range(0, gameController.boosters.Count)];
            Transform spawn = transform.Find($"BoosterPos{i}_buy");
            spawn.GetComponent<BoosterHover>().boosterTemp = boosterTemplate;
            GameObject boosterPack = GameObject.Instantiate(boosterTemplate.BoosterPrefab, spawn.position + boosterTemplate.positionOffset, Quaternion.Euler(290,0,90));
            boosterPack.transform.localScale /= boosterTemplate.scaleFactor;
            boosterPack.transform.SetParent(spawn);
        }
    }


    public void OnPointerClick(PointerEventData pointerEventData){
        List<GameObject> itemsHovered = pointerEventData.hovered;

        for (int i = 0; i < itemsHovered.Count; i++){
            if(itemsHovered[i].name == "MarketStand(Clone)"){
                itemsHovered.RemoveAt(i);
            }
        }

        GameObject SelectedItem = itemsHovered[0];

        GameObject selectedChild = SelectedItem.transform.GetChild(0).gameObject;

        switch(SelectedItem.name){

            case "deskbell":
                if(totalValue <= gameController.MoneyHeld && totalValue > 0){
                    gameController.UpdateMoney(totalValue,true);
                    moneyText.text = $"${gameController.MoneyHeld}";

                    ProcessItems();
                }
            break;

            case "CowBell":
                gameController.MoveCameraTo(gameController.DiceView,Vector3.zero);
                StartCoroutine(mapEvents.EventEnded());
            break;

            case "Sell":
            break;

            default:
                GetValues values = SelectedItem.GetComponent<GetValues>();
                Dictionary<string,float> containedValues = values.GetValuesAvailable();

                int price = (int)containedValues["Buy"];
                int sell = (int)containedValues["Sell"];

                bool inCheckout = values.GetStage();

                if(!inCheckout){
                    SelectedItem.transform.position += new Vector3(7.7f,0,0);
                    totalValue += price;
                    itemsAtCheckout.Add(selectedChild);


                    boosterCount += selectedChild.transform.tag.Contains("Booster") ? 1 : 0;

                }else{
                    SelectedItem.transform.position -= new Vector3(7.7f,0,0);
                    totalValue -= price;
                    itemsAtCheckout.Remove(selectedChild);

                    boosterCount -= selectedChild.transform.tag.Contains("Booster") ? 1 : 0;

                }

                if(selectedChild.CompareTag("Card")){
                    selectedChild.GetComponent<CardController>().basePos = SelectedItem.transform.position;
                }

                values.SetStage();

                checkoutText.text = $"Checkout: ${totalValue}";
            break;
        }
    }


    private void ProcessItems(){

        List<GameObject> itemsToRemove = new List<GameObject>();
       
        foreach(GameObject item in itemsAtCheckout){

            if(item.transform.CompareTag("Dice")){
                diceRoller.AddDice(item.transform.GetComponent<DiceRoll>().diceTemplate);
                itemsToRemove.Add(item);
            }

            else if(item.transform.CompareTag("Card")){
                cardHolder.CardAdded(item.transform.GetComponent<CardController>().cardTemplate);
                itemsToRemove.Add(item);
            }

        }

        foreach(GameObject item in itemsToRemove){
            itemsAtCheckout.Remove(item);
            Destroy(item);
        }

        if(boosterCount > 0){
            ProcessBoosters();
        }

        totalValue = 0;
        checkoutText.text = $"Checkout: ${totalValue}";
    }

    public void ProcessBoosters(){

        if(boosterCount > 0){
            GameObject currentItem = itemsAtCheckout[0];

            if(currentItem.CompareTag("DiceBooster")){
                mapEvents.SpawnDiceBox(true);

            }else if(currentItem.CompareTag("CardBooster")){
                mapEvents.SpawnBooster(true);
                
            }

            
            itemsAtCheckout.Remove(currentItem);
            Destroy(currentItem);

            boosterCount -= 1;
        }else{
            gameController.MoveCameraTo(transform.Find("MarketTableView"),Vector3.zero);
            itemsAtCheckout = new List<GameObject>{};
        }

        
    }

    public void updateInventory(){
        for(int i = 0; i < gameController.DiceHeld.Count; i++){
            Transform spawn = transform.Find("SellBoard").Find($"DicePos{i+1}_sell");

            if(spawn.childCount == 0){
                GameObject dice = GameObject.Instantiate(gameController.DiceHeld[i],spawn.position,Quaternion.identity);
                dice.transform.SetParent(spawn); 
                dice.transform.localScale *= 30;
                dice.transform.rotation = new Quaternion(0,0,0,0);
            }
        }
        for(int i = 0; i < gameController.CardsHeld.Count; i++){
            Transform spawn = transform.Find("SellBoard").Find($"CardPos{i+1}_sell");

            if(spawn.childCount == 0){
                GameObject card = GameObject.Instantiate(gameController.CardsHeld[i],spawn.position + new Vector3(0,0.2f,0),Quaternion.Euler(0,170,0));
                card.transform.SetParent(spawn); 
                card.transform.localScale /= 3;
                card.GetComponent<BoxCollider>().enabled = false;
                card.GetComponent<CardController>().basePos = card.transform.position;
            }
        }
    }

    public void MoveToSellBoard(bool isAtBoard){
        if(isAtBoard){
           gameController.MoveCameraTo(transform.Find("SellBoardView"),Vector3.zero);
        }else{
           gameController.MoveCameraTo(transform.Find("MarketTableView"),Vector3.zero);
        }
        
    }

    
}
