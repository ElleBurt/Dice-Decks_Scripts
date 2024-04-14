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
    

    void Awake(){
        gameController = FindObjectOfType<GameController>();
        cardHolder = FindObjectOfType<CardHolder>();
        mainCamera = gameController.mainCamera;
        cameraMoveSpeed = gameController.cameraMoveSpeed;
        MapView = gameController.MapView;

        foreach(CardTemplate template in gameController.CardTemplates){
            if(gameController.cardWeights.ContainsKey(template.itemRarity)){
                gameController.cardWeights[template.itemRarity].Add(template);
            }else{
                gameController.cardWeights.Add(template.itemRarity, new List<CardTemplate>{template});
            }
        }
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

        gameController.RoundConclusion();
    }

    public void OpenSequence(){
        StartCoroutine(gameController.MoveCameraTo(gameController.DiceView));
        AddCards();
        
    }



    private void AddCards(){

        for(float i = 1f; i < 4f; i++){

            Transform spawn = transform.Find($"CS{i}");
            spawn.GetComponent<BoxCollider>().enabled = false;

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
