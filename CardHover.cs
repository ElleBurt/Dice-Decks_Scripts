using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class CardHover : MonoBehaviour
{   

    public GameObject ItemDesc;
    private GameObject openDesc;

    public float scaleFactor = 1.0f;
    public float offsetFactor = 1.0f;

    public bool boosterCard = false;
    bool isAwake = false;

    public void OnMouseDown()
    {
        if(boosterCard && isAwake){
            transform.parent.GetComponent<CardBoosterController>().cardSelected(transform.GetChild(0).gameObject);
            transform.GetComponent<BoxCollider>().enabled = false;
        }
        
    }
    
    public void Setup(){
        isAwake = true;
    }

    void OnMouseEnter(){
        
        if(transform.childCount > 0){
            Vector3 pos = transform.GetChild(0).gameObject.GetComponent<CardController>().basePos;
            transform.GetChild(0).position = pos + new Vector3(0,1,0);

            if(openDesc == null){
                
                CardTemplate cardTemplate = transform.GetChild(0).gameObject.GetComponent<CardController>().cardTemplate;
                GameObject Desc = GameObject.Instantiate(ItemDesc,(transform.position + new Vector3(0,5.5f*offsetFactor,0)),Quaternion.Euler(Vector3.zero));

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
        if(transform.childCount > 0){
            Vector3 pos = transform.GetChild(0).gameObject.GetComponent<CardController>().basePos;
            transform.GetChild(0).position = pos;
        }
        if(openDesc != null){
            Destroy(openDesc);
        }
        
    }
}
