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
    private GenMapV2 genMapV2;

    public GameObject CardPrefab;

    
    void Start()
    {   //sets card count to however many are held
        genMapV2 = FindObjectOfType<GenMapV2>();
        CardCount.text = genMapV2.CardsHeld.Count.ToString() + "/" + maxCards.ToString();
        spawnPoint = gameObject.transform.position;
    }


    //called when card added to see if too many or not, also make new card and show it and edit text display of how many in hand
    public void CardAdded(CardTemplate cardTemplate){

        if(genMapV2.CardsHeld.Count < maxCards){
            GameObject card = GameObject.Instantiate(CardPrefab, spawnPoint, Quaternion.identity * Quaternion.Euler(-15,-185,0));
            card.GetComponent<CardController>().cardTemplate = cardTemplate;
            card.GetComponent<CardController>().SetupCard(ObjectState.Sell);
            card.GetComponent<CardController>().setState(ObjectState.Sell);
            card.transform.SetParent(transform);
            genMapV2.CardsHeld.Add(card);
            CardsUpdated();
        }
    }

    public void CardRemoved(GameObject card){
        genMapV2.CardsHeld.Remove(card);
        Destroy(card,0.3f);
        CardsUpdated();
    }

    //move cards according to when they should be with new values
    void CardsUpdated(){
        CardCount.text = genMapV2.CardsHeld.Count.ToString() + "/" + maxCards.ToString();
        int cardNum = 0;
        foreach(GameObject card in genMapV2.CardsHeld){

            Vector3 newPos = new Vector3(spawnPoint.x + (cardNum * cardWidth), spawnPoint.y, spawnPoint.z);
            card.transform.position = newPos;
            card.transform.rotation = Quaternion.Euler(-15,-185,0);
            card.GetComponent<CardController>().basePos = card.transform.position;
            cardNum++;
        }
    }
    

    
}



