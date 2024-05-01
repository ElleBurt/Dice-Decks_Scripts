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

    GameController gameController;

    void Start(){
        gameController = FindObjectOfType<GameController>();
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
        if(selected){
            Debug.Log(Regex.Replace(transform.GetChild(0).GetComponent<TMP_Text>().text,@"\D",""));

            gameController.UpdateMoney( int.Parse(Regex.Replace(transform.GetChild(0).GetComponent<TMP_Text>().text, @"\D","")) , false);

            transform.parent.parent.GetComponent<DiceDisplay>().Selected = false;
            transform.parent.parent.GetComponent<DiceDisplay>().MouseOver = false;
            
            Destroy(transform.parent.parent.GetComponent<DiceDisplay>().openDesc);
            Destroy(transform.parent.parent.GetChild(0).gameObject, 0.3f);
        }
    }
}
