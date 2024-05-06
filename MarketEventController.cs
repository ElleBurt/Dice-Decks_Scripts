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

    //private int totalValue = 0;
    public int boosterCount = 0;

    private List<GameObject> itemsAtCheckout = new List<GameObject>();

    public List<GameObject> diceSlots = new List<GameObject>();


    void Awake(){
        mapEvents = FindObjectOfType<MapEvents>();
        gameController = FindObjectOfType<GameController>();
        diceRoller = FindObjectOfType<DiceRoller>();
        cardHolder = FindObjectOfType<CardHolder>();
    }

    public void ExecuteEvent(){
        
        gameController.SetItemWeights();
        SpawnDice();
        SpawnCards();
        SpawnBoosters();
        gameController.MoveCameraTo(transform.Find("MarketTableView"),Vector3.zero,GameController.currentStage.Market);
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
            Dice.GetComponent<DiceRoll>().diceTemplate = diceTemp;
            spawn.GetComponent<DiceDisplay>().DiceAdded(Dice, ObjectState.Buy);
            Dice.GetComponent<MeshCollider>().enabled = false;
            Rigidbody rb = Dice.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            Dice.transform.SetParent(spawn);
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
            card.GetComponent<CardController>().cardTemplate = cardTemplate;
            card.GetComponent<CardController>().basePos = spawn.position;
            card.GetComponent<BoxCollider>().enabled = false;
            card.GetComponent<CardController>().SetupCard(ObjectState.Buy);
            card.GetComponent<CardController>().setState(ObjectState.Buy);
        }
        
    }

    private void SpawnBoosters(){

        for (int i = 1; i < 3; i++){
            BoosterTemplate boosterTemplate = gameController.boosters[Random.Range(0, gameController.boosters.Count)];
            Transform spawn = transform.Find($"BoosterPos{i}_buy");
            spawn.GetComponent<BoosterHover>().boosterTemp = boosterTemplate;
            GameObject boosterPack = GameObject.Instantiate(boosterTemplate.BoosterPrefab, spawn.position + boosterTemplate.positionOffset, Quaternion.Euler(290,0,90));
            boosterPack.transform.SetParent(spawn);
            spawn.GetComponent<BoosterHover>().Booster = boosterPack.transform;
            spawn.GetComponent<BoosterHover>().baseRot = boosterPack.transform.rotation;
        }
    }
    
     
    public void OnPointerClick(PointerEventData pointerEventData){
        List<GameObject> itemsHovered = pointerEventData.hovered;

        for (int i = 0; i < itemsHovered.Count; i++){
            if(itemsHovered[i].name == "MarketStand(Clone)"){
                itemsHovered.RemoveAt(i);
            }
        }

        if (itemsHovered.Count <= 0) return;

        GameObject SelectedItem = itemsHovered[0];

        
        if(SelectedItem.name == "deskbell"){

            gameController.MoveCameraTo(gameController.DiceView,Vector3.zero,GameController.currentStage.DiceTray);
            StartCoroutine(mapEvents.EventEnded());
            
        }
    }
    
}
