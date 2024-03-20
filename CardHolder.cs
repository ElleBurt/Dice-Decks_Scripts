using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class CardHolder : MonoBehaviour
{
    public List<CardTemplate> Templates = new List<CardTemplate>();

    public List<GameObject> CardsHeld = new List<GameObject>();

    //basics
    private Vector3 spawnPoint;
    public TMP_Text CardCount;
    public int maxCards;

    public GameObject CardPrefab;

    
    void Start()
    {   //sets card count to however many are held
        CardCount.text = CardsHeld.Count.ToString() + "/" + maxCards.ToString();
        spawnPoint = gameObject.transform.position;
    }

    void Update(){
        if(Input.GetMouseButtonDown(1)){
            CardAdded(Templates[Random.Range(0, Templates.Count)]);
        }
    }

    //called when card added to see if too many or not, also make new card and show it and edit text display of how many in hand
    public void CardAdded(CardTemplate cardTemplate){

        if(CardsHeld.Count < maxCards){
            GameObject card = GameObject.Instantiate(CardPrefab, spawnPoint, Quaternion.identity * Quaternion.Euler(-15,-185,0));
            card.GetComponent<CardController>().cardTemplate = cardTemplate;
            card.GetComponent<CardController>().SetupCard();
            card.transform.SetParent(transform.GetChild(CardsHeld.Count));
            CardsHeld.Add(card);
            CardsUpdated();
        }
    }

    void CardRemoved(){
        //not done yet :( neglected method
    }

    //move cards according to when they should be with new values
    void CardsUpdated(){
        CardCount.text = CardsHeld.Count.ToString() + "/" + maxCards.ToString();
        int cardNum = 0;
        foreach(GameObject card in CardsHeld){

            Vector3 newPos = new Vector3(spawnPoint.x + (cardNum * card.GetComponent<CardController>().cardTemplate.width), spawnPoint.y, spawnPoint.z);
            card.transform.position = newPos;
            card.transform.rotation = Quaternion.Euler(-15,-185,0);
            card.GetComponent<CardController>().basePos = card.transform.position;
            cardNum++;
        }
    }
    

    
}



