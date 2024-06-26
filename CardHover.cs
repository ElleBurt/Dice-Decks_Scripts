using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class CardHover : MonoBehaviour, GetValues
{   

    public GameObject ItemDesc;
    private GameObject openDesc;
    public CardTemplate cardTemplate;

    [Range(0f, 1f)]
    public float scaleFactor = 1.0f;
    [Range(0f, 1f)]
    public float offsetFactor = 1.0f;
    [Range(0f, 1f)]
    public float offsetDescFactor = 1.0f;

    public bool boosterCard = false;
    bool isAwake = false;
    public bool inSellBox = false;

    public MarketStage currentStage = MarketStage.None;

    public void OnMouseDown()
    {
        if(boosterCard && isAwake){
            transform.parent.GetComponent<CardBoosterController>().cardSelected(transform.GetChild(0).gameObject);
            transform.GetComponent<BoxCollider>().enabled = false;
        }
        
    }

    public MarketStage GetStage(){
        return currentStage;
    }

    public void SetStage(MarketStage stage){
        currentStage = stage;
    }

    public Dictionary<string, float> GetValuesAvailable(){
        return new Dictionary<string, float> {{"Buy", cardTemplate.basePrice},{"Sell", transform.GetChild(0).gameObject.GetComponent<CardController>().SellValue}};
    }
    
    public void Setup(){
        isAwake = true;
    }

    void OnMouseEnter(){
        
        if(transform.childCount > 0){
            transform.GetChild(0).position = transform.position + new Vector3(0, 2*offsetFactor, 0);

            if(openDesc == null && !boosterCard){
                
                cardTemplate = transform.GetChild(0).gameObject.GetComponent<CardController>().cardTemplate;
                GameObject Desc = GameObject.Instantiate(ItemDesc,(transform.position + new Vector3(0,5.5f*offsetDescFactor,0)),Quaternion.Euler(Vector3.zero));

                Desc.transform.localScale *= scaleFactor;

                Desc.transform.Find("Desc").GetComponent<TMP_Text>().text = $"{cardTemplate.name}\n{cardTemplate.description}";
                Desc.transform.SetParent(transform);
                Desc.transform.Find("Price").GetComponent<TMP_Text>().text = $"Buy: ${cardTemplate.basePrice}";

                
                Desc.transform.localScale *= 1.2f;   
                openDesc = Desc;
                
                openDesc.transform.LookAt(Camera.main.transform.position);
                openDesc.transform.rotation *= Quaternion.Euler(0,180,0);
            }
        }
       
    }
    void OnMouseExit(){
        if(transform.childCount > 0 && !inSellBox){
            transform.GetChild(0).position = transform.position;
        }
        if(openDesc != null){
            Destroy(openDesc);
        }
        
    }
}
