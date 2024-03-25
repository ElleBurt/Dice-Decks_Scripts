using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DiceBoxHover : MonoBehaviour
{
    public bool animFin = false;
    DiceRoller diceRoller;
    private bool hovered = false;

    void Awake(){
        diceRoller = FindObjectOfType<DiceRoller>();
    }
    private void OnMouseEnter() {
        if(animFin){
            transform.GetChild(0).position = transform.position + new Vector3(0, 4, 0);
            hovered = true;
        }
        
    }
    private void OnMouseExit() {
        if(animFin){
            transform.GetChild(0).position = transform.position + new Vector3(0, 2, 0);
            hovered = false;
        }
    }
    void Update(){
        if(hovered && Input.GetMouseButtonDown(0)){
            diceRoller.AddDice(transform.GetChild(0).GetComponent<DiceRoll>().diceTemplate);
            animFin = false;
            transform.parent.GetComponent<DiceBoxController>().closeBox(transform);
        }
    }
}
