using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoosterBuyScript : MonoBehaviour
{
    public Color canSellColor;
    public Color cantSellColor;
    public bool selected = false;

    GameController gameController;
    BoosterHover boosterHover;
    MapEvents mapEvents;

    void Start(){
        gameController = FindObjectOfType<GameController>();
        mapEvents = FindObjectOfType<MapEvents>();
        transform.GetChild(0).GetComponent<TMP_Text>().color = cantSellColor;
    }

    public void SwapColor(){
        Color colorToUse = new Color();

        if(selected){
            colorToUse = canSellColor;
        }else{
            colorToUse = cantSellColor;
        }
       
       transform.GetChild(0).GetComponent<TMP_Text>().color = colorToUse;
    }

    private void OnMouseDown() {
        
        boosterHover = transform.parent.parent.GetComponent<BoosterHover>();
        string boosterType = boosterHover.boosterTemp.name;
        
        if(selected && gameController.MoneyHeld >= boosterHover.boosterTemp.basePrice){

            gameController.UpdateMoney(boosterHover.boosterTemp.basePrice, true);

            if(boosterType == "DiceBooster"){
                mapEvents.SpawnDiceBox(true);
            }else if(boosterType == "CardBooster"){
                mapEvents.SpawnBooster(true);
            }

            boosterHover.GetComponent<BoxCollider>().enabled = false;

            if(boosterHover.openDesc != null){
                Destroy(boosterHover.openDesc);
            }

            Destroy(boosterHover.Booster.gameObject,0.2f);
        }
    }

    
}
