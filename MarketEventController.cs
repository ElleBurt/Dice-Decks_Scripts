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


    void Awake(){
        mapEvents = FindObjectOfType<MapEvents>();
        gameController = FindObjectOfType<GameController>();
        diceRoller = FindObjectOfType<DiceRoller>();

        moneyText = transform.Find("Money").Find("Value").GetComponent<TMP_Text>();
        checkoutText = transform.Find("Checkout").GetChild(0).GetComponent<TMP_Text>();
        moneyText.text = $"${gameController.MoneyHeld}";
        
    }

    public void ExecuteEvent(){
        
        gameController.SetItemWeights();
        SpawnDice();
        SpawnCards();
        SpawnBoosters();
        StartCoroutine(gameController.MoveCameraTo(transform.Find("MarketTableView")));
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

            GameObject card = GameObject.Instantiate(gameController.cardPrefab, spawn.position, Quaternion.Euler(-20,180,0));
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
                StartCoroutine(gameController.MoveCameraTo(gameController.DiceView));
                StartCoroutine(mapEvents.EventEnded());
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
       
        foreach(GameObject item in itemsAtCheckout){
            if(item.transform.CompareTag("Dice")){
                diceRoller.AddDice(item.GetComponent<DiceRoll>().diceTemplate);
                itemsAtCheckout.Remove(item.transform.GetChild(0).gameObject);
                Destroy(item.transform.parent.gameObject);
            }
            else if(item.transform.CompareTag("Card")){
                cardHolder.CardAdded(item.GetComponent<CardController>().cardTemplate);
                itemsAtCheckout.Remove(item.transform.GetChild(0).gameObject);
                Destroy(item.transform.parent.gameObject);
            }
        }
        if(boosterCount > 0){
            ProcessBoosters();
        }
    }

    public void ProcessBoosters(){
        if(boosterCount > 0){
            if(itemsAtCheckout[0].CompareTag("DiceBooster")){
                mapEvents.SpawnDiceBox(true);
            }else if(itemsAtCheckout[0].CompareTag("CardBooster")){
                mapEvents.SpawnBooster(true);
                
            }

            itemsAtCheckout.RemoveAt(0);
            Destroy(itemsAtCheckout[0].transform.parent.gameObject);
            boosterCount -= 1;
        }else{
            StartCoroutine(gameController.MoveCameraTo(transform.Find("MarketTableView")));
        }
        
        
    }


    
}
