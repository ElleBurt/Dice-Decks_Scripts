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

    GenMapV2 genMapV2;

    public bool fromMarket = false;

    void Awake(){
        gameController = FindObjectOfType<GameController>();
        genMapV2 = FindObjectOfType<GenMapV2>();
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



        if(fromMarket){
            gameController.MoveCameraTo(GameObject.Find("MarketTableView").transform,Vector3.zero,GameController.currentStage.Market);
        }else{
            gameController.RoundConclusion();
        }
        
        
        
    }

    



    private void AddCards(){
        int spawnIndex = 1;
        foreach(CardTemplate cardTemplate in genMapV2.RandomCards(3)){

            Transform spawn = transform.Find($"CS{spawnIndex}");

            GameObject card = GameObject.Instantiate(gameController.cardPrefab, spawn.position, Quaternion.identity * Quaternion.Euler(0,180,0));
            card.transform.SetParent(spawn);
            card.GetComponent<CardController>().cardTemplate = cardTemplate;
            card.GetComponent<CardController>().boosterCard = true;
            card.GetComponent<BoxCollider>().enabled = false;
            card.GetComponent<CardController>().SetupCard(ObjectState.Booster);

            spawnIndex++;
        }
    }


    
    public void enableColliders(){
        for(float i = 1f; i < 4f; i++){
            Transform spawn = transform.Find($"CS{i}");
            spawn.GetChild(0).GetComponent<CardController>().basePos = spawn.position;
            
            spawn.GetChild(0).GetComponent<CardController>().setState(ObjectState.Booster);
        }
    }

   
}
