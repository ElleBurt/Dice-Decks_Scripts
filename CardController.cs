using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;




public class CardController : MonoBehaviour
{
    public GameController gameController;
    public CardTemplate cardTemplate;

    [Header("Card Values")]
    public int SellValue;
    public int Multiplier;
    public int AdditionValue;
    public int cardSpeed;
    public bool CardTriggered;
    public CardType cardType;
    public int BloodShardCount = 0;

    public Vector3 basePos;
    private Quaternion baseRot;
    private Vector3 offsetPos = new Vector3(0f, 1.5f, -1f);

    private bool entered = false;

    public TMP_Text nameText;
    public TMP_Text descriptionText;

    public TMP_Text additionText;
    public TMP_Text multiText;
    public TMP_Text sellText;
    private Image img; 
    private Image effectImage;
    private Material cardMat;

    private bool isMoving = false;
    private bool atTop = false;

    public bool canPickup = false;
    public bool boosterCard = false;


    //card Vals
    private int EldritchRageBonus, DefenceForceBonus, PlagueDoctorBonus, MilitaryInvestmentBonus, EconomicsBonus;
    
    


    void Awake(){
        baseRot = transform.rotation;

        nameText = transform.GetChild(0).Find("Name").GetComponent<TMP_Text>();
        descriptionText = transform.GetChild(0).Find("Info").GetComponent<TMP_Text>();
        additionText = transform.GetChild(0).Find("Additional").GetComponent<TMP_Text>();
        multiText = transform.GetChild(0).Find("Multiplier").GetComponent<TMP_Text>();
        sellText = transform.GetChild(0).Find("Sell").GetComponent<TMP_Text>();
        img = transform.GetChild(0).Find("Image").GetComponent<Image>();
        effectImage = transform.GetChild(0).Find("Effect").GetComponent<Image>();
        cardMat = gameObject.GetComponent<MeshRenderer>().material;

        gameController = FindObjectOfType<GameController>();

    }

    private void FixedUpdate() {
        EldritchRageBonus = gameController.diceResults.Count * 3;
        DefenceForceBonus = gameController.HitsTaken;
        MilitaryInvestmentBonus = gameController.EnemiesKilled * 3;
        PlagueDoctorBonus = Mathf.FloorToInt(gameController.MoneyHeld / 3);
        EconomicsBonus = Mathf.FloorToInt(gameController.MoneyHeld / 4);

        switch(cardTemplate.cardType){
            case CardType.EldritchRage:
                additionText.text = $"+{EldritchRageBonus}";
            break;

            case CardType.DefenceForce:
                additionText.text  = $"+{DefenceForceBonus}";
            break;

            case CardType.MilitaryInvestment:
                additionText.text  = $"+{MilitaryInvestmentBonus}";
            break;

            case CardType.PlagueDoctor:
            break;

            case CardType.Economics:
                sellText.text = $"${cardTemplate.baseSellValue + EconomicsBonus}";
            break;

            default:
            break;
        }

    }

   

    public void SetupCard(){
        
        descriptionText.text = cardTemplate.description;
        img.sprite = cardTemplate.imgOverlay;
        effectImage.sprite = Resources.Load<Sprite>($"cards/Sprites/{cardTemplate.cardClass}");
        sellText.text = $"${cardTemplate.baseSellValue}";
        SellValue = cardTemplate.baseSellValue;
        cardType = cardTemplate.cardType;
        nameText.text = cardType.ToString();
        cardMat.SetTexture("_EffectTex",Resources.Load<Texture2D>($"cards/Alphas/{cardTemplate.cardClass}"));


        if(cardTemplate.cardClass == CardClass.BloodShard){
            BloodShardCount = 5;
            cardMat.SetFloat("_isCursed",1f);
        }

        if(cardTemplate.cardClass == CardClass.Cursed){
            cardMat.SetFloat("_isCursed",1f);
        }

        if(cardTemplate.cardClass == CardClass.Blessed){
            cardMat.SetFloat("_isBlessed",1f);
        }
    }

    //move card up or down on hover or exit also tilt depending on mouse distance from center of card
    void OnMouseOver(){
        if(!isMoving && !entered){
            entered = true;
        }
        
    }
    void OnMouseExit(){
        entered = false;
        if(!canPickup){
            transform.rotation = baseRot;
        }else{
            transform.rotation = Quaternion.Euler(-30,180,0);
        }
    }

    void Update(){
        if(boosterCard && !canPickup){
            gameObject.GetComponent<BoxCollider>().enabled = false;
        }else if(boosterCard && canPickup && !entered){
            gameObject.GetComponent<BoxCollider>().enabled = true;
        }else if(boosterCard && canPickup && entered && Input.GetMouseButtonDown(0)){
            transform.parent.parent.GetComponent<CardBoosterController>().cardSelected(gameObject);
        }else if(boosterCard && canPickup && entered){
            baseRot = Quaternion.Euler(-30,180,0);
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z));

            Vector3 cardPos = transform.position;

            Vector3 mouseDif = mousePos - cardPos;

            mouseDif *= 1.5f;
        
            Quaternion newRot = Quaternion.Euler(baseRot.eulerAngles.x + -mouseDif.y , baseRot.eulerAngles.y + -mouseDif.x, baseRot.eulerAngles.z);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRot, 15f * Time.deltaTime);
        }

        if(!atTop && !isMoving && entered && Input.GetMouseButtonDown(0)){
            StartCoroutine(Hovered());
            gameObject.GetComponent<AudioSource>().Play();
        }   
        if(atTop && !isMoving && entered && Input.GetMouseButtonDown(0)){
            StartCoroutine(Return());
            transform.rotation = baseRot;
            gameObject.GetComponent<AudioSource>().Play();
        }
        if(atTop && !isMoving && entered){
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(transform.position).z));

            Vector3 cardPos = transform.position;

            Vector3 mouseDif = mousePos - cardPos;

            mouseDif *= 2.5f;
        
            Quaternion newRot = Quaternion.Euler(baseRot.eulerAngles.x + -mouseDif.y , baseRot.eulerAngles.y + -mouseDif.x, baseRot.eulerAngles.z);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRot, 15f * Time.deltaTime); 
        }
        
        
            
        
    }

    IEnumerator Hovered(){
        while(Vector3.Distance(transform.position, basePos+offsetPos) > 0.01f){
            transform.position = Vector3.Lerp(transform.position, basePos+offsetPos, cardSpeed * Time.deltaTime);
            isMoving = true;
            yield return null;
        } 
        atTop = true;
        isMoving = false;
    }

    IEnumerator Return(){
        while(Vector3.Distance(transform.position, basePos) > 0.01f){
            transform.position = Vector3.Lerp(transform.position, basePos, cardSpeed * Time.deltaTime);
            isMoving = true;
            yield return null;
        } 
        atTop = false;
        isMoving = false;
    }

    //lift card when scored
    public IEnumerator ScoreCard(){
        while (Vector3.Distance(transform.position,basePos + offsetPos) > 0.01f){
            transform.position = Vector3.Lerp(transform.position, basePos + offsetPos, cardSpeed * Time.deltaTime);
            isMoving = true;
        }
        yield return new WaitForSeconds(1f);
        while (Vector3.Distance(transform.position,basePos) > 0.01f){
            transform.position = Vector3.Lerp(transform.position, basePos, cardSpeed * Time.deltaTime);
            isMoving = true;
        }
        isMoving = false;
    }

    public void cardDestroyed(){
        Destroy(gameObject,1);
    }
}
