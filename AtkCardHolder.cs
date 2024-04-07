using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//atk card struct
[System.Serializable]
public class AtkCardTypes{
    public string CardName;
    public Sprite OverlayTexture;
    public GameObject CardBase;
    public Material Outline;
    public GameObject Weapon;
}

public class AtkCardHolder : MonoBehaviour
{
    DiceRoller diceRoller;
    Score score;

    public int maxCards;
    private int cardCount;
    public Transform activeAtkPos;

    public int cardSpeed;

    public AtkCardTypes ActiveCard;

    public List<AtkCardTypes> Cards = new List<AtkCardTypes>();
    public List<AtkCardTypes> CardsInHolder = new List<AtkCardTypes>();

    public bool lastCard = false;

    void Start()
    {   //spawn cards in
        diceRoller = FindObjectOfType<DiceRoller>();
        score = FindObjectOfType<Score>();
        
    }

    //adds the cards to the holder
    public void AddCard(AtkCardTypes card){
        cardCount++;
        Vector3 position = transform.position;

        AtkCardTypes newCard = new AtkCardTypes();

        newCard.CardBase = GameObject.Instantiate(card.CardBase, position, Quaternion.identity);

        Image img = newCard.CardBase.transform.Find("Canvas").Find("Icon").GetComponent<Image>();
        TMP_Text atkValue = newCard.CardBase.transform.Find("Canvas").Find("Attack").GetComponent<TMP_Text>();

        img.sprite = card.OverlayTexture;

        newCard.CardBase.transform.rotation = Quaternion.Euler(19,-90,90);

        newCard.Weapon = card.Weapon;
        CardsInHolder.Add(newCard);
        
        Material[] Materials = newCard.CardBase.GetComponent<MeshRenderer>().materials;

        Material Outline = Materials[1];

        Outline.SetFloat("_OutlineOn", 0);


        UpdateCards(newCard);
    }
    
    //edits position of the card depending on how many cards
    void UpdateCards(AtkCardTypes card){

        Vector3 cardOffset = new Vector3(0.2f*CardsInHolder.Count,0f,0f);
        
        card.CardBase.transform.position = card.CardBase.transform.position + cardOffset;


    }

    //moves card to scoring position, plays the "tsh" sound, removes from the list
    public void DrawCard(){
        if(CardsInHolder.Count > 0){
            lastCard = false;

            if(CardsInHolder.Count == 1){
                lastCard = true;
            }

            AtkCardTypes card = CardsInHolder[CardsInHolder.Count - 1];
            StartCoroutine(MoveCard(card));
            ActiveCard = card;
            CardsInHolder.RemoveAt(CardsInHolder.Count - 1);
            gameObject.GetComponent<AudioSource>().Play();
            
        }   
        
    }

    public void ReplenishCards(){
        foreach(AtkCardTypes card in CardsInHolder){
            Destroy(card.CardBase);
        }
        CardsInHolder.Clear();
        for(int i = 0; i < maxCards; i++){
            AddCard(Cards[0]);
            
        }
    }

    //moves the card but turns on outline changes the alpha dependent on distance from destination
    IEnumerator MoveCard(AtkCardTypes card){
        Material[] Materials = card.CardBase.GetComponent<MeshRenderer>().materials;

        Material Outline = Materials[1];

        Outline.SetFloat("_OutlineOn", 1);
        Outline.SetFloat("_Alpha",0);


        while(Vector3.Distance(card.CardBase.transform.position, activeAtkPos.position) > 0.01f){
            float dist = Mathf.Clamp01(Vector3.Distance(card.CardBase.transform.position, transform.position));

            Outline.SetFloat("_Alpha",dist);

            card.CardBase.transform.position = Vector3.Lerp(card.CardBase.transform.position, activeAtkPos.position, cardSpeed * Time.deltaTime);
            card.CardBase.transform.rotation = Quaternion.Slerp(card.CardBase.transform.rotation ,Quaternion.Euler(0,180,0), cardSpeed * Time.deltaTime);
            yield return null;
        }
        card.CardBase.transform.Find("Canvas").Find("Attack").GetComponent<TMP_Text>().text = "0";
        
    }
}

    
