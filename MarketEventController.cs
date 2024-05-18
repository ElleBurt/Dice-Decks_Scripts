using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class MarketEventController : MonoBehaviour, EventMedium, IPointerClickHandler
{
    private GenMapV2 genMapV2;
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
        genMapV2 = FindObjectOfType<GenMapV2>();
        diceRoller = FindObjectOfType<DiceRoller>();
        cardHolder = FindObjectOfType<CardHolder>();
    }

    public void ExecuteEvent(){
        SpawnDice();
        SpawnCards();
        SpawnBoosters();
        genMapV2.MoveCameraTo(transform.Find("MarketTableView"),false);
    }

    private void SpawnDice(){

        int spawnIndex = 1;
        foreach(DiceTemplate diceTemplate in genMapV2.RandomDice(3)){

            Transform spawn = transform.Find($"DS{spawnIndex}");

            GameObject Dice = GameObject.Instantiate(diceTemplate.dice,spawn.position ,Quaternion.identity);
            Dice.transform.SetParent(spawn);
            Dice.GetComponent<DiceRoll>().diceTemplate = diceTemplate;
            Rigidbody rb = Dice.GetComponent<Rigidbody>();
            Dice.transform.rotation = new Quaternion(0,0,0,0);
            rb.isKinematic = true;
            spawn.GetComponent<DiceDisplay>().DiceAdded(Dice, ObjectState.Buy);

            spawnIndex++;
        }

        
    }

    private void SpawnCards(){
        int spawnIndex = 1;
        foreach(CardTemplate cardTemplate in genMapV2.RandomCards(3)){

            Transform spawn = transform.Find($"CS{spawnIndex}");

            GameObject card = GameObject.Instantiate(Resources.Load<GameObject>("Cards/Prefabs/CardBase"), spawn.position, Quaternion.identity * Quaternion.Euler(0,180,0));
            card.transform.SetParent(spawn);
            card.GetComponent<CardController>().cardTemplate = cardTemplate;
            card.GetComponent<CardController>().basePos = spawn.position;
            card.GetComponent<BoxCollider>().enabled = false;
            card.GetComponent<CardController>().SetupCard(ObjectState.Buy);
            card.GetComponent<CardController>().setState(ObjectState.Buy);

            spawnIndex++;
        }
        
    }

    private void SpawnBoosters(){
        
        int spawnIndex = 1;
        foreach(BoosterTemplate boosterTemplate in genMapV2.RandomBooster(2)){

            Transform spawn = transform.Find($"BS{spawnIndex}");
            spawn.GetComponent<BoosterHover>().boosterTemp = boosterTemplate;

            GameObject booster = GameObject.Instantiate(boosterTemplate.BoosterPrefab, spawn.position + boosterTemplate.positionOffset, Quaternion.Euler(290,0,90));
            booster.transform.SetParent(spawn);
            spawn.GetComponent<BoosterHover>().Booster = booster.transform;
            spawn.GetComponent<BoosterHover>().baseRot = booster.transform.rotation;

            spawnIndex++;
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

            genMapV2.MoveCameraTo(genMapV2.DiceView,false);
            StartCoroutine(mapEvents.EventEnded());
            
        }
    }
    
}
