using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;




public class CardController : MonoBehaviour, IPointerClickHandler
{
    public GenMapV2 genMapV2;
    public CardTemplate cardTemplate;
    public DiceRoller diceRoller;

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

    private bool clicked = false;


    //card Vals
    private int EldritchRageBonus, DefenceForceBonus, PlagueDoctorBonus, MilitaryInvestmentBonus, EconomicsBonus, StrengthRitualBonus;
    
    public ObjectState state = ObjectState.None;

    public float offsetFactor = 1.0f;

    CardHolder cardHolder;


    void Awake(){
        baseRot = transform.rotation;

        nameText = transform.GetChild(0).Find("Name").GetComponent<TMP_Text>();
        descriptionText = transform.GetChild(0).Find("Info").GetComponent<TMP_Text>();
        additionText = transform.GetChild(0).Find("Additional").GetComponent<TMP_Text>();
        multiText = transform.GetChild(0).Find("Multiplier").GetComponent<TMP_Text>();
        sellText = transform.GetChild(0).Find("Banner").GetComponent<TMP_Text>();
        img = transform.GetChild(0).Find("Image").GetComponent<Image>();
        effectImage = transform.GetChild(0).Find("Effect").GetComponent<Image>();
        cardMat = gameObject.GetComponent<MeshRenderer>().material;

        genMapV2 = FindObjectOfType<GenMapV2>();
        diceRoller = FindObjectOfType<DiceRoller>();

        cardHolder = FindObjectOfType<CardHolder>();

    }

    public void setState(ObjectState objState){
        state = objState;

        Material mat = new Material(gameObject.GetComponent<MeshRenderer>().material);

        if(state == ObjectState.Buy){
            mat.SetFloat("_Buy",1f);
        }else{
            mat.SetFloat("_Buy",0f);
        }

        gameObject.GetComponent<MeshRenderer>().material = mat;
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        List<GameObject> itemsHovered = pointerEventData.hovered;

        if (itemsHovered.Count <= 0) return;

        Debug.Log(string.Join(", ",itemsHovered));

        GameObject SelectedItem = itemsHovered[0];

        if(clicked && itemsHovered.Contains(transform.GetChild(0).Find("Banner").gameObject)){

            if(state == ObjectState.Buy && genMapV2.totalMoneyHeld >= cardTemplate.basePrice){

                genMapV2.UpdateMoney(cardTemplate.basePrice, true);
                cardHolder.CardAdded(cardTemplate);
                Destroy(gameObject,0.3f);

            }else if(state == ObjectState.Sell){

                genMapV2.UpdateMoney(SellValue, false);
                cardHolder.CardRemoved(gameObject);
            }

            

        }else if(state == ObjectState.Booster){

            transform.parent.parent.GetComponent<CardBoosterController>().cardSelected(gameObject);

        }else if(clicked){

            transform.position = basePos;

        }else{

            transform.position = basePos + new Vector3(0, 2*offsetFactor, 0);

        }

        clicked = !clicked;
        transform.GetChild(0).Find("Banner").GetComponent<CardBannerScript>().clicked = clicked;
    }

    private void FixedUpdate() {
        EldritchRageBonus = genMapV2.DiceHeld.Count * 3;
        StrengthRitualBonus = genMapV2.DiceHeld.Count * 6;
        DefenceForceBonus = genMapV2.totalHitsTaken;
        MilitaryInvestmentBonus = genMapV2.totalEnemiesKilled * 3;
        PlagueDoctorBonus = Mathf.FloorToInt(genMapV2.totalMoneyHeld / 3);
        EconomicsBonus = Mathf.FloorToInt(genMapV2.totalMoneyHeld / 4);

        switch(cardTemplate.cardType){
            case CardType.EldritchRage:
                additionText.text = $"+{EldritchRageBonus}";
                AdditionValue = EldritchRageBonus;
            break;

            case CardType.DefenceForce:
                additionText.text  = $"+{DefenceForceBonus}";
                AdditionValue = DefenceForceBonus;
            break;

            case CardType.MilitaryInvestment:
                additionText.text  = $"+{MilitaryInvestmentBonus}";
                AdditionValue = MilitaryInvestmentBonus;
            break;

            case CardType.PlagueDoctor:
            break;

            case CardType.StrengthRitual:
                additionText.text = $"+{StrengthRitualBonus}";
                AdditionValue = StrengthRitualBonus;
            break;

            case CardType.Economics:
                sellText.text = $"${cardTemplate.baseSellValue + EconomicsBonus}";
                SellValue = cardTemplate.baseSellValue + EconomicsBonus;
            break;

            default:
            break;
        }

    }

    public void BloodShardTick(bool isDown){
        BloodShardCount = isDown ? BloodShardCount - 1 : BloodShardCount + 1;
        if(BloodShardCount == 0){
            Destroy(gameObject,1f);
        }
        descriptionText.text = Regex.Replace(descriptionText.text.ToString(), @"\[[x|\d+]\]",$"[{BloodShardCount}]");

    }
   

    public void SetupCard(ObjectState state){
        
        descriptionText.text = cardTemplate.description;
        img.sprite = cardTemplate.imgOverlay;
        effectImage.sprite = Resources.Load<Sprite>($"cards/Sprites/{cardTemplate.cardClass}");

        if(state == ObjectState.Buy){
            sellText.text = $"${cardTemplate.basePrice}";
        }else{
            sellText.text = $"${cardTemplate.baseSellValue}";
        }
        
        SellValue = cardTemplate.baseSellValue;
        cardType = cardTemplate.cardType;
        nameText.text = cardType.ToString();
        cardMat.SetTexture("_EffectTex",Resources.Load<Texture2D>($"cards/Alphas/{cardTemplate.cardClass}"));


        if(cardTemplate.cardClass == CardClass.BloodShard){
            BloodShardCount = 5;
            cardMat.SetFloat("_isCursed",1f);
            descriptionText.text = Regex.Replace(descriptionText.text.ToString(), @"\[[x|\d+]\]",$"[{BloodShardCount}]");
        }

        if(cardTemplate.cardClass == CardClass.Cursed){
            cardMat.SetFloat("_isCursed",1f);
        }

        if(cardTemplate.cardClass == CardClass.Blessed){
            cardMat.SetFloat("_isBlessed",1f);
        }
    }

    
    //lift card when scored
    public IEnumerator ScoreCard(){
        float duration = 0.4f;
        float timeElapsed = 0f;

        while (timeElapsed < duration){
            transform.position = Vector3.Lerp(transform.position, basePos + offsetPos, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        timeElapsed = 0f;
        yield return new WaitForSeconds(1.2f);
        while (timeElapsed < duration){
            transform.position = Vector3.Lerp(transform.position, basePos, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }

}
