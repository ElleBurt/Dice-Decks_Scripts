using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SellBoard : MonoBehaviour
{
    private bool isAtBoard = false;
    // Start is called before the first frame update
    void OnMouseEnter(){
        transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().color = new Color32((byte)255,(byte)255,(byte)255,(byte)255);
    }
    void OnMouseExit(){
        transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().color = new Color32((byte)255,(byte)255,(byte)255,(byte)50);
    }
    void OnMouseDown(){
        transform.parent.GetComponent<MarketEventController>().MoveToSellBoard(isAtBoard);
        isAtBoard = !isAtBoard;
        
    }
}
