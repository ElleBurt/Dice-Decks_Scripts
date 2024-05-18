using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class DiceHeldInfoScript : MonoBehaviour
{
    public Color canSellColor;
    public Color cantSellColor;
    public bool selected = false;

    private GenMapV2 genMapV2;

    DiceDisplay diceDisplay;

    private GameObject dice;

    void Start(){
        genMapV2 = FindObjectOfType<GenMapV2>();
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
        
        diceDisplay = transform.parent.parent.GetComponent<DiceDisplay>();
        dice = diceDisplay.dice.gameObject;
        
        if(selected){

            switch(diceDisplay.state){

                case ObjectState.Buy:
                    foreach(Transform slot in GameObject.Find("diceDisplay").transform){
                        if(slot.childCount == 0){
                            slot.GetComponent<DiceDisplay>().DiceAdded(dice, ObjectState.Sell);
                            genMapV2.UpdateMoney(dice.GetComponent<DiceRoll>().diceTemplate.basePrice, true);
                            genMapV2.DiceHeld.Add(dice);
                            diceDisplay.dice.gameObject.GetComponent<MeshCollider>().enabled = true;
                            diceDisplay.dice = null;
                            if(diceDisplay.openDesc != null){
                                Destroy(diceDisplay.openDesc);
                            }
                            break;
                        }
                    }

                break;

                case ObjectState.Sell:

                    genMapV2.UpdateMoney( int.Parse(Regex.Replace(transform.GetChild(0).GetComponent<TMP_Text>().text, @"\D","")) , false);

                    diceDisplay.Selected = false;
                    diceDisplay.MouseOver = false;

                    genMapV2.DiceHeld.Remove(diceDisplay.dice.gameObject);
                    
                    Destroy(diceDisplay.openDesc);
                    Destroy(dice, 0.3f);

                break;

                default:
                break;
            }   
            

            
        }
    }

    
}
