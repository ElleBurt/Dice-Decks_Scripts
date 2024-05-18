using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CardBannerScript : MonoBehaviour
{
    CardHover cardHover;
    private Transform Card;
    private CardController cardController;
    private GenMapV2 genMapV2;

    public bool clicked = false;

    void Awake(){
        Card = transform.parent.parent; 
        cardController = Card.GetComponent<CardController>();
        genMapV2 = FindObjectOfType<GenMapV2>();
        
        
    }
    // Start is called before the first frame update
    void OnMouseDown(){
        cardHover = Card.parent.GetComponent<CardHover>();
        if(clicked){
            if(cardHover.state == ObjectState.Buy && genMapV2.totalMoneyHeld >= cardController.cardTemplate.basePrice){
                genMapV2.UpdateMoney(cardController.cardTemplate.basePrice, true);
            }else if(cardHover.state == ObjectState.Sell){
                genMapV2.UpdateMoney(cardController.SellValue, false);
            }
        }

    }
}
