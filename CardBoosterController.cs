using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardBoosterController : MonoBehaviour
{   
    private GenMapV2 genMapV2;
    Camera mainCamera;
    Transform MapView;
    public Vector3 cardSelectionPos;
    public int cardsReady = 0;
    CardHolder cardHolder;


    public bool fromMarket = false;

    void Awake(){
        genMapV2 = FindObjectOfType<GenMapV2>();
        cardHolder = FindObjectOfType<CardHolder>();
        mainCamera = genMapV2.mainCamera;
        MapView = genMapV2.MapView;
        
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
            genMapV2.MoveCameraTo(GameObject.Find("MarketTableView").transform,false);
        }else{
            genMapV2.RoundConclusion();
        }
        
        
        
    }

    



    private void AddCards(){
        int spawnIndex = 1;
        foreach(CardTemplate cardTemplate in genMapV2.RandomCards(3)){

            Transform spawn = transform.Find($"CS{spawnIndex}");

            GameObject card = GameObject.Instantiate(Resources.Load<GameObject>("Cards/Prefabs/CardBase"), spawn.position, Quaternion.identity * Quaternion.Euler(0,180,0));
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
