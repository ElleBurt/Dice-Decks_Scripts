using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class CardHolder : MonoBehaviour
{
    

    //public List<GameObject> CardsHeld = new List<GameObject>();

    //basics
    private Vector3 spawnPoint;
    public TMP_Text CardCount;
    public int maxCards;
    public float cardWidth;
    GameController gameController;

    public GameObject CardPrefab;

    
    void Start()
    {   //sets card count to however many are held
        gameController = FindObjectOfType<GameController>();
        CardCount.text = gameController.CardsHeld.Count.ToString() + "/" + maxCards.ToString();
        spawnPoint = gameObject.transform.position;
    }


    //called when card added to see if too many or not, also make new card and show it and edit text display of how many in hand
    public void CardAdded(CardTemplate cardTemplate){

        if(gameController.CardsHeld.Count < maxCards){
            GameObject card = GameObject.Instantiate(CardPrefab, spawnPoint, Quaternion.identity * Quaternion.Euler(-15,-185,0));
            card.GetComponent<CardController>().cardTemplate = cardTemplate;
            card.GetComponent<CardController>().SetupCard();
            card.transform.SetParent(transform);
            gameController.CardsHeld.Add(card);
            CardsUpdated();
        }
    }

    void CardRemoved(){
        //not done yet :( neglected method
    }

    //move cards according to when they should be with new values
    void CardsUpdated(){
        CardCount.text = gameController.CardsHeld.Count.ToString() + "/" + maxCards.ToString();
        int cardNum = 0;
        foreach(GameObject card in gameController.CardsHeld){

            Vector3 newPos = new Vector3(spawnPoint.x + (cardNum * cardWidth), spawnPoint.y, spawnPoint.z);
            card.transform.position = newPos;
            card.transform.rotation = Quaternion.Euler(-15,-185,0);
            card.GetComponent<CardController>().basePos = card.transform.position;
            cardNum++;
        }
    }
    

    
}



