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
    public CardTemplate cardTemplate;
    public GameController controller;


    [Range(0f, 1f)]
    public float scaleFactor = 1.0f;
    [Range(0f, 1f)]
    public float offsetFactor = 1.0f;
    [Range(0f, 1f)]
    public float offsetDescFactor = 1.0f;

    

    public bool clicked = false;

    public ObjectState state = ObjectState.None;


    private void Awake(){
        controller = FindObjectOfType<GameController>();
    }

    public void setState(ObjectState objState){
        state = objState;

        Material mat = new Material(transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material);

        if(state == ObjectState.Buy){
            mat.SetFloat("_Buy",1f);
        }else{
            mat.SetFloat("_Buy",0f);
        }

        transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = mat;
    }

    

    public void OnMouseDown(){
        
        if(state == ObjectState.Booster){

            transform.parent.GetComponent<CardBoosterController>().cardSelected(transform.GetChild(0).gameObject);

        }else if(clicked){
            transform.GetChild(0).position = transform.position;
        }else{
            transform.GetChild(0).position = transform.position + new Vector3(0, 2*offsetFactor, 0);
        }

        clicked = !clicked;
        transform.GetChild(0).GetChild(0).Find("Banner").GetComponent<CardBannerScript>().clicked = clicked;
        
    }

    
}
