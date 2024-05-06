using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CardBannerScript : MonoBehaviour
{
    CardHover cardHover;
    private Transform Card;
    private CardController cardController;
    private GameController gameController;

    public bool clicked = false;

    void Awake(){
        Card = transform.parent.parent; 
        cardController = Card.GetComponent<CardController>();
        gameController = FindObjectOfType<GameController>();
        
        
    }
    // Start is called before the first frame update
    void OnMouseDown(){
        cardHover = Card.parent.GetComponent<CardHover>();
        if(clicked){
            if(cardHover.state == ObjectState.Buy && gameController.MoneyHeld >= cardController.cardTemplate.basePrice){
                gameController.UpdateMoney(cardController.cardTemplate.basePrice, true);
            }else if(cardHover.state == ObjectState.Sell){
                gameController.UpdateMoney(cardController.SellValue, false);
            }
        }

    }
}
