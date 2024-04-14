using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketEventController : MonoBehaviour, EventMedium
{
    private GameController gameController;
    private MapEvents mapEvents;
    private DiceRoller diceRoller;

    public Vector3 spOffset;
    public GameObject DiceBagPrefab; //hey that has a catch to it
    public GameObject CardBoosterPrefab; 

    void Start(){
        mapEvents = FindObjectOfType<MapEvents>();
    }

    public void ExecuteEvent(){

        gameController = FindObjectOfType<GameController>();
        diceRoller = FindObjectOfType<DiceRoller>();

        foreach(DiceTemplate template in diceRoller.DiceBlueprints){
            if(gameController.diceWeights.ContainsKey(template.itemRarity)){
                gameController.diceWeights[template.itemRarity].Add(template);
            }else{
                gameController.diceWeights.Add(template.itemRarity, new List<DiceTemplate>{template});
            }
        }

        foreach(CardTemplate template in gameController.CardTemplates){
            if(gameController.cardWeights.ContainsKey(template.itemRarity)){
                gameController.cardWeights[template.itemRarity].Add(template);
            }else{
                gameController.cardWeights.Add(template.itemRarity, new List<CardTemplate>{template});
            }
        }

        SpawnDice();
        SpawnCards();
        SpawnBoosters();
        StartCoroutine(gameController.MoveCameraTo(transform.Find("MarketTableView")));
    }

    private void SpawnDice(){

        for(int i = 1; i < 5; i++){

            //gets the relative spawnPos for the dice
            Transform spawn = transform.Find($"DicePos{i}_buy");

            int baseRarityPerc = Mathf.Clamp(Mathf.CeilToInt(Mathf.Pow(gameController.currentRound,2) / Random.Range(1.2f,1.5f)),1,101);
            int maxRarityPerc = Mathf.Clamp(Mathf.CeilToInt(Mathf.Pow(gameController.currentRound,2)),1,101);

            //1-100 
            int rarityPercent = Random.Range(baseRarityPerc,maxRarityPerc);

            //gets the rarity by comparing the rarityPercent to the list of weights for the current round
            Rarity rarity = Rarity.Common;

            foreach(KeyValuePair<Rarity, int> kvp in gameController.roundWeights){
                if(rarityPercent > kvp.Value){
                    rarity = kvp.Key;
                    break;
                }
            }

            int diceIndex = Random.Range(0,gameController.diceWeights[rarity].Count);
            //gets a random dice template from the dict of rarities and templates
            DiceTemplate diceTemp = gameController.diceWeights[rarity][diceIndex];

            gameController.diceWeights[rarity].RemoveAt(diceIndex);

            GameObject Dice = GameObject.Instantiate(diceTemp.dice,spawn.position,Quaternion.identity);
            Dice.transform.SetParent(spawn);
            Dice.transform.localScale /= 2.3f;
            Dice.GetComponent<DiceRoll>().diceTemplate = diceTemp;
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

            int baseRarityPerc = Mathf.Clamp(Mathf.CeilToInt(Mathf.Pow(gameController.currentRound,2) / Random.Range(1.2f,1.5f)),1,101);
            int maxRarityPerc = Mathf.Clamp(Mathf.CeilToInt(Mathf.Pow(gameController.currentRound,2)),1,101);

            //1-100 
            int rarityPercent = Random.Range(baseRarityPerc,maxRarityPerc);

            Rarity rarity = Rarity.Common;

            foreach(KeyValuePair<Rarity, int> kvp in gameController.roundWeights){
                if(rarityPercent > kvp.Value){
                    rarity = kvp.Key;
                    break;
                }
            }

            int cardIndex = Random.Range(0,gameController.cardWeights[rarity].Count);

            CardTemplate cardTemplate = gameController.cardWeights[rarity][cardIndex];
            gameController.cardWeights[rarity].RemoveAt(cardIndex);

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


    public void ItemSelected(GameObject item){

    }
}
