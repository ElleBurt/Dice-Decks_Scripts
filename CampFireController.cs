using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class CampFireController : MonoBehaviour, EventMedium
{   
    private GameController gameController;
    private MapEvents mapEvents;
    public int healAmount;

    void Start(){
        gameController = FindObjectOfType<GameController>();
        mapEvents = FindObjectOfType<MapEvents>();
    }

    public void ExecuteEvent(){
        gameController.IncreaseMaxHealth(healAmount);
        StartCoroutine(mapEvents.EventEnded());
    }
}
