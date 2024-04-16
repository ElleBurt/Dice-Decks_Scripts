using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardBoosterController : MonoBehaviour
{   
    GameController gameController;
    Camera mainCamera;
    float cameraMoveSpeed;
    Transform MapView;
    public Vector3 cardSelectionPos;
    public int cardsReady = 0;
    CardHolder cardHolder;

    public bool wasBrought = false;
    

    void Awake(){
        gameController = FindObjectOfType<GameController>();
        cardHolder = FindObjectOfType<CardHolder>();
        mainCamera = gameController.mainCamera;
        cameraMoveSpeed = gameController.cameraMoveSpeed;
        MapView = gameController.MapView;

        gameController.SetItemWeights();
        
        AddCards();
    } 

    public void cardSelected(GameObject selectedCard){

        
        cardHolder.CardAdded(selectedCard.GetComponent<CardController>().cardTemplate);

        for(float i = 1f; i < 4f; i++){

            Transform spawn = transform.Find($"CS{i}");

            spawn.GetChild(0).gameObject.GetComponent<CardController>().canPickup = false;
            Destroy(spawn.GetChild(0).gameObject, 0.3f); 
        }

        gameObject.GetComponent<Animator>().SetBool("ThrowPack",true);

        Destroy(gameObject, 3f);


        if(wasBrought){
            MarketEventController MEC = FindObjectOfType<MarketEventController>();
            MEC.ProcessBoosters(); 
        }else{
            gameController.RoundConclusion();
        }
        
    }

    



    private void AddCards(){

        for(float i = 1f; i < 4f; i++){

            Transform spawn = transform.Find($"CS{i}");
            (Rarity, int) values = gameController.RandomItem("Card");

            Rarity rarity = values.Item1;
            int index = values.Item2;

            CardTemplate cardTemplate = gameController.ItemWeights[rarity].Item2[index];
            gameController.ItemWeights[rarity].Item2.RemoveAt(index);

            GameObject card = GameObject.Instantiate(gameController.cardPrefab, spawn.position, Quaternion.identity * Quaternion.Euler(0,180,0));
            card.transform.SetParent(spawn);
            card.GetComponent<CardController>().cardTemplate = cardTemplate;
            card.GetComponent<CardController>().boosterCard = true;
            card.GetComponent<BoxCollider>().enabled = false;
            card.GetComponent<CardController>().SetupCard();
        }
        
    }


    
    public void enableColliders(){
        for(float i = 1f; i < 4f; i++){
            Transform spawn = transform.Find($"CS{i}");
            spawn.GetChild(0).GetComponent<CardController>().basePos = spawn.position;
            
            spawn.GetComponent<CardHover>().boosterCard = true;
            spawn.GetComponent<CardHover>().Setup();
            spawn.GetComponent<BoxCollider>().enabled = true;
        }
    }

   
}
